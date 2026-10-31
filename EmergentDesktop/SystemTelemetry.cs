using System;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace EmergentDesktop;

public readonly record struct TelemetrySnapshot(
    double CpuPercent, double RamPercent, double RamUsedGiB, double RamTotalGiB,
    double DownBytesPerSecond, double UpBytesPerSecond);

/// <summary>
/// Reads local, non-privileged Windows system counters. Does not inspect processes,
/// corporate applications, network destinations, or transmit any information.
/// </summary>
public sealed class SystemTelemetry
{
    [StructLayout(LayoutKind.Sequential)]
    private struct FileTime
    {
        public uint Low;
        public uint High;
        public ulong Value => ((ulong)High << 32) | Low;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatus
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhys;
        public ulong AvailPhys;
        public ulong TotalPageFile;
        public ulong AvailPageFile;
        public ulong TotalVirtual;
        public ulong AvailVirtual;
        public ulong AvailExtendedVirtual;
    }

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetSystemTimes(out FileTime idle, out FileTime kernel, out FileTime user);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatus status);

    private ulong _lastIdle;
    private ulong _lastTotal;
    private long _lastDown;
    private long _lastUp;
    private DateTime _lastNetTime;
    private bool _hasCpuSample;

    public TelemetrySnapshot Read()
    {
        double cpu = ReadCpu();
        var memory = ReadMemory();
        var network = ReadNetwork();
        return new TelemetrySnapshot(cpu, memory.load, memory.usedGiB, memory.totalGiB,
            network.down, network.up);
    }

    private double ReadCpu()
    {
        if (!GetSystemTimes(out FileTime idle, out FileTime kernel, out FileTime user)) return 0;
        ulong total = kernel.Value + user.Value;
        double result = 0;
        if (_hasCpuSample && total > _lastTotal)
        {
            ulong totalDelta = total - _lastTotal;
            ulong idleDelta = idle.Value >= _lastIdle ? idle.Value - _lastIdle : 0;
            result = 100.0 * (1.0 - (double)idleDelta / totalDelta);
        }
        _hasCpuSample = true;
        _lastTotal = total;
        _lastIdle = idle.Value;
        return Math.Clamp(result, 0, 100);
    }

    private static (double load, double usedGiB, double totalGiB) ReadMemory()
    {
        var status = new MemoryStatus { Length = (uint)Marshal.SizeOf<MemoryStatus>() };
        if (!GlobalMemoryStatusEx(ref status) || status.TotalPhys == 0) return (0, 0, 0);
        const double gib = 1024.0 * 1024 * 1024;
        return (status.MemoryLoad, (status.TotalPhys - status.AvailPhys) / gib, status.TotalPhys / gib);
    }

    private (double down, double up) ReadNetwork()
    {
        long down = 0, up = 0;
        try
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up ||
                    nic.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel)
                    continue;
                var stats = nic.GetIPv4Statistics();
                down += stats.BytesReceived;
                up += stats.BytesSent;
            }
        }
        catch (Exception ex) when (ex is NetworkInformationException or InvalidOperationException
                                    or NotSupportedException or System.ComponentModel.Win32Exception)
        {
            return (0, 0);
        }

        DateTime now = DateTime.UtcNow;
        double elapsed = (now - _lastNetTime).TotalSeconds;
        double downRate = 0, upRate = 0;
        if (_lastNetTime != default && elapsed > 0.05)
        {
            downRate = Math.Max(0, down - _lastDown) / elapsed;
            upRate = Math.Max(0, up - _lastUp) / elapsed;
        }
        _lastNetTime = now;
        _lastDown = down;
        _lastUp = up;
        return (downRate, upRate);
    }

    public static string FormatRate(double bytesPerSecond)
    {
        string[] units = ["B/s", "KiB/s", "MiB/s", "GiB/s"];
        int index = 0;
        while (bytesPerSecond >= 1024 && index < units.Length - 1)
        {
            bytesPerSecond /= 1024;
            index++;
        }
        return $"{bytesPerSecond:0.0} {units[index]}";
    }
}

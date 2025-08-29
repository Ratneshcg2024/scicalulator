using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using SCIMetricAPI.Models;
//using Microsoft.VisualBasic.Devices;

namespace SCIMetricAPI.Services
{
    public class SystemMetricsService
    {
        public SystemMetricsModel GetSystemMetrics()
        {
            return new SystemMetricsModel
            {
                ProcessorName = GetProcessorName(),
                CpuUtilizationPercent = GetCpuUsage(),
                MemoryUtilizationGB = GetMemoryUsageGB(),
                StorageType = GetStorageType(),
                TotalStorageGB = GetTotalStorageGB(),
                TotalVcpu = Environment.ProcessorCount,
                VcpuUsed = GetUsedVcpu(),
                RamCapacityGB = GetRamCapacityGB()
            };
        }

        private string GetProcessorName()
        {
            var searcher = new ManagementObjectSearcher("select * from Win32_Processor");
            foreach (ManagementObject item in searcher.Get())
            {
                return item["Name"]?.ToString() ?? "Unknown";
            }
            return "Unknown";
        }

        private double GetCpuUsage()
        {
            var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
            cpuCounter.NextValue();
            System.Threading.Thread.Sleep(1000);
            return Math.Round(cpuCounter.NextValue(), 2);
        }

      private double GetMemoryUsageGB()
{
    var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");
    foreach (ManagementObject item in searcher.Get())
    {
        double total = Convert.ToDouble(item["TotalVisibleMemorySize"]);
        double free = Convert.ToDouble(item["FreePhysicalMemory"]);
        double used = total - free;
        return Math.Round(used / 1024 / 1024, 2); // Convert KB to GB
    }
    return 0;
}


        private string GetStorageType()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "-Command \"Get-PhysicalDisk | Select-Object -ExpandProperty MediaType\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var process = Process.Start(psi);
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (output.Contains("SSD")) return "SSD";
                if (output.Contains("HDD")) return "HDD";
                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private double GetTotalStorageGB()
        {
            double totalSpace = 0;
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    totalSpace += drive.TotalSize;
                }
            }
            return Math.Round(totalSpace / (1024.0 * 1024.0 * 1024.0), 2);
        }

        private int GetUsedVcpu()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                int threadCount = process.Threads.Count;
                int logicalProcessors = Environment.ProcessorCount;
                return Math.Min((int)Math.Ceiling(threadCount / (double)logicalProcessors), logicalProcessors);
            }
            catch
            {
                return 0;
            }
        }

        private double GetRamCapacityGB()
        {
            var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem");
            foreach (ManagementObject item in searcher.Get())
            {
                double total = Convert.ToDouble(item["TotalVisibleMemorySize"]) / 1024 / 1024;
                return Math.Round(total, 2);
            }
            return 0;
        }
    }
}

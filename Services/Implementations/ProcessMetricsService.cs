using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using SCIMetricAPI.Models;
using SCIMetricAPI.Services.Interfaces;

namespace SCIMetricAPI.Services.Implementations
{
    

public class ProcessMetricsService : IProcessMetricsService
    {
        public async Task<ProcessMetrics> GetProcessMetricsAsync(string processName, int durationInMinutes)
        {
            var process = Process.GetProcessesByName(processName).FirstOrDefault();
            if (process == null) return null;
            Console.WriteLine($"Collecting metrics for process: {process.ProcessName} (ID: {process.Id})"); 

            var metrics = new ProcessMetrics();
            int intervalSeconds = 5; // Define the interval in seconds
            var samples = durationInMinutes * 12;

            for (int i = 0; i < samples; i++)
            {
                metrics.CpuUsage += await GetCpuUsageAsync(process);
                metrics.MemoryUsage += process.WorkingSet64 / (1024.0 * 1024.0);
                metrics.NetworkUsage += GetNetworkUsage(process);
                metrics.RamUsage += GetAvailableRam();
                metrics.AverageCpuUsage += await GetAverageCpuUsageAsync();
                metrics.HardDiskType = GetDiskType();
                metrics.OsArchitecture = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
                metrics.ElectricityConsumption += EstimateElectricity(process);

                metrics.CpuModel = GetCpuModel();
                metrics.CpuCores = Environment.ProcessorCount;
                metrics.CpuCoresUsed = GetProcessUsedCores(process);   
                metrics.LogicalProcessors = Environment.ProcessorCount;

                (double freeMem, double usedMem) = GetMemoryStats();
                metrics.FreeMemoryMB = freeMem;
                metrics.UsedMemoryMB = usedMem;

                // Print or log the metrics after every 5 seconds (each sample)
                Console.WriteLine($"Sample {i + 1}/{samples}: " +
                    $"CPU: {metrics.CpuUsage / (i + 1):F2}%, " +
                    $"Memory: {metrics.MemoryUsage / (i + 1):F2} MB, " +
                    $"Network: {metrics.NetworkUsage / (i + 1):F2} MB/s, " +
                    $"RAM: {metrics.RamUsage / (i + 1):F2} MB, " +
                    $"Avg CPU: {metrics.AverageCpuUsage / (i + 1):F2}%, " +
                    $"Electricity: {metrics.ElectricityConsumption / (i + 1):F4} kWh");
                Console.WriteLine($"CPU Model: {metrics.CpuModel}, Cores: {metrics.CpuCores}, Used Cores: {metrics.CpuCoresUsed}, Logical Processors: {metrics.LogicalProcessors}");
                await Task.Delay(intervalSeconds * 1000);
            }

            metrics.CpuUsage /= samples;
            metrics.MemoryUsage /= samples;
            metrics.NetworkUsage /= samples;
            metrics.RamUsage /= samples;
            metrics.AverageCpuUsage /= samples;
            metrics.ElectricityConsumption /= samples;
            return metrics;
        }
         public ProcessMetricsService()
        {
            // Call GetAllProcessNames once when the service is created
            GetAllProcessNames();
           // GetExternalProcessNames();
    
        }

        public string[] GetExternalProcessNames()
        {
            var currentUser = Environment.UserName;
            var allProcesses = Process.GetProcesses();
            var systemProcessNames = new[]
            {
                "System", "Idle", "svchost", "wininit", "csrss", "services", "lsass", "smss", "winlogon", "explorer"
            };

            var externalProcesses = allProcesses
                .Where(p =>
                {
                    try
                    {
                        // Exclude system processes and processes owned by the current user
                        var owner = GetProcessOwner(p);
                        return !systemProcessNames.Contains(p.ProcessName, StringComparer.OrdinalIgnoreCase)
                               && !string.IsNullOrEmpty(owner)
                               && !owner.Equals(currentUser, StringComparison.OrdinalIgnoreCase);
                    }
                    catch
                    {
                        return false;
                    }
                })
                .Select(p => p.ProcessName)
                .Distinct()
                .OrderBy(name => name)
                .ToArray();

            Console.WriteLine("External Application Processes:");
            foreach (var name in externalProcesses)
            {
                Console.WriteLine(name);
            }

            return externalProcesses;
        }
        private static string GetProcessOwner(Process process)
        {
            try
            {
                string query = $"SELECT * FROM Win32_Process WHERE ProcessId = {process.Id}";
                using (var searcher = new ManagementObjectSearcher(query))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        object[] args = { string.Empty, string.Empty };
                        int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", args));
                        if (returnVal == 0)
                        {
                            return args[0]?.ToString() ?? string.Empty;
                        }
                    }
                }
            }
            catch
            {
                // Ignore access denied or zombie processes
            }
            return string.Empty;
        }
        public string[] GetAllProcessNames()
        {
            var processNames = Process.GetProcesses()
                                      .Select(p => p.ProcessName)
                                      .Distinct()
                                      .OrderBy(name => name)
                                      .ToArray();

            // Print process names to the console
            Console.WriteLine($"Total Processes:{processNames.Length}");
            Console.WriteLine("Running Processes:");
            foreach (var name in processNames)
            {
                Console.WriteLine(name);
            }

            return processNames;
        }

        // Calculates the number of CPU cores utilized by the process
        private static int GetProcessUsedCores(Process process)
        {
            try
            {
                // Estimate used cores by counting the number of threads in the process
                int threadCount = process.Threads.Count;
                int logicalProcessors = Environment.ProcessorCount;
                // Approximate: threads per core (not exact, but gives a rough estimate)
                int estimatedCoresUsed = Math.Min((int)Math.Ceiling(threadCount / (double)logicalProcessors), logicalProcessors);
                return estimatedCoresUsed;
            }
            catch
            {
                return 0;
            }
        }

        // Calculates the number of samples based on duration and interval
        private static int GetSampleCount(int durationInMinutes, int intervalSeconds)
        {
            return (int)Math.Ceiling(durationInMinutes * 60.0 / intervalSeconds);
        }

        // Gets CPU usage for a process asynchronously
        private static async Task<double> GetCpuUsageAsync(Process process)
        {
            TimeSpan startCpuTime = process.TotalProcessorTime;
            DateTime startTime = DateTime.UtcNow;

            await Task.Delay(1000); // Sample interval

            TimeSpan endCpuTime = process.TotalProcessorTime;
            DateTime endTime = DateTime.UtcNow;

            double cpuUsedMs = (endCpuTime - startCpuTime).TotalMilliseconds;
            double totalMsPassed = (endTime - startTime).TotalMilliseconds;

            int processorCount = Environment.ProcessorCount;
            double cpuUsagePercent = cpuUsedMs / (totalMsPassed * processorCount) * 100;

            return Math.Round(cpuUsagePercent, 2);
        }

        private static async Task<double> GetAverageCpuUsageAsync()
        {
            var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
            cpuCounter.NextValue();
            await Task.Delay(1000);
            return Math.Round(cpuCounter.NextValue(), 2);
        }

        private static double GetNetworkUsage(Process process)
        {
            var netCounter = new PerformanceCounter("Process", "IO Data Bytes/sec", process.ProcessName, true);
            return Math.Round(netCounter.NextValue() / (1024.0 * 1024.0), 2); // MB/sec
        }

        private static double GetAvailableRam()
        {
            var ramCounter = new PerformanceCounter("Memory", "Available MBytes", true);
            return Math.Round(ramCounter.NextValue(), 2);
        }

        // private static string GetDiskType()
        // {
        //     var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
        //     foreach (ManagementObject obj in searcher.Get())
        //     {
        //         var mediaType = obj["MediaType"]?.ToString();
        //         if (mediaType != null && mediaType.Contains("SSD"))
        //             return "SSD";
        //     }
        //     return "HDD";
        // }
        private static string GetDiskType()
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

        using (var process = Process.Start(psi))
        {
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (output.Contains("HDD"))
                return "HDD";
            else if (output.Contains("SSD"))
                return "SSD";
            else
                return "Unknown";
        }
    }
    catch (Exception ex)
    {
        return $"Error: {ex.Message}";
    }
}

        private static double EstimateElectricity(Process process)
        {
            // Simulated electricity consumption: 0.0001 kWh per MB
            return Math.Round(process.WorkingSet64 / (1024.0 * 1024.0) * 0.0001, 4);
        }

        private static string GetCpuModel()
        {
            var searcher = new ManagementObjectSearcher("select * from Win32_Processor");
            foreach (ManagementObject item in searcher.Get())
            {
            var model = item["Name"]?.ToString() ?? "Unknown";
            var processName = item["ProcessorName"]?.ToString() ?? "Unknown";
            return $"Model: {model}, ProcessName: {processName}";
            }
            return "Model: Unknown, ProcessName: Unknown";
        }

        private static (double Free, double Used) GetMemoryStats()
        {
            var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");
            foreach (ManagementObject item in searcher.Get())
            {
                double total = Convert.ToDouble(item["TotalVisibleMemorySize"]) / 1024; // in MB
                double free = Convert.ToDouble(item["FreePhysicalMemory"]) / 1024; // in MB
                double used = total - free;
                return (Math.Round(free, 2), Math.Round(used, 2));
            }
            return (0, 0);
        }
    }
}

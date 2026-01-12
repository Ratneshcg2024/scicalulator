// namespace SCIMetricAPI.Models
// {
//     public class SciExcelModel
//     {
//         public string AppName { get; set; }
//         public string ProcessorName { get; set; }
//         public double CpuUtilization { get; set; }
//         public string memoryUtilization_unit { get; set; }
//         public double MemoryUtilization { get; set; }
//         public string StorageType { get; set; }
//         public double StorageUsedGB { get; set; }
//         public int TotalVcpu { get; set; }
//         public int VcpuUsed { get; set; }
//         public double RamCapacityGB { get; set; }
//         public string region { get; set; }
//         public string duration_unit { get; set; }
//         public int duration { get; set; }
//     }

// }

namespace SCIMetricAPI.Models
{
    public class SciExcelModel
    {
        public string AppName { get; set; }
        public string AppCriticality { get; set; }
        public string DeploymentType { get; set; } // private or public
        public string CloudProvider { get; set; } // AWS/Azure/GCP or NA
        public string ProcessorName { get; set; } // CPU model or instance type
        public int InstanceCount { get; set; }
        public int AppCountPerInstance { get; set; }
        public double CpuUtilization { get; set; } // %
        public string memoryUtilization_unit { get; set; } // mb or percentage
        public double MemoryUtilization { get; set; }
        public string StorageType { get; set; } // HDD or SSD
        public double StorageUsedGB { get; set; }
        public int TotalVcpu { get; set; } // 0 if unknown
        public int VcpuUsed { get; set; } // 0 if unknown
        public double RamCapacityGB { get; set; } // 0 if unknown
        public string region { get; set; } // Country or Cloud region
        public string duration_unit { get; set; } // minutes/hours/days
        public double duration { get; set; }
    }
}
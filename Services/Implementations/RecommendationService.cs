using SCIMetricAPI.DTOs;
using SCIMetricAPI.Services.Interfaces;
using System;

namespace SCIMetricAPI.Services.Implementations
{
    public class RecommendationService : IRecommendationService
    {
        public RecommendationResponse GetRecommendations(RecommendationRequest input)
        {
            var output = new RecommendationResponse();
            Console.WriteLine($"[reading input] CPUutilization unit:{input.CpuUtilization} memory utilization:{input.MemoryUtilization} cpucores availeble:{input.vCPUAvailable} ");

            // 1️⃣ Switch to Renewable Power
            if (input.DeploymentType.Contains("Private", StringComparison.OrdinalIgnoreCase) && input.GridEmissions > 300)
            {
                output.SwitchToRenewablePower = "Yes";
            }
            else
            {
                output.SwitchToRenewablePower = "No";
            }

            // 2️⃣ Change Hosting Region
            if (input.DeploymentType.Contains("Public", StringComparison.OrdinalIgnoreCase) && input.GridEmissions > 300)
            {
                output.ChangeHostingRegion = "Yes";
            }
            else
            {
                output.ChangeHostingRegion = "No";
            }

            // 3️⃣ Implement Scheduling
            if (!input.AppCriticality.Contains("high", StringComparison.OrdinalIgnoreCase))
            {
                output.ScheduleWorkloads = "Yes";
            }
            else
            {
                output.ScheduleWorkloads = "No";
            }

            // 4️⃣ Right-size Instances
            if (input.CpuUtilization < 30 && input.MemoryUtilization < 40 && input.vCPUAvailable > 2)
            {
                output.RightSizeInstances = "Yes";
            }
            else
            {
                output.RightSizeInstances = "No";
            }

            // 5️⃣ Consolidate Workloads
            if ((input.CpuUtilization < 50 && input.CountOfInstances > 1) || (input.CpuUtilization > 50 && input.AppCountPerInstance < 2))
            {
                output.ConsolidateWorkloads = "Yes";
            }
            else
            {
                output.ConsolidateWorkloads = "No";
            }

            // 6️⃣ Software Efficiency
            if (input.TotalOperationalEmissions > 50 || input.TotalOperationalEnergy > 0.05m)
            {
                output.SoftwareEfficiency = "Yes";
            }
            else
            {
                output.SoftwareEfficiency = "No";
            }

            // 7️⃣ Extend Hardware Life
            if (input.DeploymentType.Contains("Private", StringComparison.OrdinalIgnoreCase) && input.TotalEmbodiedEmissions > 5 && input.CpuUtilization < 50)
            {
                output.ExtendHardwareLife = "Yes";
            }
            else
            {
                output.ExtendHardwareLife = "No";
            }

            // 8️⃣ Reduce Active Storage Footprint
            if (input.StorageUsedGB > 200)
            {
                output.ReduceStorageFootprint = "Yes";
            }
            else
            {
                output.ReduceStorageFootprint = "No";
            }

            // 9️⃣ Shut down non-business hours
            if (input.AppCriticality.Equals("low", StringComparison.OrdinalIgnoreCase) || input.AppCriticality.Equals("medium", StringComparison.OrdinalIgnoreCase))
            {
                output.ShutdownPolicies = "Yes";
            }
            else
            {
                output.ShutdownPolicies = "No";
            }

            // 🔟 Move to Serverless / Consumption Model
            if (!input.AppCriticality.Equals("high", StringComparison.OrdinalIgnoreCase) && input.AppCountPerInstance == 1 && input.CpuUtilization < 30 && input.MemoryUtilization < 40)
            {
                output.MoveToServerless = "Yes";
            }
            else
            {
                output.MoveToServerless = "No";
            }

            return output;
        }
    }
}
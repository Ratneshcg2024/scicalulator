using Microsoft.AspNetCore.Mvc;
using SCIMetricAPI.DTOs;
using SCIMetricAPI.Models;
using SCIMetricAPI.Services.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace SCIMetricAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CloudSCIController : ControllerBase
    {
        private readonly ISciCloudCalculatorService _sciCloudCalculatorService;
        private readonly IRecommendationService _recommendationService;
        private readonly IInstanceOptimizationService _instanceOptimizationService;

        public CloudSCIController(ISciCloudCalculatorService sciCloudCalculatorService,
                                  IRecommendationService recommendationService,
                                  IInstanceOptimizationService instanceOptimizationService)
        {
            _sciCloudCalculatorService = sciCloudCalculatorService;
            _recommendationService = recommendationService;
            _instanceOptimizationService = instanceOptimizationService;
        }

        [HttpPost("calculateSCIRecommendations")]
        public async Task<IActionResult> CalculateCloudSCI([FromBody] SCIRequest request)
        {
            if (request == null || request.Instances == null || !request.Instances.Any())
                return BadRequest("Invalid input: request or instances missing.");

            // ✅ Step 1: Calculate SCI
            var sciResponse = await _sciCloudCalculatorService.CalculateCloudSCI(request);
            Console.WriteLine($"[CloudSCIController] SCI Calculation completed. SCI Value: {sciResponse.SCI}");

            // ✅ Step 2: Prepare recommendation request
            var firstInstance = request.Instances.First();
            var recommendationRequest = new RecommendationRequest
            {
                SCIvalue = sciResponse.SCI,
                TotalOperationalEnergy = sciResponse.TotalOperationalEnergy,
                TotalOperationalEmissions = sciResponse.TotalOperationalEmissions,
                TotalEmbodiedEmissions = sciResponse.TotalEmbodiedEmissions,
                GridEmissions = (decimal)sciResponse.GridEmissionFactorUsed,
                DeploymentType = "Public",
                vCPUAvailable=sciResponse.InstanceResults.First().TotalVcpu,
                CpuUtilization = (float)firstInstance.CPUUtilization,
                MemoryUtilization = (float)firstInstance.memoryUtilization,
                StorageUsedGB = (float)firstInstance.StorageVolumeGB,
                CountOfInstances = request.Instances.Count,
                AppCountPerInstance = 1,
                AppCriticality = "Medium"
            };

            // ✅ Step 3: Get recommendations
            var recommendations = _recommendationService.GetRecommendations(recommendationRequest);

            // ✅ Step 4: Initialize combined response
            var combinedResponse = new CombinedSCIResponse
            {
                SCIvalue = sciResponse.SCI,
                TotalOperationalEnergy = sciResponse.TotalOperationalEnergy,
                TotalOperationalEmissions = sciResponse.TotalOperationalEmissions,
                TotalEmbodiedEmissions = sciResponse.TotalEmbodiedEmissions,
                GridEmissions = (decimal)sciResponse.GridEmissionFactorUsed,
                Recommendations = recommendations
            };

            // ✅ Step 5: If RightSizeInstances = Yes → Get right-size suggestion
            if (recommendations.RightSizeInstances.Equals("Yes", System.StringComparison.OrdinalIgnoreCase))
            {
                //var currentInstance = await _instanceOptimizationService.GetInstanceByIdAsync(firstInstance.InstanceTypeId);
                //combinedResponse.RightSizeRecommendation = await _instanceOptimizationService.GetRightSizeSuggestionAsync(currentInstance, firstInstance);
            }

            // ✅ Step 6: If ChangeHostingRegion = Yes → Get region optimization (emission reduction)
            if (recommendations.ChangeHostingRegion.Equals("Yes", System.StringComparison.OrdinalIgnoreCase))
            {
              //  combinedResponse.RegionOptimization = await _instanceOptimizationService.GetRegionOptimizationAsync(firstInstance.RegionId, request.CloudProvider);
            }

            return Ok(combinedResponse);
        }
    }
}
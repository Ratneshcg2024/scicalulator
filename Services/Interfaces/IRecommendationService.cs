namespace SCIMetricAPI.Services.Interfaces
{
    using SCIMetricAPI.DTOs;
    public interface IRecommendationService
    {
        RecommendationResponse GetRecommendations(RecommendationRequest request);
    }
}
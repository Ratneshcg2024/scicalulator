
namespace SCIMetricAPI.DTOs
{
    public class RecommendationDetailDto
    {
        public string Label { get; set; }                 // e.g., "RightSizeInstances"
        public string Title { get; set; }                 // human-friendly title
        public string Description { get; set; }           // long text
        public string Recommendation { get; set; }        // bullets rendered from "~"
        public string ConfigurationTemplate { get; set; } // placeholders filled
    }
}

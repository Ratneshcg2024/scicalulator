namespace SCIMetricAPI.DTOs
{
    public sealed class RetailPriceResultDto
    {
        public string Sku { get; init; } = default!;
        public string Region { get; init; } = default!;
        public double? LinuxPaygUsdPerHour { get; init; }
        public double? WindowsPaygUsdPerHour { get; init; }
    }
}
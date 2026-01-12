// Models/AzureRetailPriceItem.cs
public sealed class AzureRetailPriceItem
{
    public string currencyCode { get; set; } = default!;
    public double tierMinimumUnits { get; set; }
    public double retailPrice { get; set; }
    public double unitPrice { get; set; }
    public string armRegionName { get; set; } = default!;
    public string location { get; set; } = default!;
    public string effectiveStartDate { get; set; } = default!;
    public string meterId { get; set; } = default!;
    public string meterName { get; set; } = default!;
    public string productId { get; set; } = default!;
    public string skuId { get; set; } = default!;
    public string? availabilityId { get; set; }
    public string productName { get; set; } = default!;
    public string skuName { get; set; } = default!;
    public string serviceName { get; set; } = default!;
    public string serviceId { get; set; } = default!;
    public string serviceFamily { get; set; } = default!;
    public string unitOfMeasure { get; set; } = default!;
    public string type { get; set; } = default!;           // Consumption | DevTestConsumption | Reservation
    public bool isPrimaryMeterRegion { get; set; }
    public string armSkuName { get; set; } = default!;
}

// Models/AzureRetailPricesResponse.cs
public sealed class AzureRetailPricesResponse
{
    public string BillingCurrency { get; set; } = default!;
    public string CustomerEntityId { get; set; } = default!;
    public string CustomerEntityType { get; set; } = default!;
    public List<AzureRetailPriceItem> Items { get; set; } = new();
    public string? NextPageLink { get; set; }
    public int Count { get; set; }
}

// Models/AzureVmPrice.cs
public sealed class AzureVmPrice
{
    public string Region { get; set; } = default!;
    public string ArmSkuName { get; set; } = default!;
    public string Os { get; set; } = "Linux";
    public string Currency { get; set; } = "USD";
    public string UnitOfMeasure { get; set; } = "1 Hour";
    public string MeterName { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public DateTime EffectiveStartDate { get; set; }
    public double RetailPrice { get; set; }
}
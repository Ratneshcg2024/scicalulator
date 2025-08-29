using SCIMetricAPI.Models;

namespace SCIMetricAPI.Services.Interfaces
{
    public interface ISciCalculatorService
    {
        SciResultModel CalculateSCI(SciExcelModel model);
    }
}

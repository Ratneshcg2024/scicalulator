namespace SCIMetricAPI.Services.Interfaces
{
    public interface ITdpCoefficientRepository
    {
        double GetTdpCoefficient(double cpuUtilization);
    }
}

using SCIMetricAPI.Services.Interfaces;

namespace SCIMetricAPI.Services.Implementations
{
    public class TdpCoefficientRepository : ITdpCoefficientRepository
    {
        private readonly double[] xAxis = { 0, 10, 50, 100 };
        private readonly double[] yAxis = { 0.12, 0.32, 0.75, 1.02 };

        public double GetTdpCoefficient(double cpuUtilization)
        {
            for (int i = 0; i < xAxis.Length - 1; i++)
            {
                if (xAxis[i] <= cpuUtilization && cpuUtilization <= xAxis[i + 1])
                {
                    double x0 = xAxis[i], x1 = xAxis[i + 1];
                    double y0 = yAxis[i], y1 = yAxis[i + 1];
                    return y0 + ((cpuUtilization - x0) * (y1 - y0)) / (x1 - x0);
                }
            }
            return (cpuUtilization < xAxis[0]) ? yAxis[0] : yAxis[^1];
        }
    }
}

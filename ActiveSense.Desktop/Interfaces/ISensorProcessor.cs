using System.Threading.Tasks;
using ActiveSense.Desktop.Data;

namespace ActiveSense.Desktop.Interfaces;

public interface ISensorProcessor
{
    SensorTypes SupportedTypes { get; }
    Task<(bool Success, string Output, string Error)> ProcessAsync(string arguments);
}
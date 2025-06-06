using System.Threading.Tasks;

namespace ActiveSense.Desktop.Infrastructure.Export.Interfaces;

public interface IArchiveCreator
{
    Task<bool> CreateArchiveAsync(string outputPath, string pdfPath,
        string fileName, string sleepCsv, string activityCsv);
}
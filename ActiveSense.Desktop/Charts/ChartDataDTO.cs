namespace ActiveSense.Desktop.Charts.DTOs;

public class ChartDataDTO
{
   public required string[] Labels { get; set; }
   public required double[] Data { get; set; }
   public string Title { get; set; } = string.Empty;
}
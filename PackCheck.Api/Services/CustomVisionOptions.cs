namespace PackCheck.Api.Services;

public class CustomVisionOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string PredictionKey { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string PublishedName { get; set; } = string.Empty;
    public double ConfidenceThreshold { get; set; } = 0.65;
}

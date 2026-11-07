namespace PackCheck.Contracts;

public class RecognitionResultDto
{
    public List<PredictionDto> Predictions { get; set; } = [];
    public double ConfidenceThreshold { get; set; } = 0.65;
}

public class PredictionDto
{
    public string CategoryKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public double Probability { get; set; }
    public string Label => $"{DisplayName} ({Probability:P0})";
    public bool IsSupported => ItemCategories.IsSupported(CategoryKey);
    public override string ToString() => Label;
}

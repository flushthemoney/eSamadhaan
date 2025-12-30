namespace eSamadhaan.Application.DTOs.Reports;

public class FeedbackAnalyticsReportDto
{
    public double AverageRating { get; set; }
    public int TotalFeedbackCount { get; set; }
    public Dictionary<int, int> FeedbackCountByRating { get; set; } = new();
    public Dictionary<int, double> RatingPercentages { get; set; } = new();
    public int PositiveFeedbackCount { get; set; }
    public int NegativeFeedbackCount { get; set; }
    public double PositiveFeedbackPercentage { get; set; }
}

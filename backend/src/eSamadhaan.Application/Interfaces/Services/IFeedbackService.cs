namespace eSamadhaan.Application.Interfaces.Services;

public interface IFeedbackService
{
    Task<int> CreateFeedbackAsync(int grievanceId, int rating, string? comment);
    Task<object?> GetFeedbackByGrievanceIdAsync(int grievanceId);
    Task<IEnumerable<object>> GetAllFeedbackAsync();
    Task<IEnumerable<object>> GetFeedbackByRatingAsync(int rating);
    Task UpdateFeedbackAsync(int feedbackId, int rating, string? comment);
    
    // LINQ-based analytics
    Task<double> GetAverageRatingAsync();
    Task<double> GetAverageRatingByDepartmentAsync(int departmentId);
    Task<Dictionary<int, int>> GetFeedbackCountByRatingAsync();
    Task<IEnumerable<object>> GetLowRatedGrievancesAsync(int thresholdRating);
}

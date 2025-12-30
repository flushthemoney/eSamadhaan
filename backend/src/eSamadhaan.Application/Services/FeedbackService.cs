using eSamadhaan.Application.DTOs.Feedback;
using eSamadhaan.Application.Exceptions;
using eSamadhaan.Application.Interfaces.Repositories;
using eSamadhaan.Application.Interfaces.Services;
using eSamadhaan.Domain.Entities;
using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Application.Services;

public class FeedbackService : IFeedbackService
{
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly IGrievanceRepository _grievanceRepository;

    public FeedbackService(
        IFeedbackRepository feedbackRepository,
        IGrievanceRepository grievanceRepository)
    {
        _feedbackRepository = feedbackRepository;
        _grievanceRepository = grievanceRepository;
    }

    public async Task<int> CreateFeedbackAsync(int grievanceId, int rating, string? comment)
    {
        // Validate grievance exists
        var grievance = await _grievanceRepository.GetByIdAsync(grievanceId);
        if (grievance == null)
        {
            throw new NotFoundException("Grievance", grievanceId);
        }

        // Only allow feedback for Resolved or Closed grievances
        if (grievance.CurrentStatus != GrievanceStatus.Resolved && grievance.CurrentStatus != GrievanceStatus.Closed)
        {
            throw new BusinessRuleViolationException($"Feedback can only be submitted for Resolved or Closed grievances. Current status: {grievance.CurrentStatus}");
        }

        // Check if feedback already exists
        var existingFeedback = await _feedbackRepository.GetByGrievanceIdAsync(grievanceId);
        if (existingFeedback != null)
        {
            throw new BusinessRuleViolationException("Feedback already exists for this grievance. Use update instead.");
        }

        // Validate rating (1-5)
        if (rating < 1 || rating > 5)
        {
            throw new ValidationException("Rating must be between 1 and 5.");
        }

        // Create feedback
        var feedback = new Feedback
        {
            GrievanceId = grievanceId,
            Rating = rating,
            Comment = comment ?? string.Empty,
            SubmittedAt = DateTime.UtcNow
        };

        var createdFeedback = await _feedbackRepository.CreateAsync(feedback);
        return createdFeedback.Id;
    }

    public async Task<object?> GetFeedbackByGrievanceIdAsync(int grievanceId)
    {
        var feedback = await _feedbackRepository.GetByGrievanceIdAsync(grievanceId);
        
        if (feedback == null)
        {
            return null;
        }

        return MapToResponseDto(feedback);
    }

    public async Task<IEnumerable<object>> GetAllFeedbackAsync()
    {
        var feedbacks = await _feedbackRepository.GetAllAsync();
        return feedbacks.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<object>> GetFeedbackByRatingAsync(int rating)
    {
        // Validate rating
        if (rating < 1 || rating > 5)
        {
            throw new ValidationException("Rating must be between 1 and 5.");
        }

        var feedbacks = await _feedbackRepository.GetByRatingAsync(rating);
        return feedbacks.Select(MapToResponseDto);
    }

    public async Task UpdateFeedbackAsync(int feedbackId, int rating, string? comment)
    {
        var feedback = await _feedbackRepository.GetByIdAsync(feedbackId);
        if (feedback == null)
        {
            throw new NotFoundException("Feedback", feedbackId);
        }

        // Validate rating
        if (rating < 1 || rating > 5)
        {
            throw new ValidationException("Rating must be between 1 and 5.");
        }

        feedback.Rating = rating;
        feedback.Comment = comment ?? string.Empty;
        await _feedbackRepository.UpdateAsync(feedback);
    }

    public async Task<double> GetAverageRatingAsync()
    {
        var query = _feedbackRepository.GetQueryable();
        
        // LINQ aggregation: Calculate overall average rating
        if (!query.Any())
        {
            return 0;
        }

        return query.Average(f => f.Rating);
    }

    public async Task<double> GetAverageRatingByDepartmentAsync(int departmentId)
    {
        var query = _feedbackRepository.GetQueryable();
        
        // LINQ aggregation: Calculate average rating for specific department
        var departmentRatings = query
            .Join(_grievanceRepository.GetQueryable(),
                feedback => feedback.GrievanceId,
                grievance => grievance.Id,
                (feedback, grievance) => new { Feedback = feedback, Grievance = grievance })
            .Where(x => x.Grievance.DepartmentId == departmentId)
            .Select(x => x.Feedback.Rating)
            .ToList();

        if (!departmentRatings.Any())
        {
            return 0;
        }

        return departmentRatings.Average();
    }

    public async Task<Dictionary<int, int>> GetFeedbackCountByRatingAsync()
    {
        var query = _feedbackRepository.GetQueryable();
        
        // LINQ aggregation: Count feedback grouped by rating
        var result = query
            .GroupBy(f => f.Rating)
            .Select(group => new { Rating = group.Key, Count = group.Count() })
            .ToDictionary(x => x.Rating, x => x.Count);

        // Ensure all ratings 1-5 are represented
        for (int rating = 1; rating <= 5; rating++)
        {
            if (!result.ContainsKey(rating))
            {
                result[rating] = 0;
            }
        }

        return result;
    }

    public async Task<IEnumerable<object>> GetLowRatedGrievancesAsync(int thresholdRating)
    {
        // Validate threshold rating
        if (thresholdRating < 1 || thresholdRating > 5)
        {
            throw new ValidationException("Threshold rating must be between 1 and 5.");
        }

        var query = _feedbackRepository.GetQueryable();
        
        // LINQ query: Find grievances with rating at or below threshold
        var lowRatedFeedbacks = query
            .Where(f => f.Rating <= thresholdRating)
            .Join(_grievanceRepository.GetQueryable(),
                feedback => feedback.GrievanceId,
                grievance => grievance.Id,
                (feedback, grievance) => new
                {
                    GrievanceId = grievance.Id,
                    GrievanceNumber = grievance.GrievanceNumber,
                    DepartmentId = grievance.DepartmentId,
                    CategoryId = grievance.CategoryId,
                    Rating = feedback.Rating,
                    Comment = feedback.Comment,
                    SubmittedAt = feedback.SubmittedAt
                })
            .OrderBy(x => x.Rating)
            .ToList();

        return lowRatedFeedbacks;
    }

    // Private helper methods

    private FeedbackResponseDto MapToResponseDto(Feedback feedback)
    {
        return new FeedbackResponseDto
        {
            Id = feedback.Id,
            GrievanceId = feedback.GrievanceId,
            GrievanceNumber = feedback.Grievance?.GrievanceNumber ?? string.Empty,
            Rating = feedback.Rating,
            Comment = feedback.Comment,
            SubmittedAt = feedback.SubmittedAt
        };
    }
}

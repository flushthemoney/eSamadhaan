using eSamadhaan.Application.DTOs.Reports;
using eSamadhaan.Application.Exceptions;
using eSamadhaan.Application.Interfaces.Repositories;
using eSamadhaan.Application.Interfaces.Services;
using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Application.Services;

public class ReportService : IReportService
{
    private readonly IGrievanceRepository _grievanceRepository;
    private readonly IGrievanceResolutionRepository _resolutionRepository;
    private readonly IGrievanceAssignmentRepository _assignmentRepository;
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IGrievanceCategoryRepository _categoryRepository;
    private readonly IUserRepository _userRepository;

    public ReportService(
        IGrievanceRepository grievanceRepository,
        IGrievanceResolutionRepository resolutionRepository,
        IGrievanceAssignmentRepository assignmentRepository,
        IFeedbackRepository feedbackRepository,
        IDepartmentRepository departmentRepository,
        IGrievanceCategoryRepository categoryRepository,
        IUserRepository userRepository)
    {
        _grievanceRepository = grievanceRepository;
        _resolutionRepository = resolutionRepository;
        _assignmentRepository = assignmentRepository;
        _feedbackRepository = feedbackRepository;
        _departmentRepository = departmentRepository;
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
    }

    public async Task<object> GetGrievanceStatusReportAsync()
    {
        var query = _grievanceRepository.GetQueryable();
        
        // LINQ aggregation: Group by status with counts and percentages
        var totalCount = query.Count();
        var statusBreakdown = query
            .GroupBy(g => g.CurrentStatus)
            .Select(group => new
            {
                status = group.Key,
                count = group.Count(),
                percentage = totalCount > 0 ? Math.Round((double)group.Count() / totalCount * 100, 2) : 0.0
            })
            .OrderBy(x => x.status)
            .ToList();

        return new
        {
            totalGrievances = totalCount,
            statusBreakdown = statusBreakdown
        };
    }

    public async Task<object> GetDepartmentPerformanceReportAsync(int departmentId)
    {
        // Validate department exists
        if (!await _departmentRepository.ExistsAsync(departmentId))
        {
            throw new NotFoundException("Department", departmentId);
        }

        var grievanceQuery = _grievanceRepository.GetQueryable();
        var resolutionQuery = _resolutionRepository.GetQueryable();
        var feedbackQuery = _feedbackRepository.GetQueryable();

        // LINQ aggregations: Calculate department metrics
        var totalGrievances = grievanceQuery.Count(g => g.DepartmentId == departmentId);
        
        var statusBreakdown = grievanceQuery
            .Where(g => g.DepartmentId == departmentId)
            .GroupBy(g => g.CurrentStatus)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToDictionary(x => x.Status, x => x.Count);

        var resolvedCount = statusBreakdown.GetValueOrDefault(GrievanceStatus.Resolved, 0) + 
                           statusBreakdown.GetValueOrDefault(GrievanceStatus.Closed, 0);

        var resolutionRate = totalGrievances > 0 ? (double)resolvedCount / totalGrievances * 100 : 0;

        // Average resolution time - Fixed to avoid LINQ translation error
        var departmentGrievanceIds = grievanceQuery
            .Where(g => g.DepartmentId == departmentId)
            .Select(g => g.Id)
            .ToList();

        var resolutionTimes = resolutionQuery
            .Where(r => departmentGrievanceIds.Contains(r.GrievanceId))
            .Select(r => new { r.GrievanceId, r.ResolvedAt })
            .ToList();

        double avgResolutionTime = 0;
        if (resolutionTimes.Any())
        {
            var grievancesWithResolution = grievanceQuery
                .Where(g => departmentGrievanceIds.Contains(g.Id))
                .Select(g => new { g.Id, g.CreatedAt })
                .ToList();

            var resolutionTimesCalculated = resolutionTimes
                .Join(grievancesWithResolution,
                    r => r.GrievanceId,
                    g => g.Id,
                    (r, g) => (r.ResolvedAt - g.CreatedAt).TotalHours)
                .ToList();

            avgResolutionTime = resolutionTimesCalculated.Any() ? resolutionTimesCalculated.Average() : 0;
        }

        // Average feedback rating - Fixed to avoid LINQ translation error
        var departmentFeedbacks = feedbackQuery
            .Where(f => departmentGrievanceIds.Contains(f.GrievanceId))
            .Select(f => f.Rating)
            .ToList();

        var avgRating = departmentFeedbacks.Any() ? departmentFeedbacks.Average() : 0;

        return new
        {
            DepartmentId = departmentId,
            TotalGrievances = totalGrievances,
            ResolvedCount = resolvedCount,
            ResolutionRate = Math.Round(resolutionRate, 2),
            StatusBreakdown = statusBreakdown,
            AverageResolutionTimeHours = Math.Round(avgResolutionTime, 2),
            AverageRating = Math.Round(avgRating, 2)
        };
    }

    public async Task<object> GetResolutionTimeReportAsync()
    {
        var resolutionQuery = _resolutionRepository.GetQueryable();
        var grievanceQuery = _grievanceRepository.GetQueryable();

        // LINQ aggregations: Calculate resolution time statistics
        var resolutionTimesInHours = resolutionQuery
            .Join(grievanceQuery,
                resolution => resolution.GrievanceId,
                grievance => grievance.Id,
                (resolution, grievance) => (resolution.ResolvedAt - grievance.CreatedAt).TotalHours)
            .ToList();

        if (!resolutionTimesInHours.Any())
        {
            return new
            {
                totalResolvedGrievances = 0,
                averageResolutionTimeInDays = 0.0,
                minResolutionTimeInDays = 0.0,
                maxResolutionTimeInDays = 0.0,
                medianResolutionTimeInDays = 0.0
            };
        }

        // Convert hours to days
        var resolutionTimesInDays = resolutionTimesInHours.Select(h => h / 24.0).ToList();
        var sortedTimes = resolutionTimesInDays.OrderBy(x => x).ToList();
        var median = sortedTimes.Count % 2 == 0
            ? (sortedTimes[sortedTimes.Count / 2 - 1] + sortedTimes[sortedTimes.Count / 2]) / 2
            : sortedTimes[sortedTimes.Count / 2];

        return new
        {
            totalResolvedGrievances = resolutionTimesInDays.Count,
            averageResolutionTimeInDays = Math.Round(resolutionTimesInDays.Average(), 2),
            minResolutionTimeInDays = Math.Round(resolutionTimesInDays.Min(), 2),
            maxResolutionTimeInDays = Math.Round(resolutionTimesInDays.Max(), 2),
            medianResolutionTimeInDays = Math.Round(median, 2)
        };
    }

    public async Task<object> GetResolutionTimeReportByDepartmentAsync(int departmentId)
    {
        // Validate department exists
        if (!await _departmentRepository.ExistsAsync(departmentId))
        {
            throw new NotFoundException("Department", departmentId);
        }

        var resolutionQuery = _resolutionRepository.GetQueryable();
        var grievanceQuery = _grievanceRepository.GetQueryable();

        // LINQ aggregations: Calculate resolution time for department
        var resolutionTimesInHours = resolutionQuery
            .Join(grievanceQuery,
                resolution => resolution.GrievanceId,
                grievance => grievance.Id,
                (resolution, grievance) => new 
                { 
                    DepartmentId = grievance.DepartmentId,
                    ResolutionTime = (resolution.ResolvedAt - grievance.CreatedAt).TotalHours 
                })
            .Where(x => x.DepartmentId == departmentId)
            .Select(x => x.ResolutionTime)
            .ToList();

        if (!resolutionTimesInHours.Any())
        {
            return new
            {
                departmentId = departmentId,
                totalResolvedGrievances = 0,
                averageResolutionTimeInDays = 0.0
            };
        }

        // Convert hours to days
        var resolutionTimesInDays = resolutionTimesInHours.Select(h => h / 24.0).ToList();

        return new
        {
            departmentId = departmentId,
            totalResolvedGrievances = resolutionTimesInDays.Count,
            averageResolutionTimeInDays = Math.Round(resolutionTimesInDays.Average(), 2)
        };
    }

    public async Task<object> GetResolutionTimeReportByCategoryAsync(int categoryId)
    {
        // Validate category exists
        if (!await _categoryRepository.ExistsAsync(categoryId))
        {
            throw new NotFoundException("Category", categoryId);
        }

        var resolutionQuery = _resolutionRepository.GetQueryable();
        var grievanceQuery = _grievanceRepository.GetQueryable();

        // LINQ aggregations: Calculate resolution time for category
        var resolutionTimesInHours = resolutionQuery
            .Join(grievanceQuery,
                resolution => resolution.GrievanceId,
                grievance => grievance.Id,
                (resolution, grievance) => new 
                { 
                    CategoryId = grievance.CategoryId,
                    ResolutionTime = (resolution.ResolvedAt - grievance.CreatedAt).TotalHours 
                })
            .Where(x => x.CategoryId == categoryId)
            .Select(x => x.ResolutionTime)
            .ToList();

        if (!resolutionTimesInHours.Any())
        {
            return new
            {
                categoryId = categoryId,
                totalResolvedGrievances = 0,
                averageResolutionTimeInDays = 0.0
            };
        }

        // Convert hours to days
        var resolutionTimesInDays = resolutionTimesInHours.Select(h => h / 24.0).ToList();

        return new
        {
            categoryId = categoryId,
            totalResolvedGrievances = resolutionTimesInDays.Count,
            averageResolutionTimeInDays = Math.Round(resolutionTimesInDays.Average(), 2)
        };
    }

    public async Task<IEnumerable<object>> GetTopPerformingOfficersAsync(int topCount, int? departmentId = null)
    {
        var assignmentQuery = _assignmentRepository.GetQueryable();
        var resolutionQuery = _resolutionRepository.GetQueryable();
        var grievanceQuery = _grievanceRepository.GetQueryable();

        // Get all officers who have assignments, filtered by department if provided
        IQueryable<int> officerIdsQuery = assignmentQuery
            .Select(a => a.OfficerId)
            .Distinct();

        if (departmentId.HasValue)
        {
            officerIdsQuery = assignmentQuery
                .Where(a => a.Grievance.DepartmentId == departmentId.Value)
                .Select(a => a.OfficerId)
                .Distinct();
        }

        var officerIds = officerIdsQuery.ToList();
        var result = new List<object>();

        foreach (var officerId in officerIds)
        {
            var officer = await _userRepository.GetByIdAsync(officerId);
            if (officer == null || officer.Role != "DepartmentOfficer") continue;

            // Get all assignments for this officer, filtered by department if provided
            var officerAssignmentsQuery = assignmentQuery.Where(a => a.OfficerId == officerId);
            if (departmentId.HasValue)
            {
                officerAssignmentsQuery = officerAssignmentsQuery.Where(a => a.Grievance.DepartmentId == departmentId.Value);
            }
            var officerAssignments = officerAssignmentsQuery.ToList();
            var assignedGrievanceIds = officerAssignments.Select(a => a.GrievanceId).ToList();

            var totalAssignedGrievances = assignedGrievanceIds.Count;

            // Get resolved grievances - count resolutions for grievances assigned to this officer
            var resolvedGrievanceIds = resolutionQuery
                .Where(r => r.ResolvedByOfficerId == officerId && assignedGrievanceIds.Contains(r.GrievanceId))
                .Select(r => r.GrievanceId)
                .Distinct()
                .ToList();

            var resolvedGrievances = resolvedGrievanceIds.Count;
            var pendingGrievances = totalAssignedGrievances - resolvedGrievances;
            var resolutionRate = totalAssignedGrievances > 0 
                ? Math.Round((double)resolvedGrievances / totalAssignedGrievances * 100, 2) 
                : 0.0;

            // Calculate average resolution time in days for resolved grievances assigned to this officer
            var resolutionTimesInHours = resolutionQuery
                .Where(r => r.ResolvedByOfficerId == officerId && assignedGrievanceIds.Contains(r.GrievanceId))
                .Join(grievanceQuery,
                    resolution => resolution.GrievanceId,
                    grievance => grievance.Id,
                    (resolution, grievance) => (resolution.ResolvedAt - grievance.CreatedAt).TotalHours)
                .ToList();

            var averageResolutionTimeInDays = resolutionTimesInHours.Any()
                ? Math.Round(resolutionTimesInHours.Average() / 24.0, 2)
                : 0.0;

            // Get department name
            var departmentName = officer.DepartmentId.HasValue
                ? (await _departmentRepository.GetByIdAsync(officer.DepartmentId.Value))?.Name ?? "Unknown"
                : "Unknown";

            result.Add(new
            {
                officerId = officerId,
                officerName = officer.Name,
                departmentName = departmentName,
                totalAssignedGrievances = totalAssignedGrievances,
                resolvedGrievances = resolvedGrievances,
                pendingGrievances = pendingGrievances,
                resolutionRate = resolutionRate,
                averageResolutionTimeInDays = averageResolutionTimeInDays
            });
        }

        // Sort by resolution rate descending, then by total assigned descending, then by average resolution time ascending
        return result
            .OrderByDescending(x => ((dynamic)x).resolutionRate)
            .ThenByDescending(x => ((dynamic)x).totalAssignedGrievances)
            .ThenBy(x => ((dynamic)x).averageResolutionTimeInDays)
            .Take(topCount)
            .ToList();
    }

    public async Task<object> GetFeedbackAnalyticsReportAsync()
    {
        var feedbackQuery = _feedbackRepository.GetQueryable();

        // LINQ aggregations: Overall feedback analytics
        var totalFeedbacks = feedbackQuery.Count();
        
        if (totalFeedbacks == 0)
        {
            return new
            {
                totalFeedbackCount = 0,
                averageRating = 0.0,
                feedbackCountByRating = new Dictionary<int, int>(),
                ratingPercentages = new Dictionary<int, double>(),
                positiveFeedbackCount = 0,
                negativeFeedbackCount = 0,
                positiveFeedbackPercentage = 0.0
            };
        }

        var averageRating = feedbackQuery.Average(f => f.Rating);
        
        var feedbackCountByRating = feedbackQuery
            .GroupBy(f => f.Rating)
            .Select(group => new { Rating = group.Key, Count = group.Count() })
            .ToDictionary(x => x.Rating, x => x.Count);

        // Ensure all ratings 1-5 are represented
        for (int rating = 1; rating <= 5; rating++)
        {
            if (!feedbackCountByRating.ContainsKey(rating))
            {
                feedbackCountByRating[rating] = 0;
            }
        }

        // Calculate percentages
        var ratingPercentages = feedbackCountByRating.ToDictionary(
            kvp => kvp.Key,
            kvp => totalFeedbacks > 0 ? Math.Round((double)kvp.Value / totalFeedbacks * 100, 2) : 0.0
        );

        // Calculate positive (>=4) and negative (<=2) feedback counts
        var positiveFeedbackCount = feedbackQuery.Count(f => f.Rating >= 4);
        var negativeFeedbackCount = feedbackQuery.Count(f => f.Rating <= 2);
        var positiveFeedbackPercentage = totalFeedbacks > 0 
            ? Math.Round((double)positiveFeedbackCount / totalFeedbacks * 100, 2) 
            : 0.0;

        return new
        {
            totalFeedbackCount = totalFeedbacks,
            averageRating = Math.Round(averageRating, 2),
            feedbackCountByRating = feedbackCountByRating,
            ratingPercentages = ratingPercentages,
            positiveFeedbackCount = positiveFeedbackCount,
            negativeFeedbackCount = negativeFeedbackCount,
            positiveFeedbackPercentage = positiveFeedbackPercentage
        };
    }

    public async Task<object> GetFeedbackAnalyticsByDepartmentAsync(int departmentId)
    {
        // Validate department exists
        if (!await _departmentRepository.ExistsAsync(departmentId))
        {
            throw new NotFoundException("Department", departmentId);
        }

        var feedbackQuery = _feedbackRepository.GetQueryable();
        var grievanceQuery = _grievanceRepository.GetQueryable();

        // LINQ aggregations: Department feedback analytics
        var departmentFeedbacks = feedbackQuery
            .Join(grievanceQuery,
                feedback => feedback.GrievanceId,
                grievance => grievance.Id,
                (feedback, grievance) => new { Feedback = feedback, Grievance = grievance })
            .Where(x => x.Grievance.DepartmentId == departmentId)
            .Select(x => x.Feedback)
            .ToList();

        var totalFeedbacks = departmentFeedbacks.Count;

        if (totalFeedbacks == 0)
        {
            return new
            {
                departmentId = departmentId,
                totalFeedbackCount = 0,
                averageRating = 0.0,
                feedbackCountByRating = new Dictionary<int, int>(),
                ratingPercentages = new Dictionary<int, double>(),
                positiveFeedbackCount = 0,
                negativeFeedbackCount = 0,
                positiveFeedbackPercentage = 0.0
            };
        }

        var averageRating = departmentFeedbacks.Average(f => f.Rating);
        
        var feedbackCountByRating = departmentFeedbacks
            .GroupBy(f => f.Rating)
            .Select(group => new { Rating = group.Key, Count = group.Count() })
            .ToDictionary(x => x.Rating, x => x.Count);

        // Ensure all ratings 1-5 are represented
        for (int rating = 1; rating <= 5; rating++)
        {
            if (!feedbackCountByRating.ContainsKey(rating))
            {
                feedbackCountByRating[rating] = 0;
            }
        }

        // Calculate percentages
        var ratingPercentages = feedbackCountByRating.ToDictionary(
            kvp => kvp.Key,
            kvp => totalFeedbacks > 0 ? Math.Round((double)kvp.Value / totalFeedbacks * 100, 2) : 0.0
        );

        // Calculate positive (>=4) and negative (<=2) feedback counts
        var positiveFeedbackCount = departmentFeedbacks.Count(f => f.Rating >= 4);
        var negativeFeedbackCount = departmentFeedbacks.Count(f => f.Rating <= 2);
        var positiveFeedbackPercentage = totalFeedbacks > 0 
            ? Math.Round((double)positiveFeedbackCount / totalFeedbacks * 100, 2) 
            : 0.0;

        return new
        {
            departmentId = departmentId,
            totalFeedbackCount = totalFeedbacks,
            averageRating = Math.Round(averageRating, 2),
            feedbackCountByRating = feedbackCountByRating,
            ratingPercentages = ratingPercentages,
            positiveFeedbackCount = positiveFeedbackCount,
            negativeFeedbackCount = negativeFeedbackCount,
            positiveFeedbackPercentage = positiveFeedbackPercentage
        };
    }

    public async Task<object> GetDashboardSummaryAsync(int? departmentId, int? userId)
    {
        var grievanceQuery = _grievanceRepository.GetQueryable();

        // Apply filters if provided
        if (departmentId.HasValue)
        {
            grievanceQuery = grievanceQuery.Where(g => g.DepartmentId == departmentId.Value);
        }

        // LINQ aggregations: Dashboard summary statistics
        var totalGrievances = grievanceQuery.Count();
        
        var statusCounts = grievanceQuery
            .GroupBy(g => g.CurrentStatus)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToDictionary(x => x.Status, x => x.Count);

        // Individual status counts
        var submittedGrievances = statusCounts.GetValueOrDefault(GrievanceStatus.Submitted, 0);
        var assignedGrievances = statusCounts.GetValueOrDefault(GrievanceStatus.Assigned, 0);
        var inReviewGrievances = statusCounts.GetValueOrDefault(GrievanceStatus.InReview, 0);
        var resolvedGrievances = statusCounts.GetValueOrDefault(GrievanceStatus.Resolved, 0);
        var closedGrievances = statusCounts.GetValueOrDefault(GrievanceStatus.Closed, 0);

        var resolvedCount = resolvedGrievances + closedGrievances;

        // Calculate resolution rate
        var resolutionRate = totalGrievances > 0 
            ? Math.Round((double)resolvedCount / totalGrievances * 100, 2) 
            : 0;

        // Calculate average resolution time in days
        var resolutionQuery = _resolutionRepository.GetQueryable();
        var resolutionTimes = resolutionQuery
            .Join(grievanceQuery,
                resolution => resolution.GrievanceId,
                grievance => grievance.Id,
                (resolution, grievance) => new
                {
                    DepartmentId = grievance.DepartmentId,
                    ResolutionTimeDays = (resolution.ResolvedAt - grievance.CreatedAt).TotalDays
                })
            .ToList();

        // Apply department filter if provided
        if (departmentId.HasValue)
        {
            resolutionTimes = resolutionTimes
                .Where(x => x.DepartmentId == departmentId.Value)
                .ToList();
        }

        var averageResolutionTimeInDays = resolutionTimes.Any()
            ? Math.Round(resolutionTimes.Average(x => x.ResolutionTimeDays), 2)
            : 0.0;

        // Calculate average feedback rating
        var feedbackQuery = _feedbackRepository.GetQueryable();
        var feedbackRatings = feedbackQuery
            .Join(grievanceQuery,
                feedback => feedback.GrievanceId,
                grievance => grievance.Id,
                (feedback, grievance) => new
                {
                    DepartmentId = grievance.DepartmentId,
                    Rating = feedback.Rating
                })
            .ToList();

        // Apply department filter if provided
        if (departmentId.HasValue)
        {
            feedbackRatings = feedbackRatings
                .Where(x => x.DepartmentId == departmentId.Value)
                .ToList();
        }

        var averageFeedbackRating = feedbackRatings.Any()
            ? Math.Round(feedbackRatings.Average(f => f.Rating), 2)
            : 0.0;

        // Count total resolutions (not status count, but actual resolution records)
        var totalResolutions = resolutionQuery
            .Join(grievanceQuery,
                resolution => resolution.GrievanceId,
                grievance => grievance.Id,
                (resolution, grievance) => new { DepartmentId = grievance.DepartmentId })
            .ToList();

        // Apply department filter if provided
        if (departmentId.HasValue)
        {
            totalResolutions = totalResolutions
                .Where(x => x.DepartmentId == departmentId.Value)
                .ToList();
        }

        var totalResolutionsCount = totalResolutions.Count;

        // Recent grievances
        var recentGrievances = grievanceQuery
            .OrderByDescending(g => g.CreatedAt)
            .Take(10)
            .Select(g => new
            {
                id = g.Id,
                grievanceNumber = g.GrievanceNumber,
                status = g.CurrentStatus.ToString(),
                createdAt = g.CreatedAt
            })
            .ToList();

        // Recent assignments
        var assignmentQuery = _assignmentRepository.GetQueryable();
        var recentAssignments = assignmentQuery
            .Where(a => departmentId == null || a.Grievance.DepartmentId == departmentId.Value)
            .OrderByDescending(a => a.AssignedAt)
            .Take(10)
            .Select(a => new
            {
                grievanceId = a.GrievanceId,
                grievanceNumber = a.Grievance.GrievanceNumber,
                officerName = a.Officer.Name,
                assignedAt = a.AssignedAt
            })
            .ToList();

        // Top categories
        var topCategories = grievanceQuery
            .GroupBy(g => new { g.CategoryId, g.Category.Name })
            .Select(group => new { CategoryName = group.Key.Name, Count = group.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToDictionary(x => x.CategoryName, x => x.Count);

        // Officer workload if userId provided
        object? officerWorkload = null;
        if (userId.HasValue)
        {
            var activeAssignments = _assignmentRepository.GetQueryable()
                .Count(a => a.OfficerId == userId.Value && a.IsActive);
            
            var officerTotalResolutions = _resolutionRepository.GetQueryable()
                .Count(r => r.ResolvedByOfficerId == userId.Value);

            officerWorkload = new
            {
                activeAssignments = activeAssignments,
                totalResolutions = officerTotalResolutions
            };
        }

        return new
        {
            totalGrievances = totalGrievances,
            submittedGrievances = submittedGrievances,
            assignedGrievances = assignedGrievances,
            inReviewGrievances = inReviewGrievances,
            resolvedGrievances = resolvedGrievances,
            closedGrievances = closedGrievances,
            totalResolutions = totalResolutionsCount,
            resolutionRate = resolutionRate,
            averageResolutionTimeInDays = averageResolutionTimeInDays,
            averageFeedbackRating = averageFeedbackRating,
            recentGrievances = recentGrievances,
            recentAssignments = recentAssignments,
            topCategories = topCategories,
            officerWorkload = officerWorkload
        };
    }

    public async Task<object> GetOfficerDashboardSummaryAsync(int userId)
    {
        // Get officer details
        var officer = await _userRepository.GetByIdAsync(userId);
        if (officer == null)
        {
            throw new NotFoundException("Officer", userId);
        }

        if (!officer.DepartmentId.HasValue)
        {
            throw new BusinessRuleViolationException("Officer must be assigned to a department");
        }

        // Department-scoped grievances
        var grievanceQuery = _grievanceRepository.GetQueryable()
            .Where(g => g.DepartmentId == officer.DepartmentId.Value);

        // LINQ aggregations: Dashboard summary statistics
        var totalGrievances = grievanceQuery.Count();
        
        var statusCounts = grievanceQuery
            .GroupBy(g => g.CurrentStatus)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToDictionary(x => x.Status, x => x.Count);

        var pendingCount = statusCounts.GetValueOrDefault(GrievanceStatus.Submitted, 0) +
                          statusCounts.GetValueOrDefault(GrievanceStatus.Assigned, 0) +
                          statusCounts.GetValueOrDefault(GrievanceStatus.InReview, 0);

        var resolvedCount = statusCounts.GetValueOrDefault(GrievanceStatus.Resolved, 0) +
                           statusCounts.GetValueOrDefault(GrievanceStatus.Closed, 0);
        var closedCount = statusCounts.GetValueOrDefault(GrievanceStatus.Closed, 0);

        // Recent grievances in department
        var recentGrievances = grievanceQuery
            .OrderByDescending(g => g.CreatedAt)
            .Take(10)
            .Select(g => new
            {
                g.Id,
                g.GrievanceNumber,
                g.CurrentStatus,
                g.CreatedAt
            })
            .ToList();

        // Officer's workload
        var activeAssignments = _assignmentRepository.GetQueryable()
            .Count(a => a.OfficerId == userId && a.IsActive && 
                        a.Grievance.CurrentStatus != GrievanceStatus.Closed);
        
        var totalResolutions = _resolutionRepository.GetQueryable()
            .Count(r => r.ResolvedByOfficerId == userId);

        var officerWorkload = new
        {
            ActiveAssignments = activeAssignments,
            TotalResolutions = totalResolutions
        };

        return new
        {
            DepartmentId = officer.DepartmentId.Value,
            DepartmentName = officer.Department?.Name ?? "Unknown",
            TotalGrievances = totalGrievances,
            PendingCount = pendingCount,
            ResolvedCount = resolvedCount,
            ClosedCount = closedCount,
            StatusBreakdown = statusCounts,
            RecentGrievances = recentGrievances,
            OfficerWorkload = officerWorkload
        };
    }
}

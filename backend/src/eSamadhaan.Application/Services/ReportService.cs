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
        var statusCounts = query
            .GroupBy(g => g.CurrentStatus)
            .Select(group => new
            {
                Status = group.Key,
                Count = group.Count(),
                Percentage = totalCount > 0 ? (double)group.Count() / totalCount * 100 : 0
            })
            .OrderBy(x => x.Status)
            .ToList();

        return new
        {
            TotalGrievances = totalCount,
            StatusBreakdown = statusCounts
        };
    }

    public async Task<IEnumerable<object>> GetGrievancesByStatusDetailedAsync(GrievanceStatus status)
    {
        var query = _grievanceRepository.GetQueryable();
        
        // LINQ query: Filter by status and project detailed information
        return query
            .Where(g => g.CurrentStatus == status)
            .Select(g => new
            {
                g.Id,
                g.GrievanceNumber,
                CitizenName = g.Citizen.Name,
                DepartmentName = g.Department.Name,
                CategoryName = g.Category.Name,
                g.CurrentStatus,
                g.CreatedAt,
                g.UpdatedAt,
                DaysSinceCreation = (DateTime.UtcNow - g.CreatedAt).Days
            })
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
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

    public async Task<IEnumerable<object>> GetAllDepartmentPerformanceReportsAsync()
    {
        var departments = await _departmentRepository.GetAllAsync();
        var reports = new List<object>();

        foreach (var dept in departments)
        {
            var report = await GetDepartmentPerformanceReportAsync(dept.Id);
            reports.Add(report);
        }

        return reports;
    }

    public async Task<object> GetCategorySummaryReportAsync(int categoryId)
    {
        // Validate category exists
        if (!await _categoryRepository.ExistsAsync(categoryId))
        {
            throw new NotFoundException("Category", categoryId);
        }

        var query = _grievanceRepository.GetQueryable();

        // LINQ aggregations: Calculate category metrics
        var totalGrievances = query.Count(g => g.CategoryId == categoryId);
        
        var statusBreakdown = query
            .Where(g => g.CategoryId == categoryId)
            .GroupBy(g => g.CurrentStatus)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToDictionary(x => x.Status, x => x.Count);

        var departmentBreakdown = query
            .Where(g => g.CategoryId == categoryId)
            .GroupBy(g => g.DepartmentId)
            .Select(group => new 
            { 
                DepartmentId = group.Key,
                DepartmentName = group.First().Department.Name,
                Count = group.Count() 
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        return new
        {
            CategoryId = categoryId,
            TotalGrievances = totalGrievances,
            StatusBreakdown = statusBreakdown,
            DepartmentBreakdown = departmentBreakdown
        };
    }

    public async Task<IEnumerable<object>> GetAllCategorySummaryReportsAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        var reports = new List<object>();

        foreach (var category in categories)
        {
            var report = await GetCategorySummaryReportAsync(category.Id);
            reports.Add(report);
        }

        return reports;
    }

    public async Task<object> GetResolutionTimeReportAsync()
    {
        var resolutionQuery = _resolutionRepository.GetQueryable();
        var grievanceQuery = _grievanceRepository.GetQueryable();

        // LINQ aggregations: Calculate resolution time statistics
        var resolutionTimes = resolutionQuery
            .Join(grievanceQuery,
                resolution => resolution.GrievanceId,
                grievance => grievance.Id,
                (resolution, grievance) => (resolution.ResolvedAt - grievance.CreatedAt).TotalHours)
            .ToList();

        if (!resolutionTimes.Any())
        {
            return new
            {
                TotalResolutions = 0,
                AverageHours = 0.0,
                MinimumHours = 0.0,
                MaximumHours = 0.0,
                MedianHours = 0.0
            };
        }

        var sortedTimes = resolutionTimes.OrderBy(x => x).ToList();
        var median = sortedTimes.Count % 2 == 0
            ? (sortedTimes[sortedTimes.Count / 2 - 1] + sortedTimes[sortedTimes.Count / 2]) / 2
            : sortedTimes[sortedTimes.Count / 2];

        return new
        {
            TotalResolutions = resolutionTimes.Count,
            AverageHours = Math.Round(resolutionTimes.Average(), 2),
            MinimumHours = Math.Round(resolutionTimes.Min(), 2),
            MaximumHours = Math.Round(resolutionTimes.Max(), 2),
            MedianHours = Math.Round(median, 2)
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
        var resolutionTimes = resolutionQuery
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

        if (!resolutionTimes.Any())
        {
            return new
            {
                DepartmentId = departmentId,
                TotalResolutions = 0,
                AverageHours = 0.0
            };
        }

        return new
        {
            DepartmentId = departmentId,
            TotalResolutions = resolutionTimes.Count,
            AverageHours = Math.Round(resolutionTimes.Average(), 2)
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
        var resolutionTimes = resolutionQuery
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

        if (!resolutionTimes.Any())
        {
            return new
            {
                CategoryId = categoryId,
                TotalResolutions = 0,
                AverageHours = 0.0
            };
        }

        return new
        {
            CategoryId = categoryId,
            TotalResolutions = resolutionTimes.Count,
            AverageHours = Math.Round(resolutionTimes.Average(), 2)
        };
    }

    public async Task<object> GetOfficerPerformanceReportAsync(int officerId)
    {
        // Validate officer exists
        var officer = await _userRepository.GetByIdAsync(officerId);
        if (officer == null)
        {
            throw new NotFoundException("Officer", officerId);
        }

        var resolutionQuery = _resolutionRepository.GetQueryable();
        var assignmentQuery = _assignmentRepository.GetQueryable();

        // LINQ aggregations: Calculate officer performance metrics
        var totalResolutions = resolutionQuery.Count(r => r.ResolvedByOfficerId == officerId);
        var activeAssignments = assignmentQuery.Count(a => a.OfficerId == officerId && a.IsActive);

        var avgResolutionTime = resolutionQuery
            .Where(r => r.ResolvedByOfficerId == officerId)
            .Join(_grievanceRepository.GetQueryable(),
                resolution => resolution.GrievanceId,
                grievance => grievance.Id,
                (resolution, grievance) => (resolution.ResolvedAt - grievance.CreatedAt).TotalHours)
            .DefaultIfEmpty(0)
            .Average();

        return new
        {
            OfficerId = officerId,
            OfficerName = officer.Name,
            TotalResolutions = totalResolutions,
            ActiveAssignments = activeAssignments,
            AverageResolutionTimeHours = Math.Round(avgResolutionTime, 2)
        };
    }

    public async Task<IEnumerable<object>> GetTopPerformingOfficersAsync(int topCount)
    {
        var resolutionQuery = _resolutionRepository.GetQueryable();
        var grievanceQuery = _grievanceRepository.GetQueryable();

        // Get all resolutions with their grievance creation times - Fixed to avoid LINQ translation error
        var resolutionsWithGrievances = resolutionQuery
            .Select(r => new { r.ResolvedByOfficerId, r.GrievanceId, r.ResolvedAt })
            .ToList();

        var grievanceCreationTimes = grievanceQuery
            .Select(g => new { g.Id, g.CreatedAt })
            .ToList();

        // Calculate officer statistics using client-side LINQ
        var officerStats = resolutionsWithGrievances
            .Join(grievanceCreationTimes,
                r => r.GrievanceId,
                g => g.Id,
                (r, g) => new
                {
                    r.ResolvedByOfficerId,
                    ResolutionTime = (r.ResolvedAt - g.CreatedAt).TotalHours
                })
            .GroupBy(x => x.ResolvedByOfficerId)
            .Select(group => new
            {
                OfficerId = group.Key,
                ResolutionCount = group.Count(),
                AverageResolutionTime = group.Average(x => x.ResolutionTime)
            })
            .OrderByDescending(x => x.ResolutionCount)
            .ThenBy(x => x.AverageResolutionTime)
            .Take(topCount)
            .ToList();

        // Enrich with officer names
        var result = new List<object>();
        foreach (var stat in officerStats)
        {
            var officer = await _userRepository.GetByIdAsync(stat.OfficerId);
            result.Add(new
            {
                stat.OfficerId,
                OfficerName = officer?.Name ?? "Unknown",
                stat.ResolutionCount,
                AverageResolutionTimeHours = Math.Round(stat.AverageResolutionTime, 2)
            });
        }

        return result;
    }

    public async Task<IEnumerable<object>> GetGrievanceTrendReportAsync(DateTime startDate, DateTime endDate, string groupBy)
    {
        var query = _grievanceRepository.GetQueryable();

        // Filter by date range
        var filteredQuery = query.Where(g => g.CreatedAt >= startDate && g.CreatedAt <= endDate);

        // LINQ grouping: Group by time period
        switch (groupBy.ToLower())
        {
            case "day":
                return filteredQuery
                    .GroupBy(g => g.CreatedAt.Date)
                    .Select(group => new
                    {
                        Date = group.Key,
                        Count = group.Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToList();

            case "week":
                return filteredQuery
                    .GroupBy(g => new 
                    { 
                        Year = g.CreatedAt.Year,
                        Week = (g.CreatedAt.DayOfYear - 1) / 7 + 1 
                    })
                    .Select(group => new
                    {
                        Year = group.Key.Year,
                        Week = group.Key.Week,
                        Count = group.Count()
                    })
                    .OrderBy(x => x.Year).ThenBy(x => x.Week)
                    .ToList();

            case "month":
                return filteredQuery
                    .GroupBy(g => new { g.CreatedAt.Year, g.CreatedAt.Month })
                    .Select(group => new
                    {
                        Year = group.Key.Year,
                        Month = group.Key.Month,
                        Count = group.Count()
                    })
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToList();

            default:
                throw new ValidationException("Invalid groupBy parameter. Valid values: day, week, month");
        }
    }

    public async Task<object> GetMonthlyGrievanceReportAsync(int year, int month)
    {
        // Validate month
        if (month < 1 || month > 12)
        {
            throw new ValidationException("Month must be between 1 and 12.");
        }

        var query = _grievanceRepository.GetQueryable();
        
        // LINQ filtering and aggregation: Monthly statistics
        var monthlyGrievances = query
            .Where(g => g.CreatedAt.Year == year && g.CreatedAt.Month == month)
            .ToList();

        var totalCount = monthlyGrievances.Count;
        
        var statusBreakdown = monthlyGrievances
            .GroupBy(g => g.CurrentStatus)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToDictionary(x => x.Status, x => x.Count);

        var departmentBreakdown = monthlyGrievances
            .GroupBy(g => g.DepartmentId)
            .Select(group => new 
            { 
                DepartmentId = group.Key,
                DepartmentName = group.First().Department?.Name ?? "Unknown",
                Count = group.Count() 
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        var categoryBreakdown = monthlyGrievances
            .GroupBy(g => g.CategoryId)
            .Select(group => new 
            { 
                CategoryId = group.Key,
                CategoryName = group.First().Category?.Name ?? "Unknown",
                Count = group.Count() 
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        return new
        {
            Year = year,
            Month = month,
            TotalGrievances = totalCount,
            StatusBreakdown = statusBreakdown,
            DepartmentBreakdown = departmentBreakdown,
            CategoryBreakdown = categoryBreakdown
        };
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
                TotalFeedbacks = 0,
                AverageRating = 0.0,
                RatingDistribution = new Dictionary<int, int>()
            };
        }

        var averageRating = feedbackQuery.Average(f => f.Rating);
        
        var ratingDistribution = feedbackQuery
            .GroupBy(f => f.Rating)
            .Select(group => new { Rating = group.Key, Count = group.Count() })
            .ToDictionary(x => x.Rating, x => x.Count);

        // Ensure all ratings 1-5 are represented
        for (int rating = 1; rating <= 5; rating++)
        {
            if (!ratingDistribution.ContainsKey(rating))
            {
                ratingDistribution[rating] = 0;
            }
        }

        return new
        {
            TotalFeedbacks = totalFeedbacks,
            AverageRating = Math.Round(averageRating, 2),
            RatingDistribution = ratingDistribution
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

        if (!departmentFeedbacks.Any())
        {
            return new
            {
                DepartmentId = departmentId,
                TotalFeedbacks = 0,
                AverageRating = 0.0,
                RatingDistribution = new Dictionary<int, int>()
            };
        }

        var averageRating = departmentFeedbacks.Average(f => f.Rating);
        
        var ratingDistribution = departmentFeedbacks
            .GroupBy(f => f.Rating)
            .Select(group => new { Rating = group.Key, Count = group.Count() })
            .ToDictionary(x => x.Rating, x => x.Count);

        return new
        {
            DepartmentId = departmentId,
            TotalFeedbacks = departmentFeedbacks.Count,
            AverageRating = Math.Round(averageRating, 2),
            RatingDistribution = ratingDistribution
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

        var pendingCount = statusCounts.GetValueOrDefault(GrievanceStatus.Submitted, 0) +
                          statusCounts.GetValueOrDefault(GrievanceStatus.Assigned, 0) +
                          statusCounts.GetValueOrDefault(GrievanceStatus.InReview, 0);

        var resolvedCount = statusCounts.GetValueOrDefault(GrievanceStatus.Resolved, 0);
        var closedCount = statusCounts.GetValueOrDefault(GrievanceStatus.Closed, 0);

        // Recent grievances
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

        // Officer workload if userId provided
        object? officerWorkload = null;
        if (userId.HasValue)
        {
            var activeAssignments = _assignmentRepository.GetQueryable()
                .Count(a => a.OfficerId == userId.Value && a.IsActive);
            
            var totalResolutions = _resolutionRepository.GetQueryable()
                .Count(r => r.ResolvedByOfficerId == userId.Value);

            officerWorkload = new
            {
                ActiveAssignments = activeAssignments,
                TotalResolutions = totalResolutions
            };
        }

        return new
        {
            TotalGrievances = totalGrievances,
            PendingCount = pendingCount,
            ResolvedCount = resolvedCount,
            ClosedCount = closedCount,
            StatusBreakdown = statusCounts,
            RecentGrievances = recentGrievances,
            OfficerWorkload = officerWorkload
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

        var resolvedCount = statusCounts.GetValueOrDefault(GrievanceStatus.Resolved, 0);
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
            .Count(a => a.OfficerId == userId && a.IsActive);
        
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

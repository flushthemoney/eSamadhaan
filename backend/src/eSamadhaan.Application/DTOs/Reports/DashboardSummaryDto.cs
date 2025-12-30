namespace eSamadhaan.Application.DTOs.Reports;

public class DashboardSummaryDto
{
    public int TotalGrievances { get; set; }
    public int SubmittedGrievances { get; set; }
    public int AssignedGrievances { get; set; }
    public int InReviewGrievances { get; set; }
    public int ResolvedGrievances { get; set; }
    public int ClosedGrievances { get; set; }
    public double ResolutionRate { get; set; }
    public double AverageResolutionTimeInDays { get; set; }
    public double AverageFeedbackRating { get; set; }
    
    // Recent activity
    public IEnumerable<RecentGrievanceDto> RecentGrievances { get; set; } = new List<RecentGrievanceDto>();
    public IEnumerable<RecentAssignmentDto> RecentAssignments { get; set; } = new List<RecentAssignmentDto>();
    
    // Top categories
    public Dictionary<string, int> TopCategories { get; set; } = new();
    
    // Performance metrics
    public int MyAssignedGrievances { get; set; }
    public int MyResolvedGrievances { get; set; }
}

public class RecentGrievanceDto
{
    public int Id { get; set; }
    public string GrievanceNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class RecentAssignmentDto
{
    public int GrievanceId { get; set; }
    public string GrievanceNumber { get; set; } = string.Empty;
    public string OfficerName { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}

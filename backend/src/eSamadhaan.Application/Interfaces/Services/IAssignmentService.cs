namespace eSamadhaan.Application.Interfaces.Services;

public interface IAssignmentService
{
    Task<int> AssignGrievanceAsync(int grievanceId, int officerId, int assignedByUserId);
    Task ReassignGrievanceAsync(int grievanceId, int newOfficerId, int reassignedByUserId, string? reason);
    Task<object?> GetActiveAssignmentByGrievanceIdAsync(int grievanceId);
    Task<IEnumerable<object>> GetAssignmentsByGrievanceIdAsync(int grievanceId);
    Task<IEnumerable<object>> GetAssignmentsByOfficerIdAsync(int officerId);
    Task<IEnumerable<object>> GetActiveAssignmentsByOfficerIdAsync(int officerId);
    Task DeactivateAssignmentAsync(int assignmentId);
    
    // LINQ-based queries
    Task<int> GetActiveAssignmentCountByOfficerAsync(int officerId);
    Task<Dictionary<int, int>> GetAssignmentCountByDepartmentAsync();
}

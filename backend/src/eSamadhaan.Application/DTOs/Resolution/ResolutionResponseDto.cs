namespace eSamadhaan.Application.DTOs.Resolution;

public class ResolutionResponseDto
{
    public int Id { get; set; }
    public int GrievanceId { get; set; }
    public string GrievanceNumber { get; set; } = string.Empty;
    public int ResolvedByOfficerId { get; set; }
    public string ResolvedByOfficerName { get; set; } = string.Empty;
    public string ResolutionRemarks { get; set; } = string.Empty;
    public DateTime ResolvedAt { get; set; }
}

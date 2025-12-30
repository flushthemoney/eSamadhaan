namespace eSamadhaan.Application.DTOs.Resolution;

public class CreateResolutionRequestDto
{
    public int GrievanceId { get; set; }
    public string ResolutionRemarks { get; set; } = string.Empty;
}

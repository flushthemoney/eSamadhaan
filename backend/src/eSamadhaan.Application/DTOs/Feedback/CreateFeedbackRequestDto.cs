namespace eSamadhaan.Application.DTOs.Feedback;

public class CreateFeedbackRequestDto
{
    public int GrievanceId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

namespace eSamadhaan.Application.DTOs.Feedback;

public class FeedbackResponseDto
{
    public int Id { get; set; }
    public int GrievanceId { get; set; }
    public string GrievanceNumber { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime SubmittedAt { get; set; }
}

using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Application.DTOs.Reports;

public class GrievanceStatusReportDto
{
    public GrievanceStatus Status { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}

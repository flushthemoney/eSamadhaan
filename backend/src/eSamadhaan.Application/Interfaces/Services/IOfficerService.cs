using eSamadhaan.Application.DTOs.Officer;

namespace eSamadhaan.Application.Interfaces.Services;

public interface IOfficerService
{
    Task<IEnumerable<OfficerGrievanceListDto>> GetDepartmentGrievancesAsync(int officerId);
    Task<OfficerGrievanceDetailDto> GetGrievanceDetailAsync(int grievanceId, int officerId);
    Task ChangeGrievanceStatusAsync(int grievanceId, int officerId, ChangeStatusRequestDto request);
}

using eSamadhaan.Application.DTOs.Auth;
using eSamadhaan.Application.DTOs.Officer;
using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Application.Interfaces.Services;

public interface IOfficerService
{
    Task<IEnumerable<OfficerGrievanceListDto>> GetDepartmentGrievancesAsync(int officerId, int? categoryId = null, GrievanceStatus? status = null, string? sortBy = null);
    Task<OfficerGrievanceDetailDto> GetGrievanceDetailAsync(int grievanceId, int officerId);
    Task ChangeGrievanceStatusAsync(int grievanceId, int officerId, ChangeStatusRequestDto request);
    Task<IEnumerable<OfficerListDto>> GetDepartmentOfficersAsync(int officerId);
}

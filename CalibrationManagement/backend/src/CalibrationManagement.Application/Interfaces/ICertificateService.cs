using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;

namespace CalibrationManagement.Application.Interfaces;

public interface ICertificateService
{
    Task<PagedResult<CertificateDto>> SearchAsync(CertificateQuery query, CancellationToken ct = default);
    Task<CertificateDto> GetAsync(int id, CancellationToken ct = default);
    Task<CertificateDto> CreateAsync(CertificateRequest request, int? actorUserId, CancellationToken ct = default);
    Task<CertificateDto> UpdateAsync(int id, CertificateRequest request, int? actorUserId, CancellationToken ct = default);
    Task DeleteAsync(int id, int? actorUserId, CancellationToken ct = default);
}

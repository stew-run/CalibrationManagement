using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;

namespace CalibrationManagement.Application.Interfaces;

public interface ICertificateRepository
{
    Task<PagedResult<CertificateDto>> SearchAsync(CertificateQuery query, CancellationToken ct = default);
    Task<CertificateDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(CertificateRequest request, int? actorUserId, CancellationToken ct = default);
    Task UpdateAsync(int id, CertificateRequest request, int? actorUserId, CancellationToken ct = default);
    Task DeleteAsync(int id, int? actorUserId, CancellationToken ct = default);
}

using Dapper;
using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;
using CalibrationManagement.Infrastructure.Data;

namespace CalibrationManagement.Infrastructure.Repositories;

public sealed class CertificateRepository : BaseRepository, ICertificateRepository
{
    public CertificateRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<PagedResult<CertificateDto>> SearchAsync(CertificateQuery q, CancellationToken ct = default)
        => QueryPagedAsync<CertificateDto>("dbo.usp_Certificates_Search", new
        {
            q.EquipmentId, q.Search, q.ExpiringWithinDays, Page = q.SafePage, PageSize = q.SafePageSize
        }, q.SafePage, q.SafePageSize, ct);

    public Task<CertificateDto?> GetByIdAsync(int id, CancellationToken ct = default)
        => QueryFirstOrDefaultAsync<CertificateDto>("dbo.usp_Certificates_GetById", new { CertificateId = id }, ct);

    public Task<int> CreateAsync(CertificateRequest r, int? actorUserId, CancellationToken ct = default)
        => QuerySingleAsync<int>("dbo.usp_Certificates_Create", new
        {
            r.EquipmentId, r.ResultId, r.CertificateNumber, r.AgencyId, IssueDate = r.IssueDate!.Value, r.ExpiryDate,
            r.FileName, r.ContentType, r.FileSizeBytes, r.StorageLocation, r.Notes, ActorUserId = actorUserId
        }, ct);

    public Task UpdateAsync(int id, CertificateRequest r, int? actorUserId, CancellationToken ct = default)
        => ExecuteAsync("dbo.usp_Certificates_Update", new
        {
            CertificateId = id, r.ResultId, r.CertificateNumber, r.AgencyId, IssueDate = r.IssueDate!.Value, r.ExpiryDate,
            r.FileName, r.ContentType, r.FileSizeBytes, r.StorageLocation, r.Notes, ActorUserId = actorUserId
        }, ct);

    public Task DeleteAsync(int id, int? actorUserId, CancellationToken ct = default)
        => ExecuteAsync("dbo.usp_Certificates_Delete", new { CertificateId = id, ActorUserId = actorUserId }, ct);
}

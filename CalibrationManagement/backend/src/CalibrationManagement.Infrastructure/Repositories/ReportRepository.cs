using Dapper;
using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;
using CalibrationManagement.Infrastructure.Data;

namespace CalibrationManagement.Infrastructure.Repositories;

public sealed class ReportRepository : BaseRepository, IReportRepository
{
    public ReportRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<PagedResult<CalibrationReportRowDto>> CalibrationAsync(CalibrationReportQuery q, bool all, CancellationToken ct = default)
    {
        var page = all ? 1 : q.SafePage;
        var pageSize = all ? 0 : q.SafePageSize;
        return QueryPagedAsync<CalibrationReportRowDto>("dbo.usp_Reports_Calibration", new
        {
            q.ReportType, q.From, q.To, q.DepartmentId, q.AgencyId, q.Search, Page = page, PageSize = pageSize
        }, page, pageSize, ct);
    }

    public async Task<IReadOnlyList<AgencySummaryDto>> AgencySummaryAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
        => await QueryAsync<AgencySummaryDto>("dbo.usp_Reports_AgencySummary", new { From = from, To = to }, ct);

    public async Task<IReadOnlyList<DataQualityIssueDto>> DataQualityAsync(bool onlyOpen, CancellationToken ct = default)
        => await QueryAsync<DataQualityIssueDto>("dbo.usp_Reports_DataQuality", new { OnlyOpen = onlyOpen }, ct);

    public Task ResolveDataQualityAsync(int issueId, bool isResolved, int? actorUserId, CancellationToken ct = default)
        => ExecuteAsync("dbo.usp_DataQuality_Resolve", new { IssueId = issueId, IsResolved = isResolved, ActorUserId = actorUserId }, ct);
}

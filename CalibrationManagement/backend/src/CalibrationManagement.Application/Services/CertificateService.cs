using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;
using CalibrationManagement.Application.Validators;

namespace CalibrationManagement.Application.Services;

public sealed class CertificateService : ICertificateService
{
    private static readonly string[] AllowedExtensions = { ".pdf", ".png", ".jpg", ".jpeg", ".tif", ".tiff" };
    private static readonly string[] AllowedContentTypes = { "application/pdf", "image/png", "image/jpeg", "image/tiff" };
    public const long MaxFileSizeBytes = 20L * 1024 * 1024;

    private readonly ICertificateRepository _repo;

    public CertificateService(ICertificateRepository repo) => _repo = repo;

    public Task<PagedResult<CertificateDto>> SearchAsync(CertificateQuery query, CancellationToken ct = default) => _repo.SearchAsync(query, ct);

    public async Task<CertificateDto> GetAsync(int id, CancellationToken ct = default)
        => await _repo.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Certificate {id} was not found.");

    public async Task<CertificateDto> CreateAsync(CertificateRequest request, int? actorUserId, CancellationToken ct = default)
    {
        if (request.EquipmentId <= 0) throw new AppValidationException("equipmentId", "Select an equipment item.");
        Validate(request);
        var id = await _repo.CreateAsync(request, actorUserId, ct);
        return await GetAsync(id, ct);
    }

    public async Task<CertificateDto> UpdateAsync(int id, CertificateRequest request, int? actorUserId, CancellationToken ct = default)
    {
        Validate(request);
        await _repo.UpdateAsync(id, request, actorUserId, ct);
        return await GetAsync(id, ct);
    }

    public Task DeleteAsync(int id, int? actorUserId, CancellationToken ct = default) => _repo.DeleteAsync(id, actorUserId, ct);

    internal static void Validate(CertificateRequest r)
    {
        r.CertificateNumber = Guard.Required("certificateNumber", r.CertificateNumber, 100);
        if (!r.IssueDate.HasValue) throw new AppValidationException("issueDate", "Issue date is required.");
        if (r.ExpiryDate.HasValue && r.ExpiryDate.Value < r.IssueDate.Value)
            throw new AppValidationException("expiryDate", "Expiry date cannot be earlier than the issue date.");
        r.FileName = Guard.Optional("fileName", r.FileName, 260);
        r.ContentType = Guard.Optional("contentType", r.ContentType, 100);
        r.StorageLocation = Guard.Optional("storageLocation", r.StorageLocation, 500);
        r.Notes = Guard.Optional("notes", r.Notes, 500);
        if (r.FileName is not null)
        {
            if (r.FileName.IndexOfAny(new[] { '/', '\\', ':', '*', '?', '"', '<', '>', '|' }) >= 0)
                throw new AppValidationException("fileName", "File name must not contain path or reserved characters.");
            if (!AllowedExtensions.Contains(Path.GetExtension(r.FileName).ToLowerInvariant()))
                throw new AppValidationException("fileName", $"File type not allowed. Allowed: {string.Join(", ", AllowedExtensions)}.");
        }
        if (r.ContentType is not null && !AllowedContentTypes.Contains(r.ContentType.ToLowerInvariant()))
            throw new AppValidationException("contentType", $"Content type not allowed. Allowed: {string.Join(", ", AllowedContentTypes)}.");
        if (r.FileSizeBytes is < 0 or > MaxFileSizeBytes)
            throw new AppValidationException("fileSizeBytes", "File size must be between 0 and 20 MB.");
    }
}

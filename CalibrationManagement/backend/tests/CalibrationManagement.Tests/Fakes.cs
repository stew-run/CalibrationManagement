using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;
using CalibrationManagement.Domain.Entities;

namespace CalibrationManagement.Tests;

public sealed class FakeUserRepository : IUserRepository
{
    public User? Record { get; set; }
    public List<(int UserId, bool Success)> Logins { get; } = new();
    public string? LastPasswordHash { get; private set; }
    public int CreatedAdmins { get; private set; }
    public bool UpdateCalled { get; private set; }

    public Task<User?> GetByUsernameAsync(string usernameOrEmail, CancellationToken ct = default)
        => Task.FromResult(Record is not null && (Record.Username == usernameOrEmail || Record.Email == usernameOrEmail) ? Record : null);

    public Task<User?> GetByIdAsync(int userId, CancellationToken ct = default)
        => Task.FromResult(Record is not null && Record.UserId == userId ? Record : null);

    public Task<PagedResult<UserDto>> ListAsync(UserQuery query, CancellationToken ct = default) => throw new NotImplementedException();

    public Task<int> CreateAsync(string username, string email, string fullName, string passwordHash, int roleId, int? actorUserId, CancellationToken ct = default)
    {
        LastPasswordHash = passwordHash;
        Record = new User { UserId = 99, Username = username, Email = email, FullName = fullName, PasswordHash = passwordHash, RoleId = roleId, Role = "Viewer", IsActive = true };
        return Task.FromResult(99);
    }

    public Task UpdateAsync(int userId, string email, string fullName, int roleId, bool isActive, int? actorUserId, CancellationToken ct = default)
    {
        UpdateCalled = true;
        return Task.CompletedTask;
    }

    public Task UpdateProfileAsync(int userId, string email, string fullName, CancellationToken ct = default) => Task.CompletedTask;

    public Task ChangePasswordAsync(int userId, string passwordHash, int? actorUserId, CancellationToken ct = default)
    {
        LastPasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task RecordLoginAsync(int userId, bool success, int maxFailures, int lockoutMinutes, CancellationToken ct = default)
    {
        Logins.Add((userId, success));
        return Task.CompletedTask;
    }

    public Task<int> CreateInitialAdminAsync(string username, string email, string fullName, string passwordHash, CancellationToken ct = default)
    {
        CreatedAdmins++;
        LastPasswordHash = passwordHash;
        return Task.FromResult(1);
    }

    public Task<IReadOnlyList<RoleDto>> ListRolesAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<RoleDto>>(Array.Empty<RoleDto>());
}

public sealed class FakeEquipmentRepository : IEquipmentRepository
{
    public EquipmentDto? Equipment { get; set; }

    public Task<PagedResult<EquipmentDto>> SearchAsync(EquipmentQuery query, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<EquipmentDto?> GetByIdAsync(int id, CancellationToken ct = default) => Task.FromResult(Equipment is not null && Equipment.EquipmentId == id ? Equipment : null);
    public Task<int> CreateAsync(EquipmentRequest request, int? actorUserId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task UpdateAsync(int id, EquipmentRequest request, int? actorUserId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task DeleteAsync(int id, int? actorUserId, CancellationToken ct = default) => throw new NotImplementedException();
}

public sealed class FakeResultRepository : IResultRepository
{
    public DateTime? CreatedDueDate { get; private set; }
    public ResultRequest? CreatedRequest { get; private set; }

    public Task<PagedResult<ResultDto>> SearchAsync(ResultQuery query, CancellationToken ct = default) => throw new NotImplementedException();

    public Task<ResultDto?> GetByIdAsync(int id, CancellationToken ct = default)
        => Task.FromResult<ResultDto?>(CreatedRequest is null ? null : new ResultDto
        {
            ResultId = id, EquipmentId = CreatedRequest.EquipmentId, Result = CreatedRequest.Result,
            CalibrationDate = CreatedRequest.CalibrationDate!.Value, DueDate = CreatedDueDate!.Value
        });

    public Task<int> CreateAsync(ResultRequest request, DateTime dueDate, int? actorUserId, CancellationToken ct = default)
    {
        CreatedRequest = request;
        CreatedDueDate = dueDate;
        return Task.FromResult(5);
    }

    public Task UpdateAsync(int id, ResultRequest request, DateTime dueDate, int? actorUserId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task DeleteAsync(int id, int? actorUserId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<HistoryCardDto?> GetHistoryAsync(int equipmentId, CancellationToken ct = default) => throw new NotImplementedException();
}

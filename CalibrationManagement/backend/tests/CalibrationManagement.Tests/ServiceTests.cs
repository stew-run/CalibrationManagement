using Microsoft.Extensions.Options;
using Xunit;
using CalibrationManagement.Api.Middleware;
using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.Configuration;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Services;
using CalibrationManagement.Domain.Constants;
using CalibrationManagement.Domain.Entities;
using CalibrationManagement.Infrastructure.Security;

namespace CalibrationManagement.Tests;

public class AuthServiceTests
{
    private readonly PasswordService _passwords = new(workFactor: 4);
    private readonly FakeUserRepository _users = new();
    private readonly AuthService _auth;

    public AuthServiceTests()
    {
        var jwt = new JwtSettings { Key = new string('s', 48), ExpiryMinutes = 30 };
        _auth = new AuthService(_users, _passwords, new JwtTokenService(Options.Create(jwt)), Options.Create(new SecuritySettings()));
        _users.Record = new User
        {
            UserId = 3, Username = "alice", Email = "alice@example.test", FullName = "Alice", Role = RoleNames.QualityManager,
            RoleId = 2, IsActive = true, PasswordHash = _passwords.Hash("Sup3r-Secret-Value")
        };
    }

    [Fact]
    public async Task Valid_credentials_return_a_token_and_record_a_successful_login()
    {
        var response = await _auth.LoginAsync(new LoginRequest { Username = "alice", Password = "Sup3r-Secret-Value" });
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.Equal("alice", response.User.Username);
        Assert.Contains((3, true), _users.Logins);
    }

    [Fact]
    public async Task Wrong_password_is_rejected_and_the_failure_is_recorded()
    {
        await Assert.ThrowsAsync<UnauthorizedAppException>(() => _auth.LoginAsync(new LoginRequest { Username = "alice", Password = "nope" }));
        Assert.Contains((3, false), _users.Logins);
    }

    [Fact]
    public async Task Unknown_user_gets_the_same_generic_error()
    {
        var ex = await Assert.ThrowsAsync<UnauthorizedAppException>(() => _auth.LoginAsync(new LoginRequest { Username = "ghost", Password = "whatever" }));
        Assert.Equal("Invalid username or password.", ex.Message);
    }

    [Fact]
    public async Task Locked_account_is_refused_even_with_the_right_password_and_no_extra_attempt_is_recorded()
    {
        _users.Record!.LockoutUntil = DateTime.UtcNow.AddMinutes(10);
        await Assert.ThrowsAsync<UnauthorizedAppException>(() => _auth.LoginAsync(new LoginRequest { Username = "alice", Password = "Sup3r-Secret-Value" }));
        Assert.Empty(_users.Logins);
    }

    [Fact]
    public async Task Disabled_account_cannot_sign_in()
    {
        _users.Record!.IsActive = false;
        await Assert.ThrowsAsync<UnauthorizedAppException>(() => _auth.LoginAsync(new LoginRequest { Username = "alice", Password = "Sup3r-Secret-Value" }));
    }

    [Fact]
    public async Task Change_password_requires_the_current_password_and_stores_a_new_hash()
    {
        await Assert.ThrowsAsync<AppValidationException>(() =>
            _auth.ChangePasswordAsync(3, new ChangePasswordRequest { CurrentPassword = "wrong", NewPassword = "An0ther-Strong-One" }));

        await _auth.ChangePasswordAsync(3, new ChangePasswordRequest { CurrentPassword = "Sup3r-Secret-Value", NewPassword = "An0ther-Strong-One" });
        Assert.NotNull(_users.LastPasswordHash);
        Assert.True(_passwords.Verify("An0ther-Strong-One", _users.LastPasswordHash));
    }
}

public class UserAndProvisioningTests
{
    private readonly PasswordService _passwords = new(workFactor: 4);
    private readonly FakeUserRepository _users = new();

    [Fact]
    public async Task Provisioning_rejects_weak_passwords_and_stores_only_a_hash()
    {
        var service = new AdminProvisioningService(_users, _passwords);
        await Assert.ThrowsAsync<AppValidationException>(() => service.ProvisionAsync("admin1", "a@x.test", "Admin One", "weak"));
        Assert.Equal(0, _users.CreatedAdmins);

        await service.ProvisionAsync("admin1", "a@x.test", "Admin One", "Str0ng-Admin-Secret");
        Assert.Equal(1, _users.CreatedAdmins);
        Assert.DoesNotContain("Str0ng-Admin-Secret", _users.LastPasswordHash);
        Assert.True(_passwords.Verify("Str0ng-Admin-Secret", _users.LastPasswordHash));
    }

    [Fact]
    public async Task Provisioning_rejects_bad_usernames()
        => await Assert.ThrowsAsync<AppValidationException>(() =>
            new AdminProvisioningService(_users, _passwords).ProvisionAsync("bad name!", "a@x.test", "Admin", "Str0ng-Admin-Secret"));

    [Fact]
    public async Task Administrator_cannot_deactivate_their_own_account()
    {
        var service = new UserService(_users, _passwords);
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.UpdateAsync(1, new UpdateUserRequest { Email = "a@x.test", FullName = "Admin", RoleId = 1, IsActive = false }, actorUserId: 1));
        Assert.False(_users.UpdateCalled);
    }
}

public class EquipmentServiceTests
{
    [Fact]
    public void Normalize_trims_values_and_turns_blank_optionals_into_null()
    {
        var clean = EquipmentService.Normalize(new EquipmentRequest { EquipmentCode = "  209 SM ", EquipmentName = " DIGITAL VERNIER ", Make = "   ", Status = "Active", CalibrationType = "External" });
        Assert.Equal("209 SM", clean.EquipmentCode);
        Assert.Equal("DIGITAL VERNIER", clean.EquipmentName);
        Assert.Null(clean.Make);
    }

    [Fact]
    public void Invalid_status_or_type_and_missing_code_are_rejected()
    {
        Assert.Throws<AppValidationException>(() => EquipmentService.Normalize(new EquipmentRequest { EquipmentCode = "A", EquipmentName = "B", Status = "Broken", CalibrationType = "External" }));
        Assert.Throws<AppValidationException>(() => EquipmentService.Normalize(new EquipmentRequest { EquipmentCode = "A", EquipmentName = "B", Status = "Active", CalibrationType = "Moon" }));
        Assert.Throws<AppValidationException>(() => EquipmentService.Normalize(new EquipmentRequest { EquipmentCode = "   ", EquipmentName = "B", Status = "Active", CalibrationType = "External" }));
    }
}

public class ResultServiceTests
{
    private readonly FakeResultRepository _results = new();
    private readonly FakeEquipmentRepository _equipment = new() { Equipment = new EquipmentDto { EquipmentId = 10, EquipmentCode = "200 SM", FrequencyMonths = 12 } };

    private ResultService Service() => new(_results, _equipment);

    [Fact]
    public async Task Due_date_is_derived_from_the_equipment_frequency_like_the_excel_plan()
    {
        // Plan example: calibrated 11.11.2025, 1 Year frequency -> next due 10.11.2026
        var created = await Service().CreateAsync(new ResultRequest { EquipmentId = 10, CalibrationDate = new DateTime(2025, 11, 11), Result = "OK" }, actorUserId: 1);
        Assert.Equal(new DateTime(2026, 11, 10), _results.CreatedDueDate);
        Assert.Equal(new DateTime(2026, 11, 10), created.DueDate);
    }

    [Fact]
    public async Task NOTOK_requires_an_action_taken()
        => await Assert.ThrowsAsync<AppValidationException>(() => Service().CreateAsync(
            new ResultRequest { EquipmentId = 10, CalibrationDate = new DateTime(2025, 11, 11), Result = "NOTOK" }, 1));

    [Fact]
    public async Task Future_calibration_date_and_unknown_result_values_are_rejected()
    {
        await Assert.ThrowsAsync<AppValidationException>(() => Service().CreateAsync(
            new ResultRequest { EquipmentId = 10, CalibrationDate = DateTime.Today.AddDays(30), Result = "OK" }, 1));
        await Assert.ThrowsAsync<AppValidationException>(() => Service().CreateAsync(
            new ResultRequest { EquipmentId = 10, CalibrationDate = new DateTime(2025, 1, 1), Result = "MAYBE" }, 1));
    }

    [Fact]
    public async Task Explicit_due_date_before_calibration_date_is_rejected()
        => await Assert.ThrowsAsync<AppValidationException>(() => Service().CreateAsync(
            new ResultRequest { EquipmentId = 10, CalibrationDate = new DateTime(2025, 11, 11), DueDate = new DateTime(2025, 11, 1), Result = "OK" }, 1));

    [Fact]
    public async Task Missing_frequency_requires_an_explicit_due_date()
    {
        _equipment.Equipment!.FrequencyMonths = null;
        await Assert.ThrowsAsync<AppValidationException>(() => Service().CreateAsync(
            new ResultRequest { EquipmentId = 10, CalibrationDate = new DateTime(2025, 11, 11), Result = "OK" }, 1));
    }
}

public class CertificateAndReportTests
{
    private static CertificateRequest Valid() => new()
    {
        EquipmentId = 1, CertificateNumber = "RMTL/01/325108573-A1", IssueDate = new DateTime(2025, 11, 11), ExpiryDate = new DateTime(2026, 11, 10),
        FileName = "cert.pdf", ContentType = "application/pdf", FileSizeBytes = 1024
    };

    [Fact]
    public void Valid_certificate_metadata_passes() => CertificateService.Validate(Valid());

    [Fact]
    public void Dangerous_file_names_types_sizes_and_dates_are_rejected()
    {
        var bad = Valid(); bad.FileName = "..\\evil.exe";
        Assert.Throws<AppValidationException>(() => CertificateService.Validate(bad));
        bad = Valid(); bad.FileName = "macro.docm";
        Assert.Throws<AppValidationException>(() => CertificateService.Validate(bad));
        bad = Valid(); bad.FileSizeBytes = CertificateService.MaxFileSizeBytes + 1;
        Assert.Throws<AppValidationException>(() => CertificateService.Validate(bad));
        bad = Valid(); bad.ExpiryDate = new DateTime(2020, 1, 1);
        Assert.Throws<AppValidationException>(() => CertificateService.Validate(bad));
    }

    [Theory]
    [InlineData("=SUM(A1:A2)", "'=SUM(A1:A2)")]
    [InlineData("plain", "plain")]
    [InlineData("a,b", "\"a,b\"")]
    [InlineData("say \"hi\"", "\"say \"\"hi\"\"\"")]
    public void Csv_cells_are_escaped_and_protected_against_formula_injection(string input, string expected)
        => Assert.Equal(expected, ReportService.CsvText(input));

    [Fact]
    public void Csv_export_has_a_utf8_bom_header_and_rows()
    {
        var bytes = ReportService.BuildCsv(new[]
        {
            new CalibrationReportRowDto { EquipmentCode = "200 SM", EquipmentName = "DIAL CALIPER", NextDueDate = new DateTime(2026, 11, 10), CalibrationStatus = "Valid", DaysToDue = 29 }
        });
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bytes.Take(3).ToArray());
        var text = System.Text.Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
        Assert.StartsWith("Equipment Code,Equipment Name", text);
        Assert.Contains("200 SM,DIAL CALIPER", text);
        Assert.Contains("2026-11-10", text);
    }
}

public class ExceptionMappingTests
{
    [Fact]
    public void Application_exceptions_map_to_the_expected_http_status_codes()
    {
        Assert.Equal(404, ExceptionHandlingMiddleware.Map(new NotFoundException("x")).Status);
        Assert.Equal(409, ExceptionHandlingMiddleware.Map(new ConflictException("x")).Status);
        Assert.Equal(422, ExceptionHandlingMiddleware.Map(new BusinessRuleException("x")).Status);
        Assert.Equal(401, ExceptionHandlingMiddleware.Map(new UnauthorizedAppException("x")).Status);
        var validation = ExceptionHandlingMiddleware.Map(new AppValidationException("field", "bad"));
        Assert.Equal(400, validation.Status);
        Assert.Contains("field", validation.Errors!.Keys);
    }

    [Fact]
    public void Unknown_exceptions_do_not_leak_details()
    {
        var mapped = ExceptionHandlingMiddleware.Map(new InvalidOperationException("secret connection string"));
        Assert.Equal(500, mapped.Status);
        Assert.DoesNotContain("secret", mapped.Detail);
    }
}

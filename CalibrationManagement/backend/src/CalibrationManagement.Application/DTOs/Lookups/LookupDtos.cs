using System.ComponentModel.DataAnnotations;

namespace CalibrationManagement.Application.DTOs;

public class LookupItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class FrequencyItem : LookupItem
{
    public int Months { get; set; }
}

public class LookupsDto
{
    public IReadOnlyList<LookupItem> Departments { get; set; } = Array.Empty<LookupItem>();
    public IReadOnlyList<LookupItem> Agencies { get; set; } = Array.Empty<LookupItem>();
    public IReadOnlyList<FrequencyItem> Frequencies { get; set; } = Array.Empty<FrequencyItem>();
    public IReadOnlyList<LookupItem> Roles { get; set; } = Array.Empty<LookupItem>();
}

public class CreateLookupRequest
{
    [Required, StringLength(150, MinimumLength = 2)] public string Name { get; set; } = string.Empty;
}

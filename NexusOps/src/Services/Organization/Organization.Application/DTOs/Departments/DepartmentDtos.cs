using System.ComponentModel.DataAnnotations;

namespace Organization.Application.DTOs.Departments;

public sealed record CreateDepartmentRequest(
    [Required, StringLength(150, MinimumLength = 2)] string Name,
    [StringLength(500)] string? Description);

public sealed record UpdateDepartmentRequest(
    [Required, StringLength(150, MinimumLength = 2)] string Name,
    [StringLength(500)] string? Description);

public sealed record DepartmentResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    int EmployeeCount,
    int TeamCount);

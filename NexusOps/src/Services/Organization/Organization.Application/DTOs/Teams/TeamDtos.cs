using System.ComponentModel.DataAnnotations;

namespace Organization.Application.DTOs.Teams;

public sealed record CreateTeamRequest(
    [Required, StringLength(150, MinimumLength = 2)] string Name,
    [StringLength(500)] string? Description,
    Guid DepartmentId);

public sealed record UpdateTeamRequest(
    [Required, StringLength(150, MinimumLength = 2)] string Name,
    [StringLength(500)] string? Description,
    Guid DepartmentId,
    bool IsActive);

public sealed record AssignTeamLeadRequest(Guid? EmployeeId);

public sealed record TeamResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid DepartmentId,
    string DepartmentName,
    Guid? TeamLeadId,
    string? TeamLeadName,
    bool IsActive,
    DateTime CreatedAt,
    int EmployeeCount);

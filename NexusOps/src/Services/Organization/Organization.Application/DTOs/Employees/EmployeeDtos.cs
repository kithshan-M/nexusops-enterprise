using System.ComponentModel.DataAnnotations;
using Organization.Domain.Enums;

namespace Organization.Application.DTOs.Employees;

public sealed record CreateEmployeeRequest(
    [Required, StringLength(450)] string IdentityUserId,
    [Required, StringLength(50)] string EmployeeNumber,
    [Required, StringLength(100, MinimumLength = 1)] string FirstName,
    [Required, StringLength(100, MinimumLength = 1)] string LastName,
    [StringLength(150)] string? JobTitle,
    [Phone, StringLength(30)] string? PhoneNumber,
    [EmailAddress, StringLength(256)] string? WorkEmail,
    Guid DepartmentId,
    Guid? TeamId,
    Guid? ManagerId,
    DateTime? DateJoined);

public sealed record UpdateEmployeeRequest(
    [Required, StringLength(100, MinimumLength = 1)] string FirstName,
    [Required, StringLength(100, MinimumLength = 1)] string LastName,
    [StringLength(150)] string? JobTitle,
    [Phone, StringLength(30)] string? PhoneNumber,
    [EmailAddress, StringLength(256)] string? WorkEmail,
    Guid DepartmentId,
    Guid? TeamId,
    Guid? ManagerId,
    DateTime? DateJoined);

public sealed record ChangeEmployeeStatusRequest(EmployeeStatus Status);

public sealed record EmployeeResponse(
    Guid Id,
    string IdentityUserId,
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string FullName,
    string? JobTitle,
    string? PhoneNumber,
    string? WorkEmail,
    Guid DepartmentId,
    string DepartmentName,
    Guid? TeamId,
    string? TeamName,
    Guid? ManagerId,
    string? ManagerName,
    EmployeeStatus Status,
    DateTime DateJoined,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

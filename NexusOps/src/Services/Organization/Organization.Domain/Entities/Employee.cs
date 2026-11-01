using Organization.Domain.Enums;

namespace Organization.Domain.Entities;

public class Employee
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Reference to the Identity microservice user.
    // This is not a foreign key to the Identity database.
    public string IdentityUserId { get; set; } = string.Empty;

    public string EmployeeNumber { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? JobTitle { get; set; }

    public string? PhoneNumber { get; set; }

    public string? WorkEmail { get; set; }

    public Guid DepartmentId { get; set; }

    public Department? Department { get; set; }

    public Guid? TeamId { get; set; }

    public Team? Team { get; set; }

    public Guid? ManagerId { get; set; }

    public Employee? Manager { get; set; }

    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

    public DateTime DateJoined { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
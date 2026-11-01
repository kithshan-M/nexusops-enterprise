namespace Organization.Domain.Entities;

public class Team
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid DepartmentId { get; set; }

    public Department? Department { get; set; }

    public Guid? TeamLeadId { get; set; }

    public Employee? TeamLead { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
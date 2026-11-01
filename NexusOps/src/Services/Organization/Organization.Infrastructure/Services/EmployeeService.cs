using Microsoft.EntityFrameworkCore;
using Organization.Application.DTOs.Employees;
using Organization.Application.Exceptions;
using Organization.Application.Interfaces;
using Organization.Domain.Entities;
using Organization.Domain.Enums;
using Organization.Infrastructure.Persistence;

namespace Organization.Infrastructure.Services;

public sealed class EmployeeService(OrganizationDbContext db) : IEmployeeService
{
    public async Task<IReadOnlyList<EmployeeResponse>> GetAllAsync(Guid? departmentId = null, Guid? teamId = null, EmployeeStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = QueryEmployees();
        if (departmentId.HasValue) query = query.Where(x => x.DepartmentId == departmentId.Value);
        if (teamId.HasValue) query = query.Where(x => x.TeamId == teamId.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        var employees = await query.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync(cancellationToken);
        return employees.Select(ToResponse).ToList();
    }

    public async Task<EmployeeResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await QueryEmployees().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new OrganizationNotFoundException($"Employee '{id}' was not found.");
        return ToResponse(employee);
    }

    public async Task<EmployeeResponse> GetByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken = default)
    {
        var employee = await QueryEmployees().FirstOrDefaultAsync(x => x.IdentityUserId == identityUserId, cancellationToken)
            ?? throw new OrganizationNotFoundException($"No employee profile exists for Identity user '{identityUserId}'.");
        return ToResponse(employee);
    }

    public async Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        var identityUserId = request.IdentityUserId.Trim();
        var employeeNumber = request.EmployeeNumber.Trim();
        var email = Clean(request.WorkEmail)?.ToLowerInvariant();
        if (await db.Employees.AnyAsync(x => x.IdentityUserId == identityUserId, cancellationToken))
            throw new OrganizationConflictException("An employee profile already exists for this Identity user.");
        if (await db.Employees.AnyAsync(x => x.EmployeeNumber.ToLower() == employeeNumber.ToLower(), cancellationToken))
            throw new OrganizationConflictException($"Employee number '{employeeNumber}' is already in use.");
        if (email is not null && await db.Employees.AnyAsync(x => x.WorkEmail != null && x.WorkEmail.ToLower() == email, cancellationToken))
            throw new OrganizationConflictException($"Work email '{email}' is already in use.");

        await ValidateAssignmentsAsync(request.DepartmentId, request.TeamId, request.ManagerId, null, cancellationToken);
        var employee = new Employee
        {
            IdentityUserId = identityUserId,
            EmployeeNumber = employeeNumber,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            JobTitle = Clean(request.JobTitle),
            PhoneNumber = Clean(request.PhoneNumber),
            WorkEmail = email,
            DepartmentId = request.DepartmentId,
            TeamId = request.TeamId,
            ManagerId = request.ManagerId,
            DateJoined = request.DateJoined ?? DateTime.UtcNow
        };
        db.Employees.Add(employee);
        await db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(employee.Id, cancellationToken);
    }

    public async Task<EmployeeResponse> UpdateAsync(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new OrganizationNotFoundException($"Employee '{id}' was not found.");
        var email = Clean(request.WorkEmail)?.ToLowerInvariant();
        if (email is not null && await db.Employees.AnyAsync(x => x.Id != id && x.WorkEmail != null && x.WorkEmail.ToLower() == email, cancellationToken))
            throw new OrganizationConflictException($"Work email '{email}' is already in use.");
        await ValidateAssignmentsAsync(request.DepartmentId, request.TeamId, request.ManagerId, id, cancellationToken);

        employee.FirstName = request.FirstName.Trim();
        employee.LastName = request.LastName.Trim();
        employee.JobTitle = Clean(request.JobTitle);
        employee.PhoneNumber = Clean(request.PhoneNumber);
        employee.WorkEmail = email;
        employee.DepartmentId = request.DepartmentId;
        employee.TeamId = request.TeamId;
        employee.ManagerId = request.ManagerId;
        if (request.DateJoined.HasValue) employee.DateJoined = request.DateJoined.Value;
        employee.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<EmployeeResponse> ChangeStatusAsync(Guid id, EmployeeStatus status, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(status)) throw new OrganizationValidationException("The supplied employee status is invalid.");
        var employee = await db.Employees.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new OrganizationNotFoundException($"Employee '{id}' was not found.");
        employee.Status = status;
        employee.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    private IQueryable<Employee> QueryEmployees() => db.Employees.AsNoTracking()
        .Include(x => x.Department)
        .Include(x => x.Team)
        .Include(x => x.Manager);

    private async Task ValidateAssignmentsAsync(Guid departmentId, Guid? teamId, Guid? managerId, Guid? employeeId, CancellationToken cancellationToken)
    {
        if (!await db.Departments.AnyAsync(x => x.Id == departmentId && x.IsActive, cancellationToken))
            throw new OrganizationValidationException("A valid active department is required.");

        if (teamId.HasValue && !await db.Teams.AnyAsync(x => x.Id == teamId.Value && x.DepartmentId == departmentId && x.IsActive, cancellationToken))
            throw new OrganizationValidationException("The selected team must be active and belong to the selected department.");

        if (managerId.HasValue)
        {
            if (managerId == employeeId)
                throw new OrganizationValidationException("An employee cannot be their own reporting manager.");
            var manager = await db.Employees.FirstOrDefaultAsync(x => x.Id == managerId.Value, cancellationToken)
                ?? throw new OrganizationNotFoundException($"Manager employee '{managerId}' was not found.");
            if (manager.DepartmentId != departmentId)
                throw new OrganizationValidationException("The reporting manager must belong to the same department.");
            if (employeeId.HasValue && await WouldCreateManagerCycleAsync(employeeId.Value, managerId.Value, cancellationToken))
                throw new OrganizationValidationException("This reporting-manager assignment would create a management cycle.");
        }
    }

    private async Task<bool> WouldCreateManagerCycleAsync(Guid employeeId, Guid proposedManagerId, CancellationToken cancellationToken)
    {
        var visited = new HashSet<Guid>();
        Guid? currentId = proposedManagerId;
        while (currentId.HasValue && visited.Add(currentId.Value))
        {
            if (currentId.Value == employeeId) return true;
            currentId = await db.Employees.Where(x => x.Id == currentId.Value)
                .Select(x => x.ManagerId).FirstOrDefaultAsync(cancellationToken);
        }
        return false;
    }

    private static EmployeeResponse ToResponse(Employee x) => new(
        x.Id, x.IdentityUserId, x.EmployeeNumber, x.FirstName, x.LastName,
        $"{x.FirstName} {x.LastName}", x.JobTitle, x.PhoneNumber, x.WorkEmail,
        x.DepartmentId, x.Department?.Name ?? string.Empty,
        x.TeamId, x.Team?.Name,
        x.ManagerId, x.Manager is null ? null : $"{x.Manager.FirstName} {x.Manager.LastName}",
        x.Status, x.DateJoined, x.CreatedAt, x.UpdatedAt);

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

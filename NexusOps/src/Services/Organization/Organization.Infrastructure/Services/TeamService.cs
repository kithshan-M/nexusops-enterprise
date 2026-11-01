using Microsoft.EntityFrameworkCore;
using Organization.Application.DTOs.Teams;
using Organization.Application.Exceptions;
using Organization.Application.Interfaces;
using Organization.Infrastructure.Persistence;
using TeamEntity = Organization.Domain.Entities.Team;

namespace Organization.Infrastructure.Services;

public sealed class TeamService(OrganizationDbContext db) : ITeamService
{
    public async Task<IReadOnlyList<TeamResponse>> GetAllAsync(Guid? departmentId = null, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = db.Teams.AsNoTracking().Include(x => x.Department).Include(x => x.TeamLead).Include(x => x.Employees).AsQueryable();
        if (departmentId.HasValue) query = query.Where(x => x.DepartmentId == departmentId.Value);
        if (!includeInactive) query = query.Where(x => x.IsActive);
        var teams = await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
        return teams.Select(ToResponse).ToList();
    }

    public async Task<TeamResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var team = await db.Teams.AsNoTracking().Include(x => x.Department).Include(x => x.TeamLead).Include(x => x.Employees)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new OrganizationNotFoundException($"Team '{id}' was not found.");
        return ToResponse(team);
    }

    public async Task<TeamResponse> CreateAsync(CreateTeamRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureActiveDepartmentAsync(request.DepartmentId, cancellationToken);
        var name = request.Name.Trim();
        if (await db.Teams.AnyAsync(x => x.DepartmentId == request.DepartmentId && x.Name.ToLower() == name.ToLower(), cancellationToken))
            throw new OrganizationConflictException($"A team named '{name}' already exists in this department.");
        var team = new TeamEntity { Name = name, Description = Clean(request.Description), DepartmentId = request.DepartmentId };
        db.Teams.Add(team);
        await db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(team.Id, cancellationToken);
    }

    public async Task<TeamResponse> UpdateAsync(Guid id, UpdateTeamRequest request, CancellationToken cancellationToken = default)
    {
        var team = await db.Teams.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new OrganizationNotFoundException($"Team '{id}' was not found.");
        await EnsureActiveDepartmentAsync(request.DepartmentId, cancellationToken);
        var name = request.Name.Trim();
        if (await db.Teams.AnyAsync(x => x.Id != id && x.DepartmentId == request.DepartmentId && x.Name.ToLower() == name.ToLower(), cancellationToken))
            throw new OrganizationConflictException($"A team named '{name}' already exists in this department.");
        var departmentChanged = team.DepartmentId != request.DepartmentId;
        if (departmentChanged && await db.Employees.AnyAsync(x => x.TeamId == id, cancellationToken))
            throw new OrganizationValidationException("A team with assigned employees cannot be moved to another department. Reassign its employees first.");

        if (team.TeamLeadId.HasValue)
        {
            var lead = await db.Employees.FirstOrDefaultAsync(x => x.Id == team.TeamLeadId.Value, cancellationToken);
            if (lead is not null && lead.DepartmentId != request.DepartmentId)
                throw new OrganizationValidationException("The current team lead must belong to the team's department.");
        }

        team.Name = name;
        team.Description = Clean(request.Description);
        team.DepartmentId = request.DepartmentId;
        team.IsActive = request.IsActive;
        await db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<TeamResponse> AssignTeamLeadAsync(Guid id, Guid? employeeId, CancellationToken cancellationToken = default)
    {
        var team = await db.Teams.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new OrganizationNotFoundException($"Team '{id}' was not found.");
        if (employeeId.HasValue)
        {
            var employee = await db.Employees.FirstOrDefaultAsync(x => x.Id == employeeId.Value, cancellationToken)
                ?? throw new OrganizationNotFoundException($"Employee '{employeeId}' was not found.");
            if (employee.DepartmentId != team.DepartmentId)
                throw new OrganizationValidationException("The team lead must belong to the same department as the team.");
        }
        team.TeamLeadId = employeeId;
        await db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    private async Task EnsureActiveDepartmentAsync(Guid id, CancellationToken cancellationToken)
    {
        var active = await db.Departments.AnyAsync(x => x.Id == id && x.IsActive, cancellationToken);
        if (!active) throw new OrganizationValidationException("A valid active department is required.");
    }

    private static TeamResponse ToResponse(TeamEntity x) => new(
        x.Id, x.Name, x.Description, x.DepartmentId, x.Department?.Name ?? string.Empty,
        x.TeamLeadId, x.TeamLead is null ? null : $"{x.TeamLead.FirstName} {x.TeamLead.LastName}",
        x.IsActive, x.CreatedAt, x.Employees.Count);

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

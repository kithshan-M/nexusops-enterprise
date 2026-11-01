using Microsoft.EntityFrameworkCore;
using Organization.Application.DTOs.Departments;
using Organization.Application.Exceptions;
using Organization.Application.Interfaces;
using Organization.Infrastructure.Persistence;
using DepartmentEntity = Organization.Domain.Entities.Department;

namespace Organization.Infrastructure.Services;

public sealed class DepartmentService(OrganizationDbContext db) : IDepartmentService
{
    public async Task<IReadOnlyList<DepartmentResponse>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = db.Departments.AsNoTracking();
        if (!includeInactive) query = query.Where(x => x.IsActive);
        return await query.OrderBy(x => x.Name)
            .Select(x => new DepartmentResponse(x.Id, x.Name, x.Description, x.IsActive, x.CreatedAt, x.Employees.Count, x.Teams.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<DepartmentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await db.Departments.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new DepartmentResponse(x.Id, x.Name, x.Description, x.IsActive, x.CreatedAt, x.Employees.Count, x.Teams.Count))
            .FirstOrDefaultAsync(cancellationToken);
        return item ?? throw new OrganizationNotFoundException($"Department '{id}' was not found.");
    }

    public async Task<DepartmentResponse> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        if (await db.Departments.AnyAsync(x => x.Name.ToLower() == name.ToLower(), cancellationToken))
            throw new OrganizationConflictException($"A department named '{name}' already exists.");

        var entity = new DepartmentEntity { Name = name, Description = Clean(request.Description) };
        db.Departments.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<DepartmentResponse> UpdateAsync(Guid id, UpdateDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.Departments.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new OrganizationNotFoundException($"Department '{id}' was not found.");
        var name = request.Name.Trim();
        if (await db.Departments.AnyAsync(x => x.Id != id && x.Name.ToLower() == name.ToLower(), cancellationToken))
            throw new OrganizationConflictException($"A department named '{name}' already exists.");
        entity.Name = name;
        entity.Description = Clean(request.Description);
        await db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Departments.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new OrganizationNotFoundException($"Department '{id}' was not found.");
        entity.IsActive = false;
        await db.SaveChangesAsync(cancellationToken);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

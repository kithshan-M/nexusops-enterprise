using Organization.Application.DTOs.Departments;

namespace Organization.Application.Interfaces;

public interface IDepartmentService
{
    Task<IReadOnlyList<DepartmentResponse>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<DepartmentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DepartmentResponse> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default);
    Task<DepartmentResponse> UpdateAsync(Guid id, UpdateDepartmentRequest request, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}

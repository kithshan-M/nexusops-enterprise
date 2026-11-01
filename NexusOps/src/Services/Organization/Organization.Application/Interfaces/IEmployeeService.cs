using Organization.Application.DTOs.Employees;
using Organization.Domain.Enums;

namespace Organization.Application.Interfaces;

public interface IEmployeeService
{
    Task<IReadOnlyList<EmployeeResponse>> GetAllAsync(Guid? departmentId = null, Guid? teamId = null, EmployeeStatus? status = null, CancellationToken cancellationToken = default);
    Task<EmployeeResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EmployeeResponse> GetByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken = default);
    Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeResponse> UpdateAsync(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeResponse> ChangeStatusAsync(Guid id, EmployeeStatus status, CancellationToken cancellationToken = default);
}

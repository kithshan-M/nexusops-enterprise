using Organization.Application.DTOs.Teams;

namespace Organization.Application.Interfaces;

public interface ITeamService
{
    Task<IReadOnlyList<TeamResponse>> GetAllAsync(Guid? departmentId = null, bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<TeamResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TeamResponse> CreateAsync(CreateTeamRequest request, CancellationToken cancellationToken = default);
    Task<TeamResponse> UpdateAsync(Guid id, UpdateTeamRequest request, CancellationToken cancellationToken = default);
    Task<TeamResponse> AssignTeamLeadAsync(Guid id, Guid? employeeId, CancellationToken cancellationToken = default);
}

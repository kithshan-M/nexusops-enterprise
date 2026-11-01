using Microsoft.AspNetCore.Mvc;
using Organization.Application.DTOs.Teams;
using Organization.Application.Interfaces;

namespace Organization.Api.Controllers;

[ApiController]
[Route("api/teams")]
public sealed class TeamsController(ITeamService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? departmentId, [FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default) =>
        Ok(await service.GetAllAsync(departmentId, includeInactive, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(CreateTeamRequest request, CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateTeamRequest request, CancellationToken cancellationToken) =>
        Ok(await service.UpdateAsync(id, request, cancellationToken));

    [HttpPut("{id:guid}/team-lead")]
    public async Task<IActionResult> AssignTeamLead(Guid id, AssignTeamLeadRequest request, CancellationToken cancellationToken) =>
        Ok(await service.AssignTeamLeadAsync(id, request.EmployeeId, cancellationToken));
}

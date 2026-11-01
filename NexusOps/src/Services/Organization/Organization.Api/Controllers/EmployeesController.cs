using Microsoft.AspNetCore.Mvc;
using Organization.Application.DTOs.Employees;
using Organization.Application.Interfaces;
using Organization.Domain.Enums;

namespace Organization.Api.Controllers;

[ApiController]
[Route("api/employees")]
public sealed class EmployeesController(IEmployeeService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? teamId,
        [FromQuery] EmployeeStatus? status,
        CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(departmentId, teamId, status, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.GetByIdAsync(id, cancellationToken));

    [HttpGet("by-identity/{identityUserId}")]
    public async Task<IActionResult> GetByIdentityUserId(string identityUserId, CancellationToken cancellationToken) =>
        Ok(await service.GetByIdentityUserIdAsync(identityUserId, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken) =>
        Ok(await service.UpdateAsync(id, request, cancellationToken));

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, ChangeEmployeeStatusRequest request, CancellationToken cancellationToken) =>
        Ok(await service.ChangeStatusAsync(id, request.Status, cancellationToken));
}

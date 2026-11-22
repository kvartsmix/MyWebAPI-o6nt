using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebAPI.Dtos;
using MyWebAPI.Models;
using MyWebAPI.Services;

namespace MyWebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StorageTenantController : ControllerBase
{
    private readonly IStorageTenantService _tenantService;

    public StorageTenantController(IStorageTenantService tenantService) =>
        _tenantService = tenantService;

    [HttpGet]
    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.User)}")]
    public async Task<ActionResult<IReadOnlyList<StorageTenantResponseDto>>> GetAll([FromQuery] int? count) =>
        Ok(await _tenantService.GetAllAsync(count));

    [HttpGet("{id}")]
    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.User)}")]
    public async Task<ActionResult<StorageTenantResponseDto>> GetById(string id) =>
        Ok(await _tenantService.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.User)}")]
    public async Task<ActionResult<StorageTenantResponseDto>> Create([FromBody] StorageTenantCreateDto dto)
    {
        var newTenant = await _tenantService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = newTenant.Id }, newTenant);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(Roles.Admin))]
    public async Task<ActionResult<StorageTenantResponseDto>> FullUpdate(string id, [FromBody] StorageTenantCreateDto dto) =>
        Ok(await _tenantService.FullUpdateAsync(id, dto));

    [HttpPatch("{id}")]
    [Authorize(Roles = nameof(Roles.Admin))]
    public async Task<ActionResult<StorageTenantResponseDto>> PartialUpdate(string id, [FromBody] StorageTenantPatchDto dto) =>
        Ok(await _tenantService.PartialUpdateAsync(id, dto));

    [HttpPatch("{id}/pay-debt")]
    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.User)}")]
    public async Task<ActionResult<StorageTenantResponseDto>> PayDebt(string id) =>
        Ok(await _tenantService.PayDebtAsync(id));

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(Roles.Admin))]
    public async Task<ActionResult> Delete(string id)
    {
        await _tenantService.DeleteAsync(id);
        return NoContent();
    }
}
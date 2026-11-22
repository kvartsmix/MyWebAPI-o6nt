using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebAPI.Dtos;
using MyWebAPI.Models;
using MyWebAPI.Services;

namespace MyWebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StorageBoxController : ControllerBase
{
    private readonly IStorageBoxService _service;

    public StorageBoxController(IStorageBoxService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.User)}")]
    public async Task<ActionResult<IReadOnlyList<StorageBoxResponseDto>>> GetAll([FromQuery] int? count)
    {
        var boxes = await _service.GetAllAsync(count);
        return Ok(boxes);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.User)}")]
    public async Task<ActionResult<StorageBoxResponseDto>> GetById(string id)
    {
        var box = await _service.GetByIdAsync(id);
        return Ok(box);
    }

    [HttpGet("available")]
    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.User)}")]
    public async Task<ActionResult<IReadOnlyList<StorageBoxResponseDto>>> GetAvailable(
        [FromQuery] StorageBoxType? type,
        [FromQuery] double? minVolume)
    {
        var boxes = await _service.GetAvailableAsync(type, minVolume);
        return Ok(boxes);
    }

    [HttpPost]
    [Authorize(Roles = nameof(Roles.Admin))]
    public async Task<ActionResult<StorageBoxResponseDto>> Create([FromBody] StorageBoxCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(Roles.Admin))]
    public async Task<ActionResult<StorageBoxResponseDto>> FullUpdate(string id, [FromBody] StorageBoxCreateDto dto)
    {
        var updated = await _service.FullUpdateAsync(id, dto);
        return Ok(updated);
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = nameof(Roles.Admin))]
    public async Task<ActionResult<StorageBoxResponseDto>> PartialUpdate(string id, [FromBody] StorageBoxPatchDto dto)
    {
        var updated = await _service.PartialUpdateAsync(id, dto);
        return Ok(updated);
    }

    [HttpPost("{id}/disinfect")]
    [Authorize(Roles = nameof(Roles.Admin))]
    public async Task<ActionResult<StorageBoxResponseDto>> Disinfect(string id)
    {
        var result = await _service.DisinfectAsync(id);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(Roles.Admin))]
    public async Task<IActionResult> Delete(string id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
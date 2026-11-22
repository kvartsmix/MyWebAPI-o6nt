using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebAPI.Dtos;
using MyWebAPI.Models;
using MyWebAPI.Services;

namespace MyWebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StorageLeaseContractController : ControllerBase
{
    private readonly IStorageLeaseContractService _contractService;

    public StorageLeaseContractController(IStorageLeaseContractService contractService) =>
        _contractService = contractService;

    [HttpGet]
    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.User)}")]
    public async Task<ActionResult<IReadOnlyList<StorageLeaseContractResponseDto>>> GetAll([FromQuery] int? count) =>
        Ok(await _contractService.GetAllAsync(count));

    [HttpGet("{id}")]
    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.User)}")]
    public async Task<ActionResult<StorageLeaseContractResponseDto>> GetById(string id) =>
        Ok(await _contractService.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.User)}")]
    public async Task<ActionResult<StorageLeaseContractResponseDto>> Create([FromBody] StorageLeaseContractCreateDto dto)
    {
        var contract = await _contractService.CreateContractAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = contract.Id }, contract);
    }

    [HttpPatch("{id}/pay-deposit/{value}")]
    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.User)}")]
    public async Task<ActionResult<StorageLeaseContractResponseDto>> PayDeposit(string id, decimal value) =>
        Ok(await _contractService.PayDepositAsync(id, value));

    [HttpGet("{id}/payment-notification")]
    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.User)}")]
    public async Task<ActionResult<string>> GetNotification(string id) =>
        Ok(await _contractService.GetPaymentNotificationAsync(id));

    [HttpPatch("{id}/check-overdue")]
    [Authorize(Roles = nameof(Roles.Admin))]
    public async Task<ActionResult<StorageLeaseContractResponseDto>> CheckOverdue(string id) =>
        Ok(await _contractService.CheckOverdueAndSuspendAsync(id));

    [HttpPatch("{id}/close")]
    [Authorize(Roles = nameof(Roles.Admin))]
    public async Task<ActionResult<StorageLeaseContractResponseDto>> Close(string id) =>
        Ok(await _contractService.CloseContractAsync(id));

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(Roles.Admin))]
    public async Task<ActionResult<StorageLeaseContractResponseDto>> FullUpdate(string id, [FromBody] StorageLeaseContractCreateDto dto) =>
        Ok(await _contractService.UpdateContractAsync(id, dto));

    [HttpPatch("{id}")]
    [Authorize(Roles = nameof(Roles.Admin))]
    public async Task<ActionResult<StorageLeaseContractResponseDto>> PartialUpdate(string id, [FromBody] StorageLeaseContractPatchDto dto) =>
        Ok(await _contractService.PatchContractAsync(id, dto));

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(Roles.Admin))]
    public async Task<ActionResult> Delete(string id)
    {
        await _contractService.DeleteContractAsync(id);
        return NoContent();
    }
}
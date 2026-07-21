using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.Addresses.Commands.CreateAddress;
using CRM.Application.Features.Addresses.Commands.DeleteAddress;
using CRM.Application.Features.Addresses.Commands.UpdateAddress;
using CRM.Application.Features.Addresses.DTOs;
using CRM.Application.Features.Addresses.Queries.GetAddressesByCustomer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AddressController : ControllerBase
{
    private readonly IMediator _mediator;

    public AddressController(IMediator mediator) => _mediator = mediator;

    // Đọc: Sale + Manager + Accountant (đồng bộ với CustomerController)
    [HttpGet("customer/{khachHangId}")]
    [Authorize(Policy = Policies.CustomerReadAccess)]
    public async Task<IActionResult> GetByCustomer(ulong khachHangId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAddressesByCustomerQuery(khachHangId), ct);
        return Ok(ApiResponse<List<AddressDto>>.Ok(result));
    }

    // Ghi: chỉ Sale + Manager, Accountant không được sửa địa chỉ khách hàng
    [HttpPost("customer/{khachHangId}")]
    [Authorize(Policy = Policies.SalesTeam)]
    public async Task<IActionResult> Create(ulong khachHangId, [FromBody] CreateAddressCommand command, CancellationToken ct)
    {
        if (khachHangId != command.KhachHangId)
        {
            command = command with { KhachHangId = khachHangId };
        }
        var result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<AddressDto>.Ok(result, "Thêm địa chỉ thành công."));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Policies.SalesTeam)]
    public async Task<IActionResult> Update(ulong id, [FromBody] UpdateAddressCommand command, CancellationToken ct)
    {
        if (id != command.Id)
        {
            command = command with { Id = id };
        }
        var result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<AddressDto>.Ok(result, "Cập nhật địa chỉ thành công."));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Policies.SalesTeam)]
    public async Task<IActionResult> Delete(ulong id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteAddressCommand(id), ct);
        return Ok(ApiResponse<bool>.Ok(result, "Đã xóa địa chỉ."));
    }
}
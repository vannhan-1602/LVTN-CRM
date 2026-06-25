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

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.CustomerReadAccess)]
public class AddressController : ControllerBase
{
    private readonly IMediator _mediator;
    public AddressController(IMediator mediator) => _mediator = mediator;

    [HttpGet("customer/{khachHangId:long}")]
    public async Task<IActionResult> GetByCustomer(ulong khachHangId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAddressesByCustomerQuery(khachHangId), ct);
        return Ok(ApiResponse<List<AddressDto>>.Ok(result));
    }

    /// <summary>POST /api/address/customer/{khachHangId} — KhachHangId từ route, tránh trùng lặp trong body</summary>
    [HttpPost("customer/{khachHangId:long}")]
    [Authorize(Policy = Policies.SalesTeam)]
    public async Task<IActionResult> Create(ulong khachHangId, [FromBody] CreateAddressRequestDto req, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateAddressCommand(
            khachHangId,
            req.DiaChiChiTiet, req.TinhThanh, req.QuanHuyen, req.PhuongXa,
            req.LoaiDiaChi, req.IsDefault), ct);
        return Ok(ApiResponse<AddressDto>.Ok(result, "Thêm địa chỉ thành công."));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = Policies.SalesTeam)]
    public async Task<IActionResult> Update(ulong id, [FromBody] UpdateAddressRequestDto req, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateAddressCommand(
            id, req.DiaChiChiTiet, req.TinhThanh, req.QuanHuyen, req.PhuongXa,
            req.LoaiDiaChi, req.IsDefault), ct);
        return Ok(ApiResponse<AddressDto>.Ok(result, "Cập nhật địa chỉ thành công."));
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = Policies.SalesTeam)]
    public async Task<IActionResult> Delete(ulong id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteAddressCommand(id), ct);
        return Ok(ApiResponse.Ok("Xóa địa chỉ thành công."));
    }
}
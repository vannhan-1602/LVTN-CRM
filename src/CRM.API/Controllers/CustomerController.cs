using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.Customers.Commands.CreateCustomer;
using CRM.Application.Features.Customers.Commands.DeleteCustomer;
using CRM.Application.Features.Customers.Commands.UpdateCustomer;
using CRM.Application.Features.Customers.DTOs;
using CRM.Application.Features.Customers.Queries.GetAllCustomers;
using CRM.Application.Features.Customers.Queries.GetCustomerById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.SalesTeam)]   // Sale + Manager 
public class CustomerController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CustomerDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] ushort? loaiKhachHangId = null,
        [FromQuery] ushort? tinhTrangId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetAllCustomersQuery(pageNumber, pageSize, search, loaiKhachHangId, tinhTrangId),
            cancellationToken);

        return Ok(ApiResponse<PagedResult<CustomerDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(ulong id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCustomerByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<CustomerDto>.Ok(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCustomerRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateCustomerCommand(
                request.TenKhachHang,
                request.LoaiKhachHangId,
                request.TinhTrangId,
                request.Email,
                request.SoDienThoai,
                request.MaSoThue,
                request.NhanVienPhuTrachId),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            ApiResponse<CustomerDto>.Ok(result, "Tạo khách hàng thành công."));
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        ulong id,
        [FromBody] UpdateCustomerRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateCustomerCommand(
                id,
                request.TenKhachHang,
                request.LoaiKhachHangId,
                request.TinhTrangId,
                request.Email,
                request.SoDienThoai,
                request.MaSoThue,
                request.NhanVienPhuTrachId),
            cancellationToken);

        return Ok(ApiResponse<CustomerDto>.Ok(result, "Cập nhật khách hàng thành công."));
    }

    // ManagerOnly
    [HttpDelete("{id:long}")]
    [Authorize(Policy = Policies.ManagerOnly)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(ulong id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCustomerCommand(id), cancellationToken);
        return Ok(ApiResponse.Ok("Xóa khách hàng thành công (soft delete)."));
    }
}

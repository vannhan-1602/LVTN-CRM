using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.Customers.Commands.CreateCustomer;
using CRM.Application.Features.Customers.Commands.DeleteCustomer;
using CRM.Application.Features.Customers.Commands.RestoreCustomer;
using CRM.Application.Features.Customers.Commands.UpdateCustomer;
using CRM.Application.Features.Customers.DTOs;
using CRM.Application.Features.Customers.Queries.CheckDuplicateCustomer;
using CRM.Application.Features.Customers.Queries.GetAllCustomers;
using CRM.Application.Features.Customers.Queries.GetCustomerById;
using CRM.Application.Features.Loyalty.DTOs;
using CRM.Application.Features.Loyalty.Queries.GetCustomerLoyaltyInfo;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// chỉ yêu cầu đã đăng nhập. Hai nhóm quyền khác nhau (đọc vs ghi)
// "Accountant: xem Customer và Contract (chỉ đọc)") trong khi Sale/Manager được ghi.
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = Policies.CustomerReadAccess)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CustomerDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] ushort? loaiKhachHangId = null,
        [FromQuery] ushort? tinhTrangId = null,
        [FromQuery] bool? isDeleted = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetAllCustomersQuery(pageNumber, pageSize, search, loaiKhachHangId, tinhTrangId, isDeleted),
            cancellationToken);

        return Ok(ApiResponse<PagedResult<CustomerDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = Policies.CustomerReadAccess)]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(ulong id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCustomerByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<CustomerDto>.Ok(result));
    }

    // Tab "Khách hàng thân thiết" ở trang chi tiết KH — điểm/hạng/voucher/lịch sử.
    [HttpGet("{id:long}/loyalty")]
    [Authorize(Policy = Policies.CustomerReadAccess)]
    [ProducesResponseType(typeof(ApiResponse<CustomerLoyaltyInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLoyaltyInfo(ulong id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCustomerLoyaltyInfoQuery(id), cancellationToken);
        return Ok(ApiResponse<CustomerLoyaltyInfoDto>.Ok(result));
    }

    // Cảnh báo trùng lặp Email/SĐT/MST trước khi Lưu — không chặn cứng, chỉ gợi ý xem hồ sơ có sẵn.
    [HttpGet("check-duplicate")]
    [Authorize(Policy = Policies.SalesTeam)]
    [ProducesResponseType(typeof(ApiResponse<CheckDuplicateCustomerResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckDuplicate(
        [FromQuery] string? email, [FromQuery] string? soDienThoai, [FromQuery] string? maSoThue,
        [FromQuery] ulong? excludeId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CheckDuplicateCustomerQuery(email, soDienThoai, maSoThue, excludeId), cancellationToken);
        return Ok(ApiResponse<CheckDuplicateCustomerResult>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = Policies.SalesTeam)]
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
                request.NhanVienPhuTrachId,
                request.NgaySinh,
                request.NgayThanhLap,
                request.HangKhachHangId),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            ApiResponse<CustomerDto>.Ok(result, "Tạo khách hàng thành công."));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = Policies.SalesTeam)]
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
                request.NhanVienPhuTrachId,
                request.NgaySinh,
                request.NgayThanhLap,
                request.HangKhachHangId),
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

    // ManagerOnly — khôi phục khách hàng đã bị khóa/xóa mềm
    [HttpPost("{id:long}/restore")]
    [Authorize(Policy = Policies.ManagerOnly)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Restore(ulong id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RestoreCustomerCommand(id), cancellationToken);
        return Ok(ApiResponse.Ok("Khôi phục khách hàng thành công."));
    }
}

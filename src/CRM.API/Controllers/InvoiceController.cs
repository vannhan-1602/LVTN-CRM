using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.Invoices.Commands.CreateInvoice;
using CRM.Application.Features.Invoices.DTOs;
using CRM.Application.Features.Invoices.Queries.GetAllInvoices;
using CRM.Application.Features.Invoices.Queries.GetInvoiceById;
using CRM.Application.Features.Invoices.Queries.GetTongDaXuatHoaDon;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoiceController : ControllerBase
{
    private readonly IMediator _mediator;
    public InvoiceController(IMediator mediator) => _mediator = mediator;

   
    // Lấy danh sách hóa đơn.
    // Lọc theo: tìm kiếm mã/tên khách, trạng thái thanh toán, khách hàng.
    [HttpGet]
    [Authorize(Policy = Policies.CustomerReadAccess)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? trangThaiThanhToan = null,
        [FromQuery] ulong? khachHangId = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetAllInvoicesQuery(pageNumber, pageSize, search, trangThaiThanhToan, khachHangId), ct);
        return Ok(ApiResponse<PagedResult<InvoiceDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = Policies.CustomerReadAccess)]
    public async Task<IActionResult> GetById(ulong id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetInvoiceByIdQuery(id), ct);
        return Ok(ApiResponse<InvoiceDto>.Ok(result));
    }

    // Tổng tiền đã xuất hóa đơn cho 1 hợp đồng — form tạo hóa đơn dùng để tự tính
    // số tiền còn lại khi hợp đồng thanh toán 1 lần.
    [HttpGet("tong-da-xuat/{hopDongId:long}")]
    [Authorize(Policy = Policies.CustomerReadAccess)]
    public async Task<IActionResult> GetTongDaXuat(ulong hopDongId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTongDaXuatHoaDonQuery(hopDongId), ct);
        return Ok(ApiResponse<decimal>.Ok(result));
    }

    // Tạo hóa đơn mới. Chỉ Kế toán và Manager được thực hiện.
    [HttpPost]
    [Authorize(Policy = Policies.FinanceTeam)]
    public async Task<IActionResult> Create(
        [FromBody] CreateInvoiceRequestDto request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateInvoiceCommand(request.HopDongId, request.KhachHangId, request.LichThanhToanId, request.TongTien), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<InvoiceDto>.Ok(result, "Tạo hóa đơn thành công."));
    }
}
using CRM.Application.Common.Models;
using CRM.Application.Features.Invoices.Commands.CreateInvoice;
using CRM.Application.Features.Invoices.DTOs;
using CRM.Application.Features.Invoices.Queries.GetAllInvoices;
using CRM.Application.Features.Invoices.Queries.GetInvoiceById;
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

    /// <summary>
    /// Lấy danh sách hóa đơn.
    /// Lọc theo: tìm kiếm mã/tên khách, trạng thái thanh toán, khách hàng.
    /// </summary>
    [HttpGet]
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
    public async Task<IActionResult> GetById(ulong id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetInvoiceByIdQuery(id), ct);
        return Ok(ApiResponse<InvoiceDto>.Ok(result));
    }

    /// <summary>
    /// Tạo hóa đơn mới. Chỉ Kế toán và Manager được thực hiện.
    ///
    /// Cách 1 — Có hợp đồng (phổ biến):
    ///   Truyền HopDongId, bỏ qua KhachHangId (hệ thống tự lấy từ hợp đồng).
    ///   Hợp đồng phải đang ở trạng thái "DangThucHien".
    ///
    /// Cách 2 — Không có hợp đồng (bán lẻ):
    ///   Không truyền HopDongId, bắt buộc truyền KhachHangId.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateInvoiceRequestDto request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateInvoiceCommand(request.HopDongId, request.KhachHangId, request.TongTien), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<InvoiceDto>.Ok(result, "Tạo hóa đơn thành công."));
    }
}

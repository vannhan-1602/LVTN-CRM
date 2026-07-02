using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.PhieuThuChi.Commands.CreatePhieuThuChi;
using CRM.Application.Features.PhieuThuChi.DTOs;
using CRM.Application.Features.PhieuThuChi.Queries.GetAllPhieuThuChi;
using CRM.Application.Features.PhieuThuChi.Queries.GetPhieuThuChiById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PhieuThuChiController : ControllerBase
{
    private readonly IMediator _mediator;
    public PhieuThuChiController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Lấy danh sách phiếu thu/chi. Có thể lọc theo:
    /// - KhachHangId: toàn bộ phiếu của một khách hàng
    /// - HoaDonId: toàn bộ phiếu gắn với một hóa đơn cụ thể
    /// - LoaiPhieu: 'Thu' hoặc 'Chi'
    /// </summary>
    [HttpGet]
    [Authorize(Policy = Policies.CustomerReadAccess)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ulong? khachHangId = null,
        [FromQuery] ulong? hoaDonId = null,
        [FromQuery] string? loaiPhieu = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetAllPhieuThuChiQuery(pageNumber, pageSize, khachHangId, hoaDonId, loaiPhieu), ct);
        return Ok(ApiResponse<PagedResult<PhieuThuChiDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = Policies.CustomerReadAccess)]
    public async Task<IActionResult> GetById(ulong id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPhieuThuChiByIdQuery(id), ct);
        return Ok(ApiResponse<PhieuThuChiDto>.Ok(result));
    }

    /// <summary>
    /// Tạo phiếu thu hoặc phiếu chi.
    ///
    /// Phiếu Thu (LoaiPhieu = "Thu"):
    ///   - Bắt buộc truyền HoaDonId.
    ///   - Hệ thống tự kiểm tra số tiền không vượt quá số tiền còn lại của hóa đơn.
    ///   - Hệ thống tự cập nhật SoTienDaThu và TrangThaiThanhToan của hóa đơn.
    ///
    /// Phiếu Chi (LoaiPhieu = "Chi"):
    ///   - Phải truyền KhachHangId hoặc HoaDonId (hoặc cả 2).
    ///   - Không ảnh hưởng đến trạng thái thanh toán của hóa đơn.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = Policies.FinanceTeam)]
    public async Task<IActionResult> Create([FromBody] CreatePhieuThuChiRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreatePhieuThuChiCommand(request.HoaDonId, request.KhachHangId, request.LoaiPhieu, request.SoTien), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<PhieuThuChiDto>.Ok(result, "Tạo phiếu thu/chi thành công."));
    }
}

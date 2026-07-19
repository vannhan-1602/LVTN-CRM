using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.DanhMuc.Commands;
using CRM.Application.Features.DanhMuc.DTOs;
using CRM.Application.Features.DanhMuc.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;


//Quản lý các bảng danh mục — admin có thể tùy chỉnh theo từng công ty.
// PHÂN QUYỀN:
// GET  (xem danh mục): tất cả roles đã đăng nhập.
//  POST/PUT/DELETE (chỉnh sửa danh mục): chỉ Admin.
//  Riêng XepHang: chỉ Update (không cho tạo/xóa tự do vì gắn với logic tích điểm).

[ApiController]
[Route("api/danh-muc")]
[Authorize]
public class DanhMucController : ControllerBase
{
    private readonly IMediator _mediator;
    public DanhMucController(IMediator mediator) => _mediator = mediator;

    // ══════════════════════════════════════════════════════════════════════════
    // LOAI KHACH HANG
    // ══════════════════════════════════════════════════════════════════════════

    [HttpGet("loai-khach-hang")]
    public async Task<IActionResult> GetAllLoaiKhachHang(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllLoaiKhachHangQuery(), ct);
        return Ok(ApiResponse<List<LoaiKhachHangDto>>.Ok(result));
    }

    [HttpPost("loai-khach-hang")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> CreateLoaiKhachHang([FromBody] UpsertLoaiKhachHangDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateLoaiKhachHangCommand(dto), ct);
        return Ok(ApiResponse<LoaiKhachHangDto>.Ok(result, "Tạo loại khách hàng thành công."));
    }

    [HttpPut("loai-khach-hang/{id}")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> UpdateLoaiKhachHang(ushort id, [FromBody] UpsertLoaiKhachHangDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateLoaiKhachHangCommand(id, dto), ct);
        return Ok(ApiResponse<LoaiKhachHangDto>.Ok(result, "Cập nhật thành công."));
    }

    [HttpDelete("loai-khach-hang/{id}")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> DeleteLoaiKhachHang(ushort id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteLoaiKhachHangCommand(id), ct);
        return Ok(ApiResponse<string>.Ok("Đã xóa loại khách hàng."));
    }

    // ══════════════════════════════════════════════════════════════════════════
    // TINH TRANG KHACH HANG
    // ══════════════════════════════════════════════════════════════════════════

    [HttpGet("tinh-trang-khach-hang")]
    public async Task<IActionResult> GetAllTinhTrang(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllTinhTrangQuery(), ct);
        return Ok(ApiResponse<List<TinhTrangKhachHangDto>>.Ok(result));
    }

    [HttpPost("tinh-trang-khach-hang")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> CreateTinhTrang([FromBody] UpsertTinhTrangKhachHangDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateTinhTrangCommand(dto), ct);
        return Ok(ApiResponse<TinhTrangKhachHangDto>.Ok(result, "Tạo tình trạng thành công."));
    }

    [HttpPut("tinh-trang-khach-hang/{id}")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> UpdateTinhTrang(ushort id, [FromBody] UpsertTinhTrangKhachHangDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateTinhTrangCommand(id, dto), ct);
        return Ok(ApiResponse<TinhTrangKhachHangDto>.Ok(result, "Cập nhật thành công."));
    }

    [HttpDelete("tinh-trang-khach-hang/{id}")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> DeleteTinhTrang(ushort id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteTinhTrangCommand(id), ct);
        return Ok(ApiResponse<string>.Ok("Đã xóa tình trạng."));
    }

    // ══════════════════════════════════════════════════════════════════════════
    // LOAI TICKET
    // ══════════════════════════════════════════════════════════════════════════

    [HttpGet("loai-ticket")]
    public async Task<IActionResult> GetAllLoaiTicket(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllLoaiTicketQuery(), ct);
        return Ok(ApiResponse<List<LoaiTicketDto>>.Ok(result));
    }

    [HttpPost("loai-ticket")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> CreateLoaiTicket([FromBody] UpsertLoaiTicketDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateLoaiTicketCommand(dto), ct);
        return Ok(ApiResponse<LoaiTicketDto>.Ok(result, "Tạo loại ticket thành công."));
    }

    [HttpPut("loai-ticket/{id}")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> UpdateLoaiTicket(ushort id, [FromBody] UpsertLoaiTicketDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateLoaiTicketCommand(id, dto), ct);
        return Ok(ApiResponse<LoaiTicketDto>.Ok(result, "Cập nhật thành công."));
    }

    [HttpDelete("loai-ticket/{id}")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> DeleteLoaiTicket(ushort id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteLoaiTicketCommand(id), ct);
        return Ok(ApiResponse<string>.Ok("Đã xóa loại ticket."));
    }

    // ══════════════════════════════════════════════════════════════════════════
    // LOAI SAN PHAM
    // ══════════════════════════════════════════════════════════════════════════

    [HttpGet("loai-san-pham")]
    public async Task<IActionResult> GetAllLoaiSanPham(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllLoaiSanPhamQuery(), ct);
        return Ok(ApiResponse<List<LoaiSanPhamDto>>.Ok(result));
    }

    [HttpPost("loai-san-pham")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> CreateLoaiSanPham([FromBody] UpsertLoaiSanPhamDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateLoaiSanPhamCommand(dto), ct);
        return Ok(ApiResponse<LoaiSanPhamDto>.Ok(result, "Tạo loại sản phẩm thành công."));
    }

    [HttpPut("loai-san-pham/{id}")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> UpdateLoaiSanPham(uint id, [FromBody] UpsertLoaiSanPhamDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateLoaiSanPhamCommand(id, dto), ct);
        return Ok(ApiResponse<LoaiSanPhamDto>.Ok(result, "Cập nhật thành công."));
    }

    [HttpDelete("loai-san-pham/{id}")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> DeleteLoaiSanPham(uint id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteLoaiSanPhamCommand(id), ct);
        return Ok(ApiResponse<string>.Ok("Đã xóa loại sản phẩm."));
    }

    // ══════════════════════════════════════════════════════════════════════════
    // XEP HANG (chỉ GET + PUT — không cho tạo/xóa)
    // ══════════════════════════════════════════════════════════════════════════

    [HttpGet("xep-hang")]
    public async Task<IActionResult> GetAllXepHang(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllXepHangQuery(), ct);
        return Ok(ApiResponse<List<XepHangDto>>.Ok(result));
    }

    /// <summary>
    /// Cập nhật tiêu chí và % giảm voucher của 1 hạng.
    /// Chỉ Admin được chỉnh, mốc điểm phải tăng dần theo thứ tự hạng.
    /// </summary>
    [HttpPut("xep-hang/{id}")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> UpdateXepHang(ushort id, [FromBody] UpdateXepHangDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateXepHangCommand(id, dto), ct);
        return Ok(ApiResponse<XepHangDto>.Ok(result, "Cập nhật hạng thành công."));
    }

    // ══════════════════════════════════════════════════════════════════════════
    // NGAY LE
    // ══════════════════════════════════════════════════════════════════════════

    [HttpGet("ngay-le")]
    public async Task<IActionResult> GetAllNgayLe(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllNgayLeQuery(), ct);
        return Ok(ApiResponse<List<NgayLeDto>>.Ok(result));
    }

    [HttpPost("ngay-le")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> CreateNgayLe([FromBody] UpsertNgayLeDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateNgayLeCommand(dto), ct);
        return Ok(ApiResponse<NgayLeDto>.Ok(result, "Thêm ngày lễ thành công."));
    }

    [HttpPut("ngay-le/{id}")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> UpdateNgayLe(ushort id, [FromBody] UpsertNgayLeDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateNgayLeCommand(id, dto), ct);
        return Ok(ApiResponse<NgayLeDto>.Ok(result, "Cập nhật thành công."));
    }

    [HttpDelete("ngay-le/{id}")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> DeleteNgayLe(ushort id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteNgayLeCommand(id), ct);
        return Ok(ApiResponse<string>.Ok("Đã xóa ngày lễ."));
    }
}

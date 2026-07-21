using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Interfaces.Common;
using CRM.Application.Features.Tickets.Commands.AddPhanHoi;
using CRM.Application.Features.Tickets.Commands.AssignTicket;
using CRM.Application.Features.Tickets.Commands.CloseTicket;
using CRM.Application.Features.Tickets.Commands.CreateTicket;
using CRM.Application.Features.Tickets.Commands.DeleteTicket;
using CRM.Application.Features.Tickets.Commands.UpdateTicket;
using CRM.Application.Features.Tickets.DTOs;
using CRM.Application.Features.Tickets.Queries.GetAllTickets;
using CRM.Application.Features.Tickets.Queries.GetTicketById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // SalesTeam (Sale + Manager)
    
    [Authorize(Policy = Policies.SalesTeam)]
    public class TicketController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUser;
        public TicketController(IMediator mediator, ICurrentUserService currentUser)
        {
            _mediator = mediator;
            _currentUser = currentUser;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? trangThai = null,
            [FromQuery] string? mucDoUuTien = null,
            [FromQuery] ulong? khachHangId = null,
            [FromQuery] uint? nhanVienXuLyId = null,
            CancellationToken ct = default)
        {
            var result = await _mediator.Send(new GetAllTicketsQuery(
                pageNumber, pageSize, search, trangThai, mucDoUuTien, khachHangId, nhanVienXuLyId), ct);
            return Ok(ApiResponse<PagedResult<TicketDto>>.Ok(result));
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(ulong id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetTicketByIdQuery(id), ct);
            return Ok(ApiResponse<TicketDetailDto>.Ok(result));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTicketRequestDto request, CancellationToken ct)
        {
            var result = await _mediator.Send(new CreateTicketCommand(
                request.TieuDe, request.MoTa, request.FileDinhKem, request.LoaiTicketId,
                request.KhachHangId, request.HopDongId, request.SanPhamId,
                request.MucDoUuTien, request.NguonTiepNhan,
                request.NhanVienTiepNhanId, request.NhanVienXuLyId, request.NgayHenXuLy), ct);

            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ApiResponse<TicketDto>.Ok(result, "Tạo ticket thành công."));
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(ulong id, [FromBody] UpdateTicketRequestDto request, CancellationToken ct)
        {
            var result = await _mediator.Send(new UpdateTicketCommand(
                id, request.TieuDe, request.MoTa, request.FileDinhKem, request.LoaiTicketId,
                request.HopDongId, request.SanPhamId, request.MucDoUuTien,
                request.NguonTiepNhan, request.TrangThai, request.NgayHenXuLy), ct);

            return Ok(ApiResponse<TicketDto>.Ok(result, "Cập nhật ticket thành công."));
        }

        // ManagerOnly
        [HttpDelete("{id:long}")]
        [Authorize(Policy = Policies.ManagerOnly)]
        public async Task<IActionResult> Delete(ulong id, CancellationToken ct)
        {
            await _mediator.Send(new DeleteTicketCommand(id), ct);
            return Ok(ApiResponse.Ok("Xóa ticket thành công."));
        }

        [HttpPost("{id:long}/assign")]
        public async Task<IActionResult> Assign(ulong id, [FromBody] AssignTicketRequestDto request, CancellationToken ct)
        {
            var result = await _mediator.Send(new AssignTicketCommand(id, request.NhanVienXuLyId, request.NgayHenXuLy), ct);
            return Ok(ApiResponse<TicketDto>.Ok(result, "Gán xử lý ticket thành công."));
        }

        [HttpPost("{id:long}/close")]
        public async Task<IActionResult> Close(ulong id, [FromBody] CloseTicketRequestDto request, CancellationToken ct)
        {
            var result = await _mediator.Send(new CloseTicketCommand(id, _currentUser.UserId, request.LyDoDong), ct);
            return Ok(ApiResponse<TicketDto>.Ok(result, "Đóng ticket thành công."));
        }

        [HttpGet("{id:long}/phanhoi")]
        public async Task<IActionResult> GetPhanHois(ulong id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetTicketByIdQuery(id), ct);
            return Ok(ApiResponse<List<TicketPhanHoiDto>>.Ok(result.PhanHois));
        }

        [HttpPost("{id:long}/phanhoi")]
        public async Task<IActionResult> AddPhanHoi(ulong id, [FromBody] AddPhanHoiRequestDto request, CancellationToken ct)
        {
            var result = await _mediator.Send(new AddPhanHoiCommand(
                id, _currentUser.UserId, request.LoaiPhanHoi, request.NoiDung, request.FileDinhKem), ct);

            return Ok(ApiResponse<TicketPhanHoiDto>.Ok(result, "Thêm phản hồi thành công."));
        }
    }
}

using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.Contracts.Commands.CreateContractFromQuote;
using CRM.Application.Features.Contracts.Commands.CreateLicense;
using CRM.Application.Features.Contracts.Commands.CreateMilestone;
using CRM.Application.Features.Contracts.Commands.CreateRenewalContract;
using CRM.Application.Features.Contracts.Commands.DeleteContract;
using CRM.Application.Features.Contracts.Commands.DeleteMilestone;
using CRM.Application.Features.Contracts.Commands.RenewLicense;
using CRM.Application.Features.Contracts.Commands.SendContractEmail;
using CRM.Application.Features.Contracts.Commands.ToggleLicenseLock;
using CRM.Application.Features.Contracts.Commands.UpdateContractStatus;
using CRM.Application.Features.Contracts.Commands.UpdateMilestone;
using CRM.Application.Features.Contracts.DTOs;
using CRM.Application.Features.Contracts.Queries.GetAllContracts;
using CRM.Application.Features.Contracts.Queries.GetContractById;
using CRM.Application.Features.Contracts.Queries.GetContractEmailHistory;
using CRM.Application.Features.Contracts.Queries.GetLichThanhToanByHopDong;
using CRM.Application.Features.Contracts.Queries.GetLicensesByContract;
using CRM.Application.Features.Contracts.Queries.GetMilestonesByContract;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContractController : ControllerBase
{
    private readonly IMediator _mediator;
    public ContractController(IMediator mediator) => _mediator = mediator;

    //Accountant xem Customer và Contract chỉ đọc — dùng chung policy CustomerReadAccess đã có sẵn.
    [HttpGet]
    [Authorize(Policy = Policies.CustomerReadAccess)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? trangThai = null,
        [FromQuery] ulong? khachHangId = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAllContractsQuery(pageNumber, pageSize, search, trangThai, khachHangId), ct);
        return Ok(ApiResponse<PagedResult<ContractDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = Policies.CustomerReadAccess)]
    public async Task<IActionResult> GetById(ulong id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetContractByIdQuery(id), ct);
        return Ok(ApiResponse<ContractDto>.Ok(result));
    }

    // Ghi: chỉ Sale + Manager (Accountant không tạo/sửa hợp đồng, chỉ xem)
    [HttpPost("from-quote")]
    [Authorize(Policy = Policies.SalesTeam)]
    public async Task<IActionResult> CreateFromQuote([FromBody] CreateContractFromQuoteRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateContractFromQuoteCommand(
                request.BaoGiaId, request.NgayKy, request.ThoiHan,
                request.HinhThucThanhToan, request.LichThanhToans), ct);

        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<ContractDto>.Ok(result, "Tạo hợp đồng thành công."));
    }

    // Ghi: chỉ Sale + Manager — tạo hợp đồng gia hạn từ 1 hợp đồng đã có.
    [HttpPost("{id:long}/renew")]
    [Authorize(Policy = Policies.SalesTeam)]
    public async Task<IActionResult> Renew(ulong id, [FromBody] CreateRenewalContractRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateRenewalContractCommand(id, request.NgayKy, request.LichThanhToans), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<ContractDto>.Ok(result, "Tạo hợp đồng gia hạn thành công."));
    }

    [HttpGet("{id:long}/lich-thanh-toan")]
    [Authorize(Policy = Policies.CustomerReadAccess)]
    public async Task<IActionResult> GetLichThanhToan(ulong id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLichThanhToanByHopDongQuery(id), ct);
        return Ok(ApiResponse<List<Application.Features.Contracts.DTOs.LichThanhToanDto>>.Ok(result));
    }

    // Gửi hợp đồng (kèm file PDF) cho khách hàng qua email — chỉ Sale + Manager (người
    // có quyền tạo/sửa hợp đồng) mới được gửi, giống quyền tạo hợp đồng từ báo giá.
    [HttpPost("{id:long}/send-email")]
    [Authorize(Policy = Policies.SalesTeam)]
    public async Task<IActionResult> SendEmail(
        ulong id, [FromBody] SendContractEmailRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new SendContractEmailCommand(id, request), ct);
        return Ok(ApiResponse<SendContractEmailResultDto>.Ok(result, $"Đã gửi hợp đồng tới {result.EmailDaGui}."));
    }

    [HttpGet("{id:long}/email-history")]
    [Authorize(Policy = Policies.CustomerReadAccess)]
    public async Task<IActionResult> GetEmailHistory(ulong id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetContractEmailHistoryQuery(id), ct);
        return Ok(ApiResponse<List<ContractEmailHistoryItemDto>>.Ok(result));
    }

    [HttpPut("{id:long}/status")]
    [Authorize(Policy = Policies.SalesTeam)]
    public async Task<IActionResult> UpdateStatus(ulong id, [FromBody] UpdateContractStatusRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateContractStatusCommand(id, request.TrangThai), ct);
        return Ok(ApiResponse<ContractDto>.Ok(result, "Cập nhật trạng thái hợp đồng thành công."));
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = Policies.ManagerOnly)]
    public async Task<IActionResult> Delete(ulong id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteContractCommand(id), ct);
        return Ok(ApiResponse.Ok("Xóa hợp đồng thành công."));
    }

    // ── Mốc triển khai (Đào tạo / Bàn giao / Nghiệm thu) ─────────────────────

    [HttpGet("{id:long}/moc-trien-khai")]
    [Authorize(Policy = Policies.CustomerReadAccess)]
    public async Task<IActionResult> GetMocTrienKhai(ulong id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMilestonesByContractQuery(id), ct);
        return Ok(ApiResponse<List<MocTrienKhaiDto>>.Ok(result));
    }

    // Chỉ Manager tạo mốc triển khai (bao gồm gán nhân viên phụ trách) — Sale không được tạo.
    [HttpPost("{id:long}/moc-trien-khai")]
    [Authorize(Policy = Policies.ManagerOnly)]
    public async Task<IActionResult> CreateMocTrienKhai(ulong id, [FromBody] CreateMocTrienKhaiRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateMilestoneCommand(
            id, request.LoaiMoc, request.NoiDung, request.NgayThucHien, request.NhanVienThucHienId), ct);
        return Ok(ApiResponse<MocTrienKhaiDto>.Ok(result, "Tạo mốc triển khai thành công."));
    }

    [HttpPut("moc-trien-khai/{mocId:long}")]
    [Authorize(Policy = Policies.SalesTeam)]
    public async Task<IActionResult> UpdateMocTrienKhai(ulong mocId, [FromBody] UpdateMocTrienKhaiRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateMilestoneCommand(
            mocId, request.NoiDung, request.NgayThucHien, request.NhanVienThucHienId,
            request.NguoiXacNhanKhach, request.FileBienBan, request.TrangThai), ct);
        return Ok(ApiResponse<MocTrienKhaiDto>.Ok(result, "Cập nhật mốc triển khai thành công."));
    }

    // Chỉ Manager xóa mốc triển khai — Sale không được xóa.
    [HttpDelete("moc-trien-khai/{mocId:long}")]
    [Authorize(Policy = Policies.ManagerOnly)]
    public async Task<IActionResult> DeleteMocTrienKhai(ulong mocId, CancellationToken ct)
    {
        await _mediator.Send(new DeleteMilestoneCommand(mocId), ct);
        return Ok(ApiResponse.Ok("Xóa mốc triển khai thành công."));
    }

    // ── License phần mềm ──────────────────────────────────────────────────────
    // Ràng buộc nghiệp vụ: chỉ cấp/gia hạn/khóa bởi Manager (giống mốc triển khai) — Sale chỉ xem.
    // Xem README ở CreateLicenseCommand/RenewLicenseCommand để biết chi tiết ràng buộc.

    [HttpGet("{id:long}/license")]
    [Authorize(Policy = Policies.CustomerReadAccess)]
    public async Task<IActionResult> GetLicenses(ulong id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLicensesByContractQuery(id), ct);
        return Ok(ApiResponse<List<LicenseDto>>.Ok(result));
    }

    [HttpPost("{id:long}/license")]
    [Authorize(Policy = Policies.ManagerOnly)]
    public async Task<IActionResult> CreateLicense(ulong id, [FromBody] CreateLicenseRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateLicenseCommand(
            id, request.SanPhamId, request.SoLuongUser, request.PhienBan, request.MoiTruongTrienKhai), ct);
        return Ok(ApiResponse<LicenseDto>.Ok(result, "Cấp License thành công."));
    }

    [HttpPost("license/{licenseId:long}/renew")]
    [Authorize(Policy = Policies.ManagerOnly)]
    public async Task<IActionResult> RenewLicense(ulong licenseId, [FromBody] RenewLicenseRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RenewLicenseCommand(licenseId, request.HopDongGiaHanId), ct);
        return Ok(ApiResponse<LicenseDto>.Ok(result, "Gia hạn License thành công."));
    }

    [HttpPut("license/{licenseId:long}/lock")]
    [Authorize(Policy = Policies.ManagerOnly)]
    public async Task<IActionResult> ToggleLicenseLock(ulong licenseId, [FromBody] ToggleLicenseLockRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ToggleLicenseLockCommand(licenseId, request.Khoa), ct);
        var msg = request.Khoa ? "Đã khóa License." : "Đã mở khóa License.";
        return Ok(ApiResponse<LicenseDto>.Ok(result, msg));
    }
}
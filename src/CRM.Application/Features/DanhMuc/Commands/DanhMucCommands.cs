using CRM.Application.Common.Exceptions;
using CRM.Application.Features.DanhMuc.DTOs;
using CRM.Application.Interfaces.DanhMuc;
using CRM.Domain.Enums;
using FluentValidation;
using MediatR;

namespace CRM.Application.Features.DanhMuc.Commands;

// ════════════════════════════════════════════════════════════════════════════
// LOAI KHACH HANG
// ════════════════════════════════════════════════════════════════════════════
public record CreateLoaiKhachHangCommand(UpsertLoaiKhachHangDto Dto) : IRequest<LoaiKhachHangDto>;
public record UpdateLoaiKhachHangCommand(ushort Id, UpsertLoaiKhachHangDto Dto) : IRequest<LoaiKhachHangDto>;
public record DeleteLoaiKhachHangCommand(ushort Id) : IRequest;

// Validator PHẢI target đúng Command type — ValidationBehavior<TRequest,TResponse> chỉ
// resolve IValidator<TRequest> (TRequest = Command), không resolve IValidator<Dto lồng bên
// trong>. Validator target DTO sẽ được đăng ký vào DI nhưng KHÔNG BAO GIỜ được pipeline gọi.
public class CreateLoaiKhachHangCommandValidator : AbstractValidator<CreateLoaiKhachHangCommand>
{
    public CreateLoaiKhachHangCommandValidator()
    {
        RuleFor(x => x.Dto.TenLoai).NotEmpty().MaximumLength(50)
            .WithMessage("Tên loại khách hàng không được để trống và tối đa 50 ký tự.");
    }
}
public class UpdateLoaiKhachHangCommandValidator : AbstractValidator<UpdateLoaiKhachHangCommand>
{
    public UpdateLoaiKhachHangCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan((ushort)0);
        RuleFor(x => x.Dto.TenLoai).NotEmpty().MaximumLength(50)
            .WithMessage("Tên loại khách hàng không được để trống và tối đa 50 ký tự.");
    }
}

public class CreateLoaiKhachHangHandler : IRequestHandler<CreateLoaiKhachHangCommand, LoaiKhachHangDto>
{
    private readonly IDanhMucRepository _repo;
    public CreateLoaiKhachHangHandler(IDanhMucRepository repo) => _repo = repo;
    public Task<LoaiKhachHangDto> Handle(CreateLoaiKhachHangCommand r, CancellationToken ct) =>
        _repo.CreateLoaiKhachHangAsync(r.Dto, ct);
}
public class UpdateLoaiKhachHangHandler : IRequestHandler<UpdateLoaiKhachHangCommand, LoaiKhachHangDto>
{
    private readonly IDanhMucRepository _repo;
    public UpdateLoaiKhachHangHandler(IDanhMucRepository repo) => _repo = repo;
    public Task<LoaiKhachHangDto> Handle(UpdateLoaiKhachHangCommand r, CancellationToken ct) =>
        _repo.UpdateLoaiKhachHangAsync(r.Id, r.Dto, ct);
}
public class DeleteLoaiKhachHangHandler : IRequestHandler<DeleteLoaiKhachHangCommand>
{
    private readonly IDanhMucRepository _repo;
    public DeleteLoaiKhachHangHandler(IDanhMucRepository repo) => _repo = repo;
    public Task Handle(DeleteLoaiKhachHangCommand r, CancellationToken ct) =>
        _repo.DeleteLoaiKhachHangAsync(r.Id, ct);
}

// ════════════════════════════════════════════════════════════════════════════
// TINH TRANG KHACH HANG
// ════════════════════════════════════════════════════════════════════════════
public record CreateTinhTrangCommand(UpsertTinhTrangKhachHangDto Dto) : IRequest<TinhTrangKhachHangDto>;
public record UpdateTinhTrangCommand(ushort Id, UpsertTinhTrangKhachHangDto Dto) : IRequest<TinhTrangKhachHangDto>;
public record DeleteTinhTrangCommand(ushort Id) : IRequest;

public class CreateTinhTrangCommandValidator : AbstractValidator<CreateTinhTrangCommand>
{
    public CreateTinhTrangCommandValidator()
    {
        RuleFor(x => x.Dto.TenTinhTrang).NotEmpty().MaximumLength(50)
            .WithMessage("Tên tình trạng không được để trống và tối đa 50 ký tự.");
    }
}
public class UpdateTinhTrangCommandValidator : AbstractValidator<UpdateTinhTrangCommand>
{
    public UpdateTinhTrangCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan((ushort)0);
        RuleFor(x => x.Dto.TenTinhTrang).NotEmpty().MaximumLength(50)
            .WithMessage("Tên tình trạng không được để trống và tối đa 50 ký tự.");
    }
}

public class CreateTinhTrangHandler : IRequestHandler<CreateTinhTrangCommand, TinhTrangKhachHangDto>
{
    private readonly IDanhMucRepository _repo;
    public CreateTinhTrangHandler(IDanhMucRepository repo) => _repo = repo;
    public Task<TinhTrangKhachHangDto> Handle(CreateTinhTrangCommand r, CancellationToken ct) =>
        _repo.CreateTinhTrangAsync(r.Dto, ct);
}
public class UpdateTinhTrangHandler : IRequestHandler<UpdateTinhTrangCommand, TinhTrangKhachHangDto>
{
    private readonly IDanhMucRepository _repo;
    public UpdateTinhTrangHandler(IDanhMucRepository repo) => _repo = repo;
    public Task<TinhTrangKhachHangDto> Handle(UpdateTinhTrangCommand r, CancellationToken ct) =>
        _repo.UpdateTinhTrangAsync(r.Id, r.Dto, ct);
}
public class DeleteTinhTrangHandler : IRequestHandler<DeleteTinhTrangCommand>
{
    private readonly IDanhMucRepository _repo;
    public DeleteTinhTrangHandler(IDanhMucRepository repo) => _repo = repo;
    public Task Handle(DeleteTinhTrangCommand r, CancellationToken ct) =>
        _repo.DeleteTinhTrangAsync(r.Id, ct);
}

// ════════════════════════════════════════════════════════════════════════════
// LOAI TICKET
// ════════════════════════════════════════════════════════════════════════════
public record CreateLoaiTicketCommand(UpsertLoaiTicketDto Dto) : IRequest<LoaiTicketDto>;
public record UpdateLoaiTicketCommand(ushort Id, UpsertLoaiTicketDto Dto) : IRequest<LoaiTicketDto>;
public record DeleteLoaiTicketCommand(ushort Id) : IRequest;

public class CreateLoaiTicketCommandValidator : AbstractValidator<CreateLoaiTicketCommand>
{
    public CreateLoaiTicketCommandValidator()
    {
        RuleFor(x => x.Dto.TenLoai).NotEmpty().MaximumLength(100)
            .WithMessage("Tên loại ticket không được để trống và tối đa 100 ký tự.");
    }
}
public class UpdateLoaiTicketCommandValidator : AbstractValidator<UpdateLoaiTicketCommand>
{
    public UpdateLoaiTicketCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan((ushort)0);
        RuleFor(x => x.Dto.TenLoai).NotEmpty().MaximumLength(100)
            .WithMessage("Tên loại ticket không được để trống và tối đa 100 ký tự.");
    }
}

public class CreateLoaiTicketHandler : IRequestHandler<CreateLoaiTicketCommand, LoaiTicketDto>
{
    private readonly IDanhMucRepository _repo;
    public CreateLoaiTicketHandler(IDanhMucRepository repo) => _repo = repo;
    public Task<LoaiTicketDto> Handle(CreateLoaiTicketCommand r, CancellationToken ct) =>
        _repo.CreateLoaiTicketAsync(r.Dto, ct);
}
public class UpdateLoaiTicketHandler : IRequestHandler<UpdateLoaiTicketCommand, LoaiTicketDto>
{
    private readonly IDanhMucRepository _repo;
    public UpdateLoaiTicketHandler(IDanhMucRepository repo) => _repo = repo;
    public Task<LoaiTicketDto> Handle(UpdateLoaiTicketCommand r, CancellationToken ct) =>
        _repo.UpdateLoaiTicketAsync(r.Id, r.Dto, ct);
}
public class DeleteLoaiTicketHandler : IRequestHandler<DeleteLoaiTicketCommand>
{
    private readonly IDanhMucRepository _repo;
    public DeleteLoaiTicketHandler(IDanhMucRepository repo) => _repo = repo;
    public Task Handle(DeleteLoaiTicketCommand r, CancellationToken ct) =>
        _repo.DeleteLoaiTicketAsync(r.Id, ct);
}

// ════════════════════════════════════════════════════════════════════════════
// LOAI SAN PHAM
// ════════════════════════════════════════════════════════════════════════════
public record CreateLoaiSanPhamCommand(UpsertLoaiSanPhamDto Dto) : IRequest<LoaiSanPhamDto>;
public record UpdateLoaiSanPhamCommand(uint Id, UpsertLoaiSanPhamDto Dto) : IRequest<LoaiSanPhamDto>;
public record DeleteLoaiSanPhamCommand(uint Id) : IRequest;

public class CreateLoaiSanPhamCommandValidator : AbstractValidator<CreateLoaiSanPhamCommand>
{
    public CreateLoaiSanPhamCommandValidator()
    {
        RuleFor(x => x.Dto.TenLoai).NotEmpty().MaximumLength(100)
            .WithMessage("Tên loại sản phẩm không được để trống và tối đa 100 ký tự.");
        RuleFor(x => x.Dto.HinhThuc).Must(v => ProductFormType.All.Contains(v))
            .WithMessage($"Hình thức phải là một trong: {string.Join(", ", ProductFormType.All)}.");
    }
}
public class UpdateLoaiSanPhamCommandValidator : AbstractValidator<UpdateLoaiSanPhamCommand>
{
    public UpdateLoaiSanPhamCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0U);
        RuleFor(x => x.Dto.TenLoai).NotEmpty().MaximumLength(100)
            .WithMessage("Tên loại sản phẩm không được để trống và tối đa 100 ký tự.");
        RuleFor(x => x.Dto.HinhThuc).Must(v => ProductFormType.All.Contains(v))
            .WithMessage($"Hình thức phải là một trong: {string.Join(", ", ProductFormType.All)}.");
    }
}

public class CreateLoaiSanPhamHandler : IRequestHandler<CreateLoaiSanPhamCommand, LoaiSanPhamDto>
{
    private readonly IDanhMucRepository _repo;
    public CreateLoaiSanPhamHandler(IDanhMucRepository repo) => _repo = repo;
    public Task<LoaiSanPhamDto> Handle(CreateLoaiSanPhamCommand r, CancellationToken ct) =>
        _repo.CreateLoaiSanPhamAsync(r.Dto, ct);
}
public class UpdateLoaiSanPhamHandler : IRequestHandler<UpdateLoaiSanPhamCommand, LoaiSanPhamDto>
{
    private readonly IDanhMucRepository _repo;
    public UpdateLoaiSanPhamHandler(IDanhMucRepository repo) => _repo = repo;
    public Task<LoaiSanPhamDto> Handle(UpdateLoaiSanPhamCommand r, CancellationToken ct) =>
        _repo.UpdateLoaiSanPhamAsync(r.Id, r.Dto, ct);
}
public class DeleteLoaiSanPhamHandler : IRequestHandler<DeleteLoaiSanPhamCommand>
{
    private readonly IDanhMucRepository _repo;
    public DeleteLoaiSanPhamHandler(IDanhMucRepository repo) => _repo = repo;
    public Task Handle(DeleteLoaiSanPhamCommand r, CancellationToken ct) =>
        _repo.DeleteLoaiSanPhamAsync(r.Id, ct);
}

// ════════════════════════════════════════════════════════════════════════════
// XEP HANG (chỉ Update, không tạo/xóa)
// ════════════════════════════════════════════════════════════════════════════
public record UpdateXepHangCommand(ushort Id, UpdateXepHangDto Dto) : IRequest<XepHangDto>;

public class UpdateXepHangCommandValidator : AbstractValidator<UpdateXepHangCommand>
{
    public UpdateXepHangCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan((ushort)0);
        RuleFor(x => x.Dto.DiemToiThieu).GreaterThanOrEqualTo(0)
            .WithMessage("Mốc điểm không được âm.");
        RuleFor(x => x.Dto.SoLanThuToiThieu).GreaterThanOrEqualTo(0)
            .WithMessage("Số lần thu không được âm.");
        RuleFor(x => x.Dto.PhanTramGiamVoucher).InclusiveBetween(0, 100)
            .WithMessage("Phần trăm giảm voucher phải từ 0 đến 100.");
    }
}

public class UpdateXepHangHandler : IRequestHandler<UpdateXepHangCommand, XepHangDto>
{
    private readonly IDanhMucRepository _repo;
    public UpdateXepHangHandler(IDanhMucRepository repo) => _repo = repo;
    public Task<XepHangDto> Handle(UpdateXepHangCommand r, CancellationToken ct) =>
        _repo.UpdateXepHangAsync(r.Id, r.Dto, ct);
}

// ════════════════════════════════════════════════════════════════════════════
// NGAY LE
// ════════════════════════════════════════════════════════════════════════════
public record CreateNgayLeCommand(UpsertNgayLeDto Dto) : IRequest<NgayLeDto>;
public record UpdateNgayLeCommand(ushort Id, UpsertNgayLeDto Dto) : IRequest<NgayLeDto>;
public record DeleteNgayLeCommand(ushort Id) : IRequest;

public class CreateNgayLeCommandValidator : AbstractValidator<CreateNgayLeCommand>
{
    public CreateNgayLeCommandValidator()
    {
        RuleFor(x => x.Dto.TenNgayLe).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Dto.Thang).InclusiveBetween((byte)1, (byte)12)
            .WithMessage("Tháng phải từ 1 đến 12.");
        RuleFor(x => x.Dto.Ngay).InclusiveBetween((byte)1, (byte)31)
            .WithMessage("Ngày phải từ 1 đến 31.");
        // InclusiveBetween(1,31) không đủ — Ngay=31 với Thang thuộc nhóm 30 ngày (4/6/9/11) hoặc
        // Ngay=30/31 với Thang=2 là ngày KHÔNG TỒN TẠI trong bất kỳ năm nào (khác 29/2 vẫn tồn
        // tại ở năm nhuận). LoyaltyService.ChayJobHangNgayAsync có try/catch quanh new DateTime()
        // nên không crash — nhưng hậu quả là ngày lễ đó ÂM THẦM KHÔNG BAO GIỜ được gửi mà Admin
        // không biết, vì validator không cảnh báo lúc nhập liệu.
        RuleFor(x => x.Dto)
            .Must(d => d.Thang is not (4 or 6 or 9 or 11) || d.Ngay <= 30)
            .WithMessage("Tháng 4, 6, 9, 11 chỉ có tối đa 30 ngày.")
            .Must(d => d.Thang != 2 || d.Ngay <= 29)
            .WithMessage("Tháng 2 chỉ có tối đa 29 ngày (kể cả năm nhuận).");
        RuleFor(x => x.Dto.SoNgayGuiTruoc).InclusiveBetween((byte)1, (byte)30)
            .WithMessage("Số ngày gửi trước phải từ 1 đến 30.");
        RuleFor(x => x.Dto.ApDungChoLoaiKH)
            .Must(v => new[] { "B2C", "B2B", "TatCa" }.Contains(v))
            .WithMessage("ApDungChoLoaiKH chỉ nhận: B2C, B2B, TatCa.");
    }
}
public class UpdateNgayLeCommandValidator : AbstractValidator<UpdateNgayLeCommand>
{
    public UpdateNgayLeCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan((ushort)0);
        RuleFor(x => x.Dto.TenNgayLe).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Dto.Thang).InclusiveBetween((byte)1, (byte)12)
            .WithMessage("Tháng phải từ 1 đến 12.");
        RuleFor(x => x.Dto.Ngay).InclusiveBetween((byte)1, (byte)31)
            .WithMessage("Ngày phải từ 1 đến 31.");
        RuleFor(x => x.Dto)
            .Must(d => d.Thang is not (4 or 6 or 9 or 11) || d.Ngay <= 30)
            .WithMessage("Tháng 4, 6, 9, 11 chỉ có tối đa 30 ngày.")
            .Must(d => d.Thang != 2 || d.Ngay <= 29)
            .WithMessage("Tháng 2 chỉ có tối đa 29 ngày (kể cả năm nhuận).");
        RuleFor(x => x.Dto.SoNgayGuiTruoc).InclusiveBetween((byte)1, (byte)30)
            .WithMessage("Số ngày gửi trước phải từ 1 đến 30.");
        RuleFor(x => x.Dto.ApDungChoLoaiKH)
            .Must(v => new[] { "B2C", "B2B", "TatCa" }.Contains(v))
            .WithMessage("ApDungChoLoaiKH chỉ nhận: B2C, B2B, TatCa.");
    }
}

public class CreateNgayLeHandler : IRequestHandler<CreateNgayLeCommand, NgayLeDto>
{
    private readonly IDanhMucRepository _repo;
    public CreateNgayLeHandler(IDanhMucRepository repo) => _repo = repo;
    public Task<NgayLeDto> Handle(CreateNgayLeCommand r, CancellationToken ct) =>
        _repo.CreateNgayLeAsync(r.Dto, ct);
}
public class UpdateNgayLeHandler : IRequestHandler<UpdateNgayLeCommand, NgayLeDto>
{
    private readonly IDanhMucRepository _repo;
    public UpdateNgayLeHandler(IDanhMucRepository repo) => _repo = repo;
    public Task<NgayLeDto> Handle(UpdateNgayLeCommand r, CancellationToken ct) =>
        _repo.UpdateNgayLeAsync(r.Id, r.Dto, ct);
}
public class DeleteNgayLeHandler : IRequestHandler<DeleteNgayLeCommand>
{
    private readonly IDanhMucRepository _repo;
    public DeleteNgayLeHandler(IDanhMucRepository repo) => _repo = repo;
    public Task Handle(DeleteNgayLeCommand r, CancellationToken ct) =>
        _repo.DeleteNgayLeAsync(r.Id, ct);
}
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.DanhMuc.DTOs;
using CRM.Application.Interfaces.DanhMuc;
using MediatR;

namespace CRM.Application.Features.DanhMuc.Queries;

// ── LoaiKhachHang ─────────────────────────────────────────────────────────────
public record GetAllLoaiKhachHangQuery : IRequest<List<LoaiKhachHangDto>>;
public class GetAllLoaiKhachHangHandler : IRequestHandler<GetAllLoaiKhachHangQuery, List<LoaiKhachHangDto>>
{
    private readonly IDanhMucRepository _repo;
    public GetAllLoaiKhachHangHandler(IDanhMucRepository repo) => _repo = repo;
    public Task<List<LoaiKhachHangDto>> Handle(GetAllLoaiKhachHangQuery _, CancellationToken ct) =>
        _repo.GetAllLoaiKhachHangAsync(ct);
}

// ── TinhTrangKhachHang ────────────────────────────────────────────────────────
public record GetAllTinhTrangQuery : IRequest<List<TinhTrangKhachHangDto>>;
public class GetAllTinhTrangHandler : IRequestHandler<GetAllTinhTrangQuery, List<TinhTrangKhachHangDto>>
{
    private readonly IDanhMucRepository _repo;
    public GetAllTinhTrangHandler(IDanhMucRepository repo) => _repo = repo;
    public Task<List<TinhTrangKhachHangDto>> Handle(GetAllTinhTrangQuery _, CancellationToken ct) =>
        _repo.GetAllTinhTrangAsync(ct);
}

// ── LoaiTicket ────────────────────────────────────────────────────────────────
public record GetAllLoaiTicketQuery : IRequest<List<LoaiTicketDto>>;
public class GetAllLoaiTicketHandler : IRequestHandler<GetAllLoaiTicketQuery, List<LoaiTicketDto>>
{
    private readonly IDanhMucRepository _repo;
    public GetAllLoaiTicketHandler(IDanhMucRepository repo) => _repo = repo;
    public Task<List<LoaiTicketDto>> Handle(GetAllLoaiTicketQuery _, CancellationToken ct) =>
        _repo.GetAllLoaiTicketAsync(ct);
}

// ── LoaiSanPham ───────────────────────────────────────────────────────────────
public record GetAllLoaiSanPhamQuery : IRequest<List<LoaiSanPhamDto>>;
public class GetAllLoaiSanPhamHandler : IRequestHandler<GetAllLoaiSanPhamQuery, List<LoaiSanPhamDto>>
{
    private readonly IDanhMucRepository _repo;
    public GetAllLoaiSanPhamHandler(IDanhMucRepository repo) => _repo = repo;
    public Task<List<LoaiSanPhamDto>> Handle(GetAllLoaiSanPhamQuery _, CancellationToken ct) =>
        _repo.GetAllLoaiSanPhamAsync(ct);
}

// ── XepHang ───────────────────────────────────────────────────────────────────
public record GetAllXepHangQuery : IRequest<List<XepHangDto>>;
public class GetAllXepHangHandler : IRequestHandler<GetAllXepHangQuery, List<XepHangDto>>
{
    private readonly IDanhMucRepository _repo;
    public GetAllXepHangHandler(IDanhMucRepository repo) => _repo = repo;
    public Task<List<XepHangDto>> Handle(GetAllXepHangQuery _, CancellationToken ct) =>
        _repo.GetAllXepHangAsync(ct);
}

// ── NgayLe ────────────────────────────────────────────────────────────────────
public record GetAllNgayLeQuery : IRequest<List<NgayLeDto>>;
public class GetAllNgayLeHandler : IRequestHandler<GetAllNgayLeQuery, List<NgayLeDto>>
{
    private readonly IDanhMucRepository _repo;
    public GetAllNgayLeHandler(IDanhMucRepository repo) => _repo = repo;
    public Task<List<NgayLeDto>> Handle(GetAllNgayLeQuery _, CancellationToken ct) =>
        _repo.GetAllNgayLeAsync(ct);
}

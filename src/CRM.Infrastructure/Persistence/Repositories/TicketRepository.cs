using CRM.Application.Common.Models;
using CRM.Application.Interfaces.Tickets;
using CRM.Domain.Entities.Tickets;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence.Contexts;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly CrmDbContext _context;
        public TicketRepository(CrmDbContext context) => _context = context;

        public async Task<Ticket?> GetByIdAsync(ulong id, CancellationToken ct = default)
        {
            var e = await _context.Set<TkTicketEntity>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
            return e is null ? null : MapToDomain(e);
        }

        public async Task<PagedResult<Ticket>> GetPagedAsync(
            int pageNumber, int pageSize, string? search, string? trangThai,
            string? mucDoUuTien, ulong? khachHangId, uint? nhanVienXuLyId,
            CancellationToken ct = default)
        {
            var query = _context.Set<TkTicketEntity>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x =>
                    x.MaTicket.Contains(search) ||
                    x.TieuDe.Contains(search));

            if (!string.IsNullOrWhiteSpace(trangThai))
                query = query.Where(x => x.TrangThai == trangThai);

            if (!string.IsNullOrWhiteSpace(mucDoUuTien))
                query = query.Where(x => x.MucDoUuTien == mucDoUuTien);

            if (khachHangId.HasValue)
                query = query.Where(x => x.KhachHang_Id == khachHangId.Value);

            if (nhanVienXuLyId.HasValue)
                query = query.Where(x => x.NhanVienXuLy_Id == nhanVienXuLyId.Value);

            var total = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<Ticket>
            {
                Items = items.Select(MapToDomain).ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        public async Task<Ticket> AddAsync(Ticket ticket, CancellationToken ct = default)
        {
            var entity = MapToEntity(ticket);
            _context.Set<TkTicketEntity>().Add(entity);
            await _context.SaveChangesAsync(ct);
            ticket.Id = entity.Id;
            return ticket;
        }

        public async Task UpdateAsync(Ticket ticket, CancellationToken ct = default)
        {
            var entity = await _context.Set<TkTicketEntity>().FindAsync([ticket.Id], ct);
            if (entity is null) return;

            entity.TieuDe = ticket.TieuDe;
            entity.MoTa = ticket.MoTa;
            entity.FileDinhKem = ticket.FileDinhKem;
            entity.LoaiTicket_Id = ticket.LoaiTicketId;
            entity.HopDongId = ticket.HopDongId;
            entity.SanPham_Id = ticket.SanPhamId;
            entity.MucDoUuTien = ticket.MucDoUuTien.ToString();
            entity.NguonTiepNhan = ticket.NguonTiepNhan.ToString();
            entity.TrangThai = ticket.TrangThai.ToString();
            entity.NhanVienTiepNhan_Id = ticket.NhanVienTiepNhanId;
            entity.NhanVienXuLy_Id = ticket.NhanVienXuLyId;
            entity.NgayHenXuLy = ticket.NgayHenXuLy;
            entity.ThoiHanSLA = ticket.ThoiHanSLA;
            entity.SoLanEscalate = ticket.SoLanEscalate;
            entity.NgayDong = ticket.NgayDong;
            entity.LyDoDong = ticket.LyDoDong;
            entity.UpdatedAt = ticket.UpdatedAt;
        }

        public async Task<bool> DeleteAsync(ulong id, CancellationToken ct = default)
        {
            var entity = await _context.Set<TkTicketEntity>().FindAsync([id], ct);
            if (entity is null || entity.IsDeleted) return false;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            return true;
        }

        public async Task<string> GenerateMaTicketAsync(CancellationToken ct = default)
        {
            var lastTicket = await _context.Set<TkTicketEntity>()
                .IgnoreQueryFilters()
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync(ct);

            var nextNumber = (lastTicket?.Id ?? 0) + 1;
            return $"TK{nextNumber:D5}";
        }

        public async Task<List<TicketPhanHoi>> GetPhanHoisAsync(ulong ticketId, CancellationToken ct = default)
        {
            var entities = await _context.Set<TkTicketPhanHoiEntity>()
                .AsNoTracking()
                .Where(x => x.Ticket_Id == ticketId)
                .ToListAsync(ct);

            return entities.Select(MapPhanHoiToDomain).ToList();
        }

        public async Task<TicketPhanHoi> AddPhanHoiAsync(TicketPhanHoi phanHoi, CancellationToken ct = default)
        {
            var entity = new TkTicketPhanHoiEntity
            {
                Ticket_Id = phanHoi.TicketId,
                NguoiPhanHoi_Id = phanHoi.NguoiPhanHoiId,
                LoaiPhanHoi = phanHoi.LoaiPhanHoi.ToString(),
                NoiDung = phanHoi.NoiDung,
                FileDinhKem = phanHoi.FileDinhKem,
                TrangThaiTruoc = phanHoi.TrangThaiTruoc?.ToString(),
                TrangThaiSau = phanHoi.TrangThaiSau?.ToString(),
                CreatedAt = phanHoi.CreatedAt
            };

            _context.Set<TkTicketPhanHoiEntity>().Add(entity);
            await _context.SaveChangesAsync(ct);
            phanHoi.Id = entity.Id;
            return phanHoi;
        }

        public Task<bool> LoaiTicketExistsAsync(ushort id, CancellationToken ct = default) =>
            _context.Set<TkLoaiTicketEntity>().AnyAsync(x => x.Id == id && x.IsActive, ct);

        public Task<bool> KhachHangExistsAsync(ulong id, CancellationToken ct = default) =>
            _context.KhKhachHangs.AnyAsync(x => x.Id == id && !x.IsDeleted, ct);

        private static Ticket MapToDomain(TkTicketEntity e) => new()
        {
            Id = e.Id,
            MaTicket = e.MaTicket,
            TieuDe = e.TieuDe,
            MoTa = e.MoTa,
            FileDinhKem = e.FileDinhKem,
            LoaiTicketId = e.LoaiTicket_Id,
            KhachHangId = e.KhachHang_Id,
            HopDongId = e.HopDongId,
            SanPhamId = e.SanPham_Id,
            MucDoUuTien = Enum.Parse<TicketPriority>(e.MucDoUuTien),
            NguonTiepNhan = Enum.Parse<TicketSource>(e.NguonTiepNhan),
            TrangThai = Enum.Parse<TicketStatus>(e.TrangThai),
            NhanVienTiepNhanId = e.NhanVienTiepNhan_Id,
            NhanVienXuLyId = e.NhanVienXuLy_Id,
            NgayHenXuLy = e.NgayHenXuLy,
            ThoiHanSLA = e.ThoiHanSLA,
            SoLanEscalate = e.SoLanEscalate,
            NgayDong = e.NgayDong,
            LyDoDong = e.LyDoDong,
            IsDeleted = e.IsDeleted,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };

        private static TkTicketEntity MapToEntity(Ticket t) => new()
        {
            MaTicket = t.MaTicket,
            TieuDe = t.TieuDe,
            MoTa = t.MoTa,
            FileDinhKem = t.FileDinhKem,
            LoaiTicket_Id = t.LoaiTicketId,
            KhachHang_Id = t.KhachHangId,
            HopDongId = t.HopDongId,
            SanPham_Id = t.SanPhamId,
            MucDoUuTien = t.MucDoUuTien.ToString(),
            NguonTiepNhan = t.NguonTiepNhan.ToString(),
            TrangThai = t.TrangThai.ToString(),
            NhanVienTiepNhan_Id = t.NhanVienTiepNhanId,
            NhanVienXuLy_Id = t.NhanVienXuLyId,
            NgayHenXuLy = t.NgayHenXuLy,
            ThoiHanSLA = t.ThoiHanSLA,
            SoLanEscalate = t.SoLanEscalate,
            NgayDong = t.NgayDong,
            LyDoDong = t.LyDoDong,
            IsDeleted = t.IsDeleted,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        };


        public async Task<ushort?> GetLoaiTicketIdByNameAsync(string tenLoai, CancellationToken ct = default) =>
            await _context.TkLoaiTickets
                .AsNoTracking()
                .Where(x => x.TenLoai == tenLoai && x.IsActive)
                .Select(x => (ushort?)x.Id)
                .FirstOrDefaultAsync(ct);

        public async Task<ulong> CreateTicketForVoucherAsync(
            ulong khachHangId, ushort? loaiTicketId,
            string tieuDe, string moTa,
            CancellationToken ct = default)
        {
            var maTicket = await GenerateMaTicketAsync(ct);

            var entity = new TkTicketEntity
            {
                MaTicket          = maTicket,
                TieuDe            = tieuDe,
                MoTa              = moTa,
                LoaiTicket_Id     = loaiTicketId,
                KhachHang_Id      = khachHangId,
                MucDoUuTien       = "TrungBinh",
                NguonTiepNhan     = "Web",       // khách bấm link web
                TrangThai         = "Moi",
                IsDeleted         = false,
                CreatedAt         = DateTime.UtcNow,
                UpdatedAt         = DateTime.UtcNow,
            };

            _context.TkTickets.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity.Id;
        }

        public Task<int?> GetSlaSoGioXuLyAsync(string mucDoUuTien, CancellationToken ct = default) =>
            _context.Set<TkSlaEntity>()
                .AsNoTracking()
                .Where(x => x.MucDoUuTien == mucDoUuTien)
                .Select(x => (int?)x.SoGioXuLy)
                .FirstOrDefaultAsync(ct);

        private static TicketPhanHoi MapPhanHoiToDomain(TkTicketPhanHoiEntity e) => new()
        {
            Id = e.Id,
            TicketId = e.Ticket_Id,
            NguoiPhanHoiId = e.NguoiPhanHoi_Id,
            LoaiPhanHoi = Enum.Parse<TicketPhanHoiLoai>(e.LoaiPhanHoi),
            NoiDung = e.NoiDung,
            FileDinhKem = e.FileDinhKem,
            TrangThaiTruoc = string.IsNullOrEmpty(e.TrangThaiTruoc) ? null : Enum.Parse<TicketStatus>(e.TrangThaiTruoc),
            TrangThaiSau = string.IsNullOrEmpty(e.TrangThaiSau) ? null : Enum.Parse<TicketStatus>(e.TrangThaiSau),
            CreatedAt = e.CreatedAt
        };
    }
}
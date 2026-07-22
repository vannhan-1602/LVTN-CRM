using System.Security.Cryptography;
using CRM.Application.Features.Tickets.DTOs;
using CRM.Application.Interfaces.Tickets;
using CRM.Infrastructure.Persistence.Contexts;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Repositories;

public class CsatRepository : ICsatRepository
{
    private readonly CrmDbContext _context;
    public CsatRepository(CrmDbContext context) => _context = context;

    public async Task<string> CreateRequestAsync(ulong ticketId, CancellationToken ct = default)
    {
        var existing = await _context.TkDanhGiaHaiLongs.FindAsync(new object[] { ticketId }, ct);
        if (existing is not null) return existing.Token;

        var token = GenerateToken();
        _context.TkDanhGiaHaiLongs.Add(new TkDanhGiaHaiLongEntity
        {
            Ticket_Id = ticketId,
            Token = token,
            DaGuiEmail = false
        });
        await _context.SaveChangesAsync(ct);
        return token;
    }

    public async Task MarkEmailSentAsync(ulong ticketId, CancellationToken ct = default)
    {
        var entity = await _context.TkDanhGiaHaiLongs.FindAsync(new object[] { ticketId }, ct);
        if (entity is null) return;
        entity.DaGuiEmail = true;
        entity.NgayGuiEmail = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }

    public async Task<CsatDto?> GetByTokenAsync(string token, CancellationToken ct = default)
    {
        var row = await (
            from d in _context.TkDanhGiaHaiLongs.AsNoTracking()
            join t in _context.TkTickets.AsNoTracking() on d.Ticket_Id equals t.Id
            where d.Token == token
            select new CsatDto
            {
                TicketId = d.Ticket_Id,
                MaTicket = t.MaTicket,
                TieuDeTicket = t.TieuDe,
                DiemDanhGia = d.DiemDanhGia,
                NhanXet = d.NhanXet,
                NgayDanhGia = d.NgayDanhGia
            }).FirstOrDefaultAsync(ct);
        return row;
    }

    public async Task<CsatDto?> SubmitAsync(string token, byte diemDanhGia, string? nhanXet, CancellationToken ct = default)
    {
        var entity = await _context.TkDanhGiaHaiLongs.FirstOrDefaultAsync(x => x.Token == token, ct);
        if (entity is null || entity.DiemDanhGia.HasValue) return null;

        entity.DiemDanhGia = diemDanhGia;
        entity.NhanXet = nhanXet?.Trim();
        entity.NgayDanhGia = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return await GetByTokenAsync(token, ct);
    }

    private static string GenerateToken() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
}

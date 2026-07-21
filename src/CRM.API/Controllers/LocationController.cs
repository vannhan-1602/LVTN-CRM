using CRM.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class LocationController : ControllerBase
{
    private readonly CrmDbContext _context;

    public LocationController(CrmDbContext context)
    {
        _context = context;
    }

    [HttpGet("tinh-thanh")]
    public async Task<IActionResult> GetTinhThanhs()
    {
        var data = await _context.DmTinhThanhs
            .OrderBy(x => x.TenTinhThanh)
            .Select(x => new { id = x.Id, tenTinhThanh = x.TenTinhThanh })
            .ToListAsync();
        return Ok(data);
    }

    [HttpGet("tinh-thanh/{tinhThanhId}/phuong-xa")]
    public async Task<IActionResult> GetPhuongXas(uint tinhThanhId)
    {
        var data = await _context.DmPhuongXas
            .Where(x => x.TinhThanh_Id == tinhThanhId)
            .OrderBy(x => x.TenPhuongXa)
            .Select(x => new { id = x.Id, tenPhuongXa = x.TenPhuongXa })
            .ToListAsync();
        return Ok(data);
    }
}
using AutoMapper;

namespace CRM.Infrastructure.Persistence.Mappings;

/// <summary>
/// Profile mapping giữa EF Core Entities (Persistence/Entities) và Domain Models.
/// Sau khi chạy Scaffold-DbContext, bổ sung CreateMap cho từng cặp entity tại đây.
/// </summary>
public class PersistenceMappingProfile : Profile
{
    public PersistenceMappingProfile()
    {
        // Ví dụ sau scaffold:
        // CreateMap<Entities.KhKhachHang, Domain.Entities.Customers.KhachHang>().ReverseMap();
        // CreateMap<Entities.BhCoHoiBanHang, Domain.Entities.Sales.CoHoiBanHang>().ReverseMap();
        // CreateMap<Entities.TkTicket, Domain.Entities.Tickets.Ticket>().ReverseMap();
    }
}

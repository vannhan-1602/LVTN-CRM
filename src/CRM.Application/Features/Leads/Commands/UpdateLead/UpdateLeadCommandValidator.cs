using CRM.Application.Interfaces.Leads;
using CRM.Domain.Enums;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Features.Leads.Commands.UpdateLead
{
    public class UpdateLeadCommandValidator : AbstractValidator<UpdateLeadCommand>
    {
        public UpdateLeadCommandValidator(ILeadRepository leadRepository)
        {
            RuleFor(x => x.Id).GreaterThan(0UL).WithMessage("Id không hợp lệ.");
            RuleFor(x => x.TenLead)
                .NotEmpty().WithMessage("Tên lead không được để trống.")
                .MaximumLength(150);
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email không hợp lệ.")
                .MustAsync(async (cmd, email, ct) => !await leadRepository.EmailExistsAsync(email!, cmd.Id, ct))
                .WithMessage("Email này đã tồn tại ở một Lead khác.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));
            RuleFor(x => x.TinhTrang)
                // DaChuyenDoi CỐ Ý bị loại khỏi danh sách cho phép ở đây (khác LeadTinhTrang.All
                // dùng ở nơi khác) — trạng thái này chỉ được phép đổi qua ConvertLeadCommand, vì
                // đó là nơi DUY NHẤT thực sự tạo bản ghi KH_KhachHang tương ứng. Nếu cho phép set
                // trực tiếp qua Update, Lead sẽ bị đánh dấu "đã chuyển đổi" mà KHÔNG có Khách hàng
                // thật nào được tạo — sai lệch toàn bộ số liệu report tỷ lệ chuyển đổi Lead.
                .Must(t => t != LeadTinhTrang.DaChuyenDoi && LeadTinhTrang.All.Contains(t))
                .WithMessage("Không thể set trực tiếp trạng thái 'Đã chuyển đổi' — vui lòng dùng chức năng Chuyển đổi Lead.")
                .When(x => !string.IsNullOrWhiteSpace(x.TinhTrang));
        }
    }
}

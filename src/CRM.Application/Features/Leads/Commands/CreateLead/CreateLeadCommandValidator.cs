using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Features.Leads.Commands.CreateLead
{
    public class CreateLeadCommandValidator : AbstractValidator<CreateLeadCommand>
    {
        public CreateLeadCommandValidator()
        {
            RuleFor(x => x.TenLead)
                .NotEmpty().WithMessage("Tên lead không được để trống.")
                .MaximumLength(150).WithMessage("Tên lead không quá 150 ký tự.");
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));
            RuleFor(x => x.SoDienThoai)
                .MaximumLength(20).WithMessage("Số điện thoại không quá 20 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.SoDienThoai));
        }
    }
}

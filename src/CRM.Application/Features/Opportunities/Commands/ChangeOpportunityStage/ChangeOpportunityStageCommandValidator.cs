using CRM.Domain.Enums;
using FluentValidation;

namespace CRM.Application.Features.Opportunities.Commands.ChangeOpportunityStage
{
    public class ChangeOpportunityStageCommandValidator : AbstractValidator<ChangeOpportunityStageCommand>
    {
        public ChangeOpportunityStageCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0UL);

            RuleFor(x => x.GiaiDoan)
                .NotEmpty().WithMessage("Giai đoạn không được để trống.")
                .Must(g => Enum.TryParse<CoHoiGiaiDoan>(g, out _))
                .WithMessage(
                    $"Giai đoạn không hợp lệ. Chỉ chấp nhận: {string.Join(", ", Enum.GetNames<CoHoiGiaiDoan>())}.");
        }
    }
}
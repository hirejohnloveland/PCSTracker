using FluentValidation;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;

namespace PartsTrackerErp.GoldenPattern.Core.Validation.Validators;

public sealed class ListRefrigerantLogEntriesDtoValidator : PagedRequestDtoValidator<ListRefrigerantLogEntriesDto>
{
    public ListRefrigerantLogEntriesDtoValidator()
    {
        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom)
            .When(x => x.DateFrom is not null && x.DateTo is not null)
            .WithMessage("Date to must be on or after date from.");
    }
}

using FluentValidation;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;

namespace PartsTrackerErp.GoldenPattern.Core.Validation.Validators;

/// <summary>
/// The paging rules every list request shares. A validator for a list request derives from it and adds its own.
/// </summary>
public abstract class PagedRequestDtoValidator<T> : AbstractValidator<T>
    where T : PagedRequestDto
{
    protected PagedRequestDtoValidator()
    {
        // The offset, (page - 1) * pageSize, is passed to the database as an int.
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be 1 or greater.")
            .Must((request, page) => ((long)page - 1) * request.PageSize <= int.MaxValue)
            .WithMessage("Page is out of range.")
            .WithErrorCode("PageOutOfRange");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PagedRequestDto.MaxPageSize)
            .WithMessage($"Page size must be between 1 and {PagedRequestDto.MaxPageSize}.");
    }
}

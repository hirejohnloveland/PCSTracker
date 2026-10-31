using FluentValidation;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;

namespace PartsTrackerErp.GoldenPattern.Core.Validation.Validators;

public sealed class VendorInvoiceDtoValidator : AbstractValidator<VendorInvoiceDto>
{
    public VendorInvoiceDtoValidator()
    {
        RuleFor(x => x.VendorName)
            .NotEmpty()
            .WithMessage("Vendor name is required.");

        RuleFor(x => x.Balance)
            .GreaterThan(0)
            .WithMessage("Balance must be positive.");
    }
}

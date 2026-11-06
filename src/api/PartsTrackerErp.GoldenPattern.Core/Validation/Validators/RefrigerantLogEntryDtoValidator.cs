using FluentValidation;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities.Settings;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;

namespace PartsTrackerErp.GoldenPattern.Core.Validation.Validators;

public sealed class RefrigerantLogEntryDtoValidator : AbstractValidator<RefrigerantLogEntryDto>
{
    public const int TicketNumberMaxLength = 50;
    public const int JobNameMaxLength = 200;
    public const int VendorInvoiceRefMaxLength = 100;
    public const int NotesMaxLength = 4000;
    public const string TechnicianMessage = "Technician must be the full name of an existing technician.";
    public const string RefrigerantTypeMessage = "Refrigerant type must be one of the configured refrigerant types.";
    public const string CustomerMessage = "Customer must be the name of an existing customer.";

    public RefrigerantLogEntryDtoValidator(
        ISettingsRepository settingsRepository,
        ITechnicianRepository technicianRepository,
        ICustomerRepository customerRepository)
    {
        RuleFor(x => x.Date)
            .NotEqual(default(DateOnly))
            .WithMessage("Date is required.");

        RuleFor(x => x.Technician)
            .NotEmpty()
            .WithMessage("Technician is required.")
            .MustAsync((technician, cancellationToken) => technicianRepository.ExistsByNameAsync(technician.Trim(), cancellationToken))
            .When(x => !string.IsNullOrWhiteSpace(x.Technician), ApplyConditionTo.CurrentValidator)
            .WithMessage(TechnicianMessage);

        RuleFor(x => x.RefrigerantType)
            .NotEmpty()
            .WithMessage("Refrigerant type is required.")
            .MustAsync(async (refrigerantType, cancellationToken) =>
            {
                RefrigerantSettings settings = await settingsRepository.GetAsync<RefrigerantSettings>(cancellationToken);

                return settings.Refrigerants.Contains(refrigerantType.Trim(), StringComparer.Ordinal);
            })
            .When(x => !string.IsNullOrWhiteSpace(x.RefrigerantType), ApplyConditionTo.CurrentValidator)
            .WithMessage(RefrigerantTypeMessage);

        RuleFor(x => x.AmountLbs)
            .GreaterThan(0)
            .WithMessage("Amount used must be greater than zero.");

        RuleFor(x => x.Notes)
            .NotEmpty()
            .WithMessage("Notes are required.")
            .MaximumLength(NotesMaxLength)
            .WithMessage($"Notes must be {NotesMaxLength} characters or fewer.");

        RuleFor(x => x.TicketNumber)
            .MaximumLength(TicketNumberMaxLength)
            .WithMessage($"Ticket number must be {TicketNumberMaxLength} characters or fewer.");

        RuleFor(x => x.JobName)
            .MaximumLength(JobNameMaxLength)
            .WithMessage($"Job name must be {JobNameMaxLength} characters or fewer.");

        RuleFor(x => x.VendorInvoiceRef)
            .MaximumLength(VendorInvoiceRefMaxLength)
            .WithMessage($"Vendor invoice ref must be {VendorInvoiceRefMaxLength} characters or fewer.");

        RuleFor(x => x.Customer)
            .MustAsync((customer, cancellationToken) => customerRepository.ExistsByNameAsync(customer!.Trim(), cancellationToken))
            .When(x => !string.IsNullOrWhiteSpace(x.Customer))
            .WithMessage(CustomerMessage);
    }
}

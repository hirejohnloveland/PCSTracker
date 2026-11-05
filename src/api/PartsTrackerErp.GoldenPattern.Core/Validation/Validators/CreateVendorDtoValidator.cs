using System.Text.RegularExpressions;
using FluentValidation;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;

namespace PartsTrackerErp.GoldenPattern.Core.Validation.Validators;

public sealed partial class CreateVendorDtoValidator : AbstractValidator<CreateVendorDto>
{
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 500;
    public const int ContactMaxLength = 200;
    public const int PhoneMaxLength = 50;
    public const int NotesMaxLength = 4000;
    public const int QuickbooksIdMaxLength = 50;

    public CreateVendorDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Vendor name is required.")
            .MaximumLength(NameMaxLength)
            .WithMessage($"Vendor name must be {NameMaxLength} characters or fewer.");

        RuleFor(x => x.Description)
            .MaximumLength(DescriptionMaxLength)
            .WithMessage($"Description must be {DescriptionMaxLength} characters or fewer.");

        RuleFor(x => x.Contact)
            .MaximumLength(ContactMaxLength)
            .WithMessage($"Contact must be {ContactMaxLength} characters or fewer.");

        RuleFor(x => x.Phone)
            .MaximumLength(PhoneMaxLength)
            .WithMessage($"Phone must be {PhoneMaxLength} characters or fewer.")
            .Matches(PhonePattern())
            .When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("Phone may contain only digits, spaces, + - ( ) . and an optional extension such as x123.");

        RuleFor(x => x.Notes)
            .MaximumLength(NotesMaxLength)
            .WithMessage($"Notes must be {NotesMaxLength} characters or fewer.");

        RuleFor(x => x.QuickbooksId)
            .MaximumLength(QuickbooksIdMaxLength)
            .WithMessage($"QuickBooks Id must be {QuickbooksIdMaxLength} characters or fewer.");
    }

    // Digits with spaces and common punctuation, an optional leading + or (, and an optional extension (x123 or ext. 123).
    [GeneratedRegex(@"^\s*\+?\(?[0-9][0-9\s().\-]*(?:\s*(?:x|ext\.?)\s*[0-9]+)?\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex PhonePattern();
}

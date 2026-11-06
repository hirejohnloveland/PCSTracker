using System.Text.RegularExpressions;
using FluentValidation;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PhoneNumbers;

namespace PartsTrackerErp.GoldenPattern.Core.Validation.Validators;

public sealed partial class CustomerDtoValidator : AbstractValidator<CustomerDto>
{
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 500;
    public const int ContactMaxLength = 200;
    public const int PhoneMaxLength = 50;
    public const int NotesMaxLength = 4000;
    public const int QuickbooksIdMaxLength = 50;
    public const string PhoneMessage = "Phone must be a valid US or Canadian number, with an optional extension such as x123.";

    private static readonly PhoneNumberUtil s_phoneNumberUtil = PhoneNumberUtil.GetInstance();

    public CustomerDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Customer name is required.")
            .MaximumLength(NameMaxLength)
            .WithMessage($"Customer name must be {NameMaxLength} characters or fewer.");

        RuleFor(x => x.Description)
            .MaximumLength(DescriptionMaxLength)
            .WithMessage($"Description must be {DescriptionMaxLength} characters or fewer.");

        RuleFor(x => x.Contact)
            .MaximumLength(ContactMaxLength)
            .WithMessage($"Contact must be {ContactMaxLength} characters or fewer.");

        RuleFor(x => x.Phone)
            .MaximumLength(PhoneMaxLength)
            .WithMessage($"Phone must be {PhoneMaxLength} characters or fewer.")
            .Must(BeAUsOrCanadianNumber)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage(PhoneMessage);

        RuleFor(x => x.Notes)
            .MaximumLength(NotesMaxLength)
            .WithMessage($"Notes must be {NotesMaxLength} characters or fewer.");

        RuleFor(x => x.QuickbooksId)
            .MaximumLength(QuickbooksIdMaxLength)
            .WithMessage($"QuickBooks Id must be {QuickbooksIdMaxLength} characters or fewer.");
    }

    // Libphonenumber also reads letters as a vanity number such as 1-800-FLOWERS, so the characters are checked first:
    // digits, spaces and common punctuation, an optional leading + or (, and an optional extension (x123 or ext. 123).
    [GeneratedRegex(@"^\s*\+?\(?[0-9][0-9\s().\-]*(?:\s*(?:x|ext\.?)\s*[0-9]+)?\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex PhoneCharacters();

    private static bool BeAUsOrCanadianNumber(string? phone)
    {
        if (phone is null || !PhoneCharacters().IsMatch(phone))
        {
            return false;
        }

        try
        {
            PhoneNumber number = s_phoneNumberUtil.Parse(phone, "US");

            return s_phoneNumberUtil.IsValidNumber(number)
                && s_phoneNumberUtil.GetRegionCodeForNumber(number) is "US" or "CA";
        }
        catch (NumberParseException)
        {
            return false;
        }
    }
}

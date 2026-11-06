using System.Text.RegularExpressions;
using FluentValidation;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities.Settings;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;
using PhoneNumbers;

namespace PartsTrackerErp.GoldenPattern.Core.Validation.Validators;

public sealed partial class TechnicianDtoValidator : AbstractValidator<TechnicianDto>
{
    public const int FullNameMaxLength = 200;
    public const int PhoneMaxLength = 50;
    public const int NotesMaxLength = 4000;
    public const int QuickbooksIdMaxLength = 50;
    public const string PhoneMessage = "Phone must be a valid US or Canadian number, with an optional extension such as x123.";
    public const string RoleMessage = "Role must be one of the configured technician roles.";

    private static readonly PhoneNumberUtil s_phoneNumberUtil = PhoneNumberUtil.GetInstance();

    public TechnicianDtoValidator(ISettingsRepository settingsRepository)
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Full name is required.")
            .MaximumLength(FullNameMaxLength)
            .WithMessage($"Full name must be {FullNameMaxLength} characters or fewer.");

        RuleFor(x => x.Role)
            .NotEmpty()
            .WithMessage("Role is required.")
            .MustAsync(async (role, cancellationToken) =>
            {
                TechnicianRoleSettings settings = await settingsRepository.GetAsync<TechnicianRoleSettings>(cancellationToken);

                return settings.Roles.Contains(role.Trim(), StringComparer.Ordinal);
            })
            .When(x => !string.IsNullOrWhiteSpace(x.Role), ApplyConditionTo.CurrentValidator)
            .WithMessage(RoleMessage);

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

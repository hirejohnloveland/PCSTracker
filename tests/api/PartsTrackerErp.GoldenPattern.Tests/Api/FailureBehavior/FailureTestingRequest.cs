using FluentValidation;

namespace PartsTrackerErp.GoldenPattern.Tests.Api.FailureBehavior;

public sealed class FailureTestingRequest
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    // A string in the JSON body fails model binding instead of validation.
    public int Count { get; set; }

    public List<FailureTestingItem> Items { get; set; } = [];
}

public sealed class FailureTestingItem
{
    public string Sku { get; set; } = string.Empty;
}

public sealed class FailureTestingRequestValidator : AbstractValidator<FailureTestingRequest>
{
    public FailureTestingRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Email).EmailAddress().WithMessage("Email address is invalid.");
        RuleForEach(x => x.Items)
            .ChildRules(item => item.RuleFor(x => x.Sku).NotEmpty().WithMessage("Sku is required."));
    }
}

using System.Diagnostics;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PartsTrackerErp.GoldenPattern.Api.ErrorHandling;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;

namespace PartsTrackerErp.GoldenPattern.Tests.Api.ErrorHandling;

[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_ProblemDetailsMapper
{
    private const string TypeBase = "https://api.example.com/problems/";

    private static readonly JsonSerializerOptions s_webOptions = new(JsonSerializerDefaults.Web);

    private static readonly Dictionary<ErrorType, (int Status, string Category)> s_categories = new()
    {
        [ErrorType.Validation] = (StatusCodes.Status400BadRequest, "validation"),
        [ErrorType.Unauthorized] = (StatusCodes.Status401Unauthorized, "unauthorized"),
        [ErrorType.Forbidden] = (StatusCodes.Status403Forbidden, "forbidden"),
        [ErrorType.NotFound] = (StatusCodes.Status404NotFound, "not-found"),
        [ErrorType.BusinessRuleViolation] = (StatusCodes.Status409Conflict, "business-rule-violation"),
        [ErrorType.Unexpected] = (StatusCodes.Status500InternalServerError, "unexpected")
    };

    public static TheoryData<ErrorType, int, string> CategoryCases()
    {
        var cases = new TheoryData<ErrorType, int, string>();
        foreach ((ErrorType errorType, (int status, string category)) in s_categories)
        {
            cases.Add(errorType, status, category);
        }

        return cases;
    }

    private static DomainError Error(string code, string message)
    {
        return new() { Code = code, Message = message };
    }

    private static DomainErrorEnvelope Envelope(ErrorType errorType, params DomainError[] errors)
    {
        return new() { ErrorType = errorType, Errors = errors };
    }

    private static ProblemError[] ErrorsOf(ProblemDetails problem)
    {
        return Assert.IsType<ProblemError[]>(problem.Extensions["errors"]);
    }

    private static string[] PropertyNames(JsonElement element)
    {
        return [.. element.EnumerateObject().Select(property => property.Name).OrderBy(name => name, StringComparer.Ordinal)];
    }

    // ---- Envelope ----

    [Theory]
    [MemberData(nameof(CategoryCases))]
    public void ToProblemDetails_Envelope_SetsTheStatusAndTheCategoryType(ErrorType errorType, int status, string category)
    {
        ProblemDetails problem = ProblemDetailsMapper.ToProblemDetails(Envelope(errorType, Error("Code", "Message.")));

        Assert.Equal(status, problem.Status);
        Assert.Equal(TypeBase + category, problem.Type);
    }

    [Fact]
    public void ToProblemDetails_Envelope_EveryErrorTypeIsCoveredByTheseTests()
    {
        // Fails when an ErrorType is added without a decision about its problem type.
        Assert.Equal(
            Enum.GetValues<ErrorType>().OrderBy(errorType => errorType),
            s_categories.Keys.OrderBy(errorType => errorType));
    }

    [Fact]
    public void ToProblemDetails_Envelope_UndefinedErrorType_Is500Unexpected()
    {
        ProblemDetails problem = ProblemDetailsMapper.ToProblemDetails(Envelope((ErrorType)999, Error("Code", "Message.")));

        Assert.Equal(StatusCodes.Status500InternalServerError, problem.Status);
        Assert.Equal(TypeBase + "unexpected", problem.Type);
    }

    [Fact]
    public void ToProblemDetails_Envelope_CarriesEachAuthoredErrorInOrderWithoutAPointer()
    {
        ProblemDetails problem = ProblemDetailsMapper.ToProblemDetails(Envelope(
            ErrorType.BusinessRuleViolation,
            Error("First", "First problem."),
            Error("Second", "Second problem.")));

        Assert.Equal(
            [new ProblemError("First", "First problem.", null), new ProblemError("Second", "Second problem.", null)],
            ErrorsOf(problem));
    }

    [Fact]
    public void ToProblemDetails_Envelope_OneError_UsesItsMessageAsTheDetail()
    {
        ProblemDetails problem = ProblemDetailsMapper.ToProblemDetails(
            Envelope(ErrorType.NotFound, Error("VendorNotFound", "Vendor 7 was not found.")));

        Assert.Equal("Vendor 7 was not found.", problem.Detail);
    }

    [Fact]
    public void ToProblemDetails_Envelope_SeveralValidationErrors_UseTheValidationSummary()
    {
        ProblemDetails problem = ProblemDetailsMapper.ToProblemDetails(
            Envelope(ErrorType.Validation, Error("A", "First."), Error("B", "Second.")));

        Assert.Equal("One or more validation errors occurred.", problem.Detail);
    }

    [Fact]
    public void ToProblemDetails_Envelope_SeveralOtherErrors_UseTheGenericSummary()
    {
        ProblemDetails problem = ProblemDetailsMapper.ToProblemDetails(
            Envelope(ErrorType.BusinessRuleViolation, Error("A", "First."), Error("B", "Second.")));

        Assert.Equal("One or more errors occurred.", problem.Detail);
    }

    [Fact]
    public void ToProblemDetails_Envelope_NoErrors_HasNoErrorsMember()
    {
        ProblemDetails problem = ProblemDetailsMapper.ToProblemDetails(Envelope(ErrorType.Forbidden));

        Assert.False(problem.Extensions.ContainsKey("errors"));
        Assert.Equal("One or more errors occurred.", problem.Detail);
    }

    [Fact]
    public void ToProblemDetails_Envelope_ValidationHasItsOwnTitle()
    {
        ProblemDetails problem = ProblemDetailsMapper.ToProblemDetails(Envelope(ErrorType.Validation, Error("A", "First.")));

        Assert.Equal("Validation Failed", problem.Title);
    }

    [Theory]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.BusinessRuleViolation)]
    [InlineData(ErrorType.Unexpected)]
    public void ToProblemDetails_Envelope_OtherCategoriesLeaveTheTitleToTheFramework(ErrorType errorType)
    {
        ProblemDetails problem = ProblemDetailsMapper.ToProblemDetails(Envelope(errorType, Error("A", "First.")));

        Assert.Null(problem.Title);
    }

    // ---- Request validation failures ----

    [Fact]
    public void ToProblemDetails_ValidationFailures_Gives400WithOneEntryPerFailure()
    {
        ProblemDetails problem = ProblemDetailsMapper.ToProblemDetails(
        [
            new ValidationFailure("Name", "Vendor name is required.") { ErrorCode = "NotEmptyValidator" },
            new ValidationFailure("Email", "Email address is invalid.") { ErrorCode = "EmailValidator" }
        ]);

        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal("Validation Failed", problem.Title);
        Assert.Equal(TypeBase + "validation", problem.Type);
        Assert.Equal("One or more validation errors occurred.", problem.Detail);
        Assert.Equal(
            [
                new ProblemError("NotEmptyValidator", "Vendor name is required.", "#/name"),
                new ProblemError("EmailValidator", "Email address is invalid.", "#/email")
            ],
            ErrorsOf(problem));
    }

    [Fact]
    public void ToProblemDetails_ValidationFailures_OneFailure_UsesItsMessageAsTheDetail()
    {
        ProblemDetails problem = ProblemDetailsMapper.ToProblemDetails(
            [new ValidationFailure("Name", "Vendor name is required.") { ErrorCode = "NotEmptyValidator" }]);

        Assert.Equal("Vendor name is required.", problem.Detail);
    }

    [Fact]
    public void ToProblemDetails_ValidationFailures_WithoutAnErrorCode_UseInvalidRequest()
    {
        ProblemDetails problem = ProblemDetailsMapper.ToProblemDetails([new ValidationFailure("Name", "Not good.")]);

        Assert.Equal("InvalidRequest", Assert.Single(ErrorsOf(problem)).Code);
    }

    [Theory]
    [InlineData("Name", "#/name")]
    [InlineData("VendorName", "#/vendorName")]
    [InlineData("Items[0].Name", "#/items/0/name")]
    [InlineData("Address.Line1", "#/address/line1")]
    [InlineData("Items[2].Tags[10]", "#/items/2/tags/10")]
    [InlineData("", "#")]
    [InlineData("a/b", "#/a~1b")]
    [InlineData("a~b", "#/a~0b")]
    public void ToProblemDetails_ValidationFailures_PointAtTheCamelCasedPath(string propertyName, string expectedPointer)
    {
        ProblemDetails problem = ProblemDetailsMapper.ToProblemDetails([new ValidationFailure(propertyName, "Not good.")]);

        Assert.Equal(expectedPointer, Assert.Single(ErrorsOf(problem)).Pointer);
    }

    [Fact]
    public async Task ToProblemDetails_FailuresFromARealValidator_PointAtTheNestedProperty()
    {
        ValidationResult result = await new OrderValidator().ValidateAsync(
            new Order { Items = [new OrderLine { Name = "Bolt" }, new OrderLine()] });

        ProblemDetails problem = ProblemDetailsMapper.ToProblemDetails(result.Errors);

        ProblemError error = Assert.Single(ErrorsOf(problem));
        Assert.Equal("NotEmptyValidator", error.Code);
        Assert.Equal("#/items/1/name", error.Pointer);
    }

    // ---- Model state ----

    [Fact]
    public void ToProblemDetails_ModelState_Gives400WithAnInvalidRequestEntryPerError()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Name", "Name is required.");
        modelState.AddModelError("$.count", "The JSON value could not be converted to System.Int32.");
        modelState.AddModelError("Items[0].Sku", "Sku is required.");

        ProblemDetails problem = ProblemDetailsMapper.ToProblemDetails(modelState);

        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal("Validation Failed", problem.Title);
        Assert.Equal(TypeBase + "validation", problem.Type);
        Assert.Equal("One or more validation errors occurred.", problem.Detail);
        ProblemError[] errors = ErrorsOf(problem);
        Assert.Equal(3, errors.Length);
        Assert.Contains(new ProblemError("InvalidRequest", "Name is required.", "#/name"), errors);
        Assert.Contains(new ProblemError("InvalidRequest", "The JSON value could not be converted to System.Int32.", "#/count"), errors);
        Assert.Contains(new ProblemError("InvalidRequest", "Sku is required.", "#/items/0/sku"), errors);
    }

    [Fact]
    public void ToProblemDetails_ModelState_ErrorThatCarriesAnException_DoesNotLeakItsText()
    {
        var modelState = new ModelStateDictionary();
        modelState.TryAddModelException("Name", new InvalidOperationException("Server=secret;Password=hunter2."));

        ProblemDetails problem = ProblemDetailsMapper.ToProblemDetails(modelState);

        ProblemError error = Assert.Single(ErrorsOf(problem));
        Assert.Equal("The input was not valid.", error.Detail);
        Assert.DoesNotContain("hunter2", JsonSerializer.Serialize(problem, s_webOptions));
    }

    // ---- Status without an ErrorType ----

    [Fact]
    public void ToProblemDetails_StatusAndError_KeepsTheStatusWithNoCategoryType()
    {
        var error = new ProblemError("UpstreamServiceUnavailable", "A required upstream service is unavailable.", null);

        ProblemDetails problem = ProblemDetailsMapper.ToProblemDetails(StatusCodes.Status502BadGateway, error);

        Assert.Equal(StatusCodes.Status502BadGateway, problem.Status);
        Assert.Null(problem.Type);
        Assert.Equal(error.Detail, problem.Detail);
        Assert.Equal([error], ErrorsOf(problem));
    }

    [Fact]
    public void ToProblemDetails_Status_CarriesTheDefaultEntryForThatStatus()
    {
        ProblemDetails problem = ProblemDetailsMapper.ToProblemDetails(StatusCodes.Status413PayloadTooLarge);

        Assert.Equal(StatusCodes.Status413PayloadTooLarge, problem.Status);
        ProblemError error = Assert.Single(ErrorsOf(problem));
        Assert.Equal("InvalidRequest", error.Code);
        Assert.False(string.IsNullOrWhiteSpace(error.Detail));
    }

    // ---- Customize ----

    private static ProblemDetails Customized(
        int? status,
        string path = "/vendors/7",
        Action<ProblemDetails>? configure = null,
        int responseStatus = StatusCodes.Status200OK)
    {
        var httpContext = new DefaultHttpContext { Request = { Path = path }, Response = { StatusCode = responseStatus } };
        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails { Status = status }
        };
        configure?.Invoke(context.ProblemDetails);

        ProblemDetailsMapper.Customize(context);

        return context.ProblemDetails;
    }

    [Fact]
    public void Customize_SetsTheInstanceToTheRequestPath()
    {
        Assert.Equal("/vendors/7", Customized(StatusCodes.Status404NotFound).Instance);
    }

    [Fact]
    public void Customize_KeepsAnInstanceThatIsAlreadySet()
    {
        ProblemDetails problem = Customized(
            StatusCodes.Status404NotFound,
            configure: details => details.Instance = "/somewhere/else");

        Assert.Equal("/somewhere/else", problem.Instance);
    }

    [Fact]
    public void Customize_NoRequestPath_LeavesTheInstanceEmpty()
    {
        Assert.Null(Customized(StatusCodes.Status404NotFound, path: string.Empty).Instance);
    }

    [Theory]
    [MemberData(nameof(CategoryCases))]
    public void Customize_GivesAStatusWithACategoryItsCategoryType(ErrorType _, int status, string category)
    {
        ProblemDetails problem = Customized(
            status,
            configure: details => details.Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1");

        Assert.Equal(TypeBase + category, problem.Type);
    }

    [Theory]
    [InlineData(StatusCodes.Status405MethodNotAllowed)]
    [InlineData(StatusCodes.Status413PayloadTooLarge)]
    [InlineData(StatusCodes.Status502BadGateway)]
    [InlineData(StatusCodes.Status504GatewayTimeout)]
    public void Customize_LeavesTheFrameworkTypeOfAStatusWithoutACategory(int status)
    {
        ProblemDetails problem = Customized(status, configure: details => details.Type = "https://example.test/framework-link");

        Assert.Equal("https://example.test/framework-link", problem.Type);
    }

    [Theory]
    [InlineData(StatusCodes.Status400BadRequest, "InvalidRequest")]
    [InlineData(StatusCodes.Status401Unauthorized, "Unauthorized")]
    [InlineData(StatusCodes.Status403Forbidden, "Forbidden")]
    [InlineData(StatusCodes.Status404NotFound, "NotFound")]
    [InlineData(StatusCodes.Status405MethodNotAllowed, "InvalidRequest")]
    [InlineData(StatusCodes.Status409Conflict, "InvalidRequest")]
    [InlineData(StatusCodes.Status415UnsupportedMediaType, "InvalidRequest")]
    [InlineData(StatusCodes.Status500InternalServerError, "Unexpected")]
    [InlineData(StatusCodes.Status502BadGateway, "Unexpected")]
    [InlineData(StatusCodes.Status503ServiceUnavailable, "Unexpected")]
    public void Customize_AddsTheDefaultEntryForTheStatusWhenThereAreNoErrors(int status, string expectedCode)
    {
        ProblemDetails problem = Customized(status);

        ProblemError error = Assert.Single(ErrorsOf(problem));
        Assert.Equal(expectedCode, error.Code);
        Assert.False(string.IsNullOrWhiteSpace(error.Detail));
        Assert.Null(error.Pointer);
    }

    [Fact]
    public void Customize_ServerErrorDefault_SaysAnUnexpectedErrorOccurred()
    {
        ProblemError error = Assert.Single(ErrorsOf(Customized(StatusCodes.Status500InternalServerError)));

        Assert.Equal("An unexpected error occurred.", error.Detail);
    }

    [Fact]
    public void Customize_OtherClientErrorDefault_UsesTheStatusName()
    {
        ProblemError error = Assert.Single(ErrorsOf(Customized(StatusCodes.Status405MethodNotAllowed)));

        Assert.Equal("Method Not Allowed", error.Detail);
    }

    [Fact]
    public void Customize_KeepsErrorsThatAreAlreadyThere()
    {
        var authored = new ProblemError("VendorNotFound", "Vendor 7 was not found.", null);

        ProblemDetails problem = Customized(
            StatusCodes.Status404NotFound,
            configure: details => details.Extensions["errors"] = new[] { authored });

        Assert.Equal([authored], ErrorsOf(problem));
    }

    [Theory]
    [InlineData(StatusCodes.Status200OK)]
    [InlineData(StatusCodes.Status302Found)]
    public void Customize_AddsNoEntryBelow400(int status)
    {
        Assert.False(Customized(status).Extensions.ContainsKey("errors"));
    }

    [Fact]
    public void Customize_ProblemWithoutAStatus_UsesTheResponseStatus()
    {
        ProblemDetails problem = Customized(status: null, responseStatus: StatusCodes.Status403Forbidden);

        Assert.Equal(TypeBase + "forbidden", problem.Type);
        Assert.Equal("Forbidden", Assert.Single(ErrorsOf(problem)).Code);
    }

    // ---- Wire shape ----

    [Fact]
    public void Serialized_UsesCamelCaseWireNamesAndOmitsANullPointer()
    {
        ProblemDetails problem = ProblemDetailsMapper.ToProblemDetails(
            Envelope(ErrorType.NotFound, Error("VendorNotFound", "Vendor 7 was not found.")));

        using JsonDocument document = JsonDocument.Parse(JsonSerializer.Serialize(problem, s_webOptions));

        JsonElement error = Assert.Single(document.RootElement.GetProperty("errors").EnumerateArray());
        Assert.Equal(["code", "detail"], PropertyNames(error));
        Assert.Equal("VendorNotFound", error.GetProperty("code").GetString());
        Assert.Equal("Vendor 7 was not found.", error.GetProperty("detail").GetString());
    }

    [Fact]
    public void Serialized_WritesThePointerWhenThereIsOne()
    {
        ProblemDetails problem = ProblemDetailsMapper.ToProblemDetails([new ValidationFailure("Name", "Required.")]);

        using JsonDocument document = JsonDocument.Parse(JsonSerializer.Serialize(problem, s_webOptions));

        JsonElement error = Assert.Single(document.RootElement.GetProperty("errors").EnumerateArray());
        Assert.Equal(["code", "detail", "pointer"], PropertyNames(error));
        Assert.Equal("#/name", error.GetProperty("pointer").GetString());
    }

    // ---- Built through the framework factory ----

    [Fact]
    public void ToObjectResult_BuildsThroughTheFrameworkFactoryAndKeepsTheAuthoredErrors()
    {
        using var services = ErrorHandlingTestServices.Build();
        DefaultHttpContext httpContext = ErrorHandlingTestServices.NewHttpContext(services, "/vendors/7/activate");
        ProblemDetails authored = ProblemDetailsMapper.ToProblemDetails(
            Envelope(ErrorType.NotFound, Error("VendorNotFound", "Vendor 7 was not found.")));

        ObjectResult result = authored.ToObjectResult(httpContext);

        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Equal(["application/problem+json"], result.ContentTypes);
        var built = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal("Not Found", built.Title);
        Assert.Equal(TypeBase + "not-found", built.Type);
        Assert.Equal("Vendor 7 was not found.", built.Detail);
        Assert.Equal("/vendors/7/activate", built.Instance);
        Assert.False(string.IsNullOrEmpty(Assert.IsType<string>(built.Extensions["traceId"])));
        Assert.Equal([new ProblemError("VendorNotFound", "Vendor 7 was not found.", null)], ErrorsOf(built));
    }

    // ---- Trace id ----

    [Fact]
    public void GetTraceId_WithoutAnActivity_IsTheRequestsTraceIdentifier()
    {
        var httpContext = new DefaultHttpContext { TraceIdentifier = "request-42" };

        Assert.Null(Activity.Current);
        Assert.Equal("request-42", ProblemDetailsMapper.GetTraceId(httpContext));
    }

    [Fact]
    public void GetTraceId_WithAnActivity_IsTheActivityId()
    {
        var httpContext = new DefaultHttpContext { TraceIdentifier = "request-42" };
        using var activity = new Activity("test").Start();

        Assert.Equal(activity.Id, ProblemDetailsMapper.GetTraceId(httpContext));
    }

    private sealed class Order
    {
        public List<OrderLine> Items { get; set; } = [];
    }

    private sealed class OrderLine
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class OrderValidator : AbstractValidator<Order>
    {
        public OrderValidator()
        {
            RuleForEach(order => order.Items).ChildRules(line => line.RuleFor(item => item.Name).NotEmpty());
        }
    }
}

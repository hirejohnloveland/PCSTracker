using Microsoft.AspNetCore.Http;
using PartsTrackerErp.GoldenPattern.Api.ErrorHandling;
using PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;

namespace PartsTrackerErp.GoldenPattern.Tests.Api.ErrorHandling;

[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_ErrorTypeExtensions
{
    private static readonly Dictionary<ErrorType, int> s_expectedStatusCodes = new()
    {
        [ErrorType.Validation] = StatusCodes.Status400BadRequest,
        [ErrorType.Unauthorized] = StatusCodes.Status401Unauthorized,
        [ErrorType.Forbidden] = StatusCodes.Status403Forbidden,
        [ErrorType.NotFound] = StatusCodes.Status404NotFound,
        [ErrorType.BusinessRuleViolation] = StatusCodes.Status409Conflict,
        [ErrorType.Unexpected] = StatusCodes.Status500InternalServerError
    };

    public static TheoryData<ErrorType, int> StatusCodeCases()
    {
        var cases = new TheoryData<ErrorType, int>();
        foreach ((ErrorType errorType, int statusCode) in s_expectedStatusCodes)
        {
            cases.Add(errorType, statusCode);
        }

        return cases;
    }

    [Theory]
    [MemberData(nameof(StatusCodeCases))]
    public void ToHttpStatusCode_MapsEachErrorTypeToItsStatusCode(ErrorType errorType, int expected)
    {
        Assert.Equal(expected, errorType.ToHttpStatusCode());
    }

    [Fact]
    public void ToHttpStatusCode_EveryErrorTypeIsCoveredByTheseTests()
    {
        // Fails when an ErrorType is added without a decision about its status code.
        Assert.Equal(
            Enum.GetValues<ErrorType>().OrderBy(errorType => errorType),
            s_expectedStatusCodes.Keys.OrderBy(errorType => errorType));
    }

    [Fact]
    public void ToHttpStatusCode_UndefinedErrorType_Maps500()
    {
        Assert.Equal(StatusCodes.Status500InternalServerError, ((ErrorType)999).ToHttpStatusCode());
    }
}

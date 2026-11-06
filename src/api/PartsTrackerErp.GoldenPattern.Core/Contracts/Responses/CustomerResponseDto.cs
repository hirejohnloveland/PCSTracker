namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

/// <summary>
/// A customer as the API reports it. <paramref name="QuickbooksId"/> is null for a customer that is not linked to QuickBooks.
/// <paramref name="Description"/>, <paramref name="Contact"/>, <paramref name="Phone"/> and <paramref name="Notes"/> are
/// null when none was provided.
/// </summary>
public sealed record CustomerResponseDto(
    string Id,
    string Name,
    string? QuickbooksId,
    string? Description,
    string? Contact,
    string? Phone,
    string? Notes);

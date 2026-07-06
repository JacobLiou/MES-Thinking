namespace MES.Domain.Services;

public sealed class FlowValidationResult
{
    public required bool IsValid { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }

    public static FlowValidationResult Ok() => new() { IsValid = true };

    public static FlowValidationResult Fail(string code, string message) =>
        new()
        {
            IsValid = false,
            ErrorCode = code,
            ErrorMessage = message
        };
}

namespace ChequeToWords.Core;

/// <summary>Outcome of converting a raw cheque-amount string into words.</summary>
public sealed class ConversionResult
{
    public bool Success { get; }
    public string? Words { get; }
    public string? Error { get; }
    public bool WasRounded { get; }

    private ConversionResult(bool success, string? words, string? error, bool wasRounded)
    {
        Success = success;
        Words = words;
        Error = error;
        WasRounded = wasRounded;
    }

    public static ConversionResult Ok(string words, bool wasRounded) =>
        new(true, words, null, wasRounded);

    public static ConversionResult Fail(string error) =>
        new(false, null, error, false);
}

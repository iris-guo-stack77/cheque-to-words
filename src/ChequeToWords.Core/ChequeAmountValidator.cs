using System.Globalization;

namespace ChequeToWords.Core;

/// <summary>Parses and validates raw user input into a safe, rounded cheque amount.</summary>
public static class ChequeAmountValidator
{
    /// <summary>
    /// Largest amount <see cref="AmountToWordsConverter"/> can render (its grouping
    /// covers thousand/million/billion — one less than a trillion).
    /// </summary>
    public const decimal MaxSupportedAmount = 999_999_999_999.99m;

    public static bool TryParse(string? rawInput, out decimal amount, out bool wasRounded, out string? error)
    {
        amount = 0m;
        wasRounded = false;
        error = null;

        if (string.IsNullOrWhiteSpace(rawInput))
        {
            error = "Enter a cheque amount.";
            return false;
        }

        var cleaned = rawInput.Trim().Replace("$", "").Replace(",", "");

        const NumberStyles styles = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
        if (!decimal.TryParse(cleaned, styles, CultureInfo.InvariantCulture, out var parsed))
        {
            error = "Enter a valid number, e.g. 1234.56.";
            return false;
        }

        if (parsed < 0)
        {
            error = "Cheque amount cannot be negative.";
            return false;
        }

        if (parsed > MaxSupportedAmount)
        {
            error = $"Amount is too large — the maximum supported is {MaxSupportedAmount:N2}.";
            return false;
        }

        var rounded = decimal.Round(parsed, 2, MidpointRounding.AwayFromZero);
        wasRounded = rounded != parsed;
        amount = rounded;
        return true;
    }
}

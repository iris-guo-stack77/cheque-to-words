namespace ChequeToWords.Core;

/// <summary>Single entry point web/console callers use: validate raw input, then convert.</summary>
public static class ChequeAmountService
{
    public static ConversionResult Convert(string? rawInput)
    {
        if (!ChequeAmountValidator.TryParse(rawInput, out var amount, out var wasRounded, out var error))
            return ConversionResult.Fail(error!);

        var words = AmountToWordsConverter.Convert(amount);
        return ConversionResult.Ok(words, wasRounded);
    }
}

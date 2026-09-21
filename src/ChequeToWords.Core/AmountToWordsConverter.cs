using System.Text;

namespace ChequeToWords.Core;

/// <summary>
/// Converts a non-negative dollar amount into British/NZ English words, e.g.
/// 1234.56 -> "One thousand, two hundred and thirty-four dollars and fifty-six cents."
/// Assumes the caller (<see cref="ChequeAmountValidator"/>) has already validated the
/// amount is non-negative and within <see cref="ChequeAmountValidator.MaxSupportedAmount"/>.
/// </summary>
public static class AmountToWordsConverter
{
    private static readonly string[] Ones =
    [
        "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine",
        "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen",
        "seventeen", "eighteen", "nineteen"
    ];

    private static readonly string[] Tens =
    [
        "", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety"
    ];

    private static readonly string[] Scales = ["", "thousand", "million", "billion"];

    public static string Convert(decimal amount)
    {
        var dollars = (long)decimal.Truncate(amount);
        var cents = (int)decimal.Round((amount - dollars) * 100m, 0, MidpointRounding.AwayFromZero);
        if (cents == 100)
        {
            cents = 0;
            dollars++;
        }

        var dollarsWords = Capitalize(ConvertInteger(dollars));
        var dollarUnit = dollars == 1 ? "dollar" : "dollars";

        if (cents == 0)
            return $"{dollarsWords} {dollarUnit} only.";

        var centsWords = ConvertUnderThousand(cents);
        var centUnit = cents == 1 ? "cent" : "cents";
        return $"{dollarsWords} {dollarUnit} and {centsWords} {centUnit}.";
    }

    private static string ConvertInteger(long n)
    {
        if (n == 0) return "zero";

        var groups = new List<(int Value, string Scale)>();
        var remaining = n;
        var scaleIndex = 0;
        while (remaining > 0)
        {
            groups.Add(((int)(remaining % 1000), Scales[scaleIndex]));
            remaining /= 1000;
            scaleIndex++;
        }
        groups.Reverse();

        var nonZeroGroups = groups.Where(g => g.Value != 0).ToList();

        var sb = new StringBuilder();
        for (var i = 0; i < nonZeroGroups.Count; i++)
        {
            var (value, scale) = nonZeroGroups[i];
            var groupWords = ConvertUnderThousand(value);
            var withScale = scale.Length == 0 ? groupWords : $"{groupWords} {scale}";

            if (i == 0)
            {
                sb.Append(withScale);
                continue;
            }

            var isLast = i == nonZeroGroups.Count - 1;
            var connector = isLast && value < 100 ? " and " : ", ";
            sb.Append(connector).Append(withScale);
        }

        return sb.ToString();
    }

    private static string ConvertUnderThousand(int n)
    {
        if (n < 20) return Ones[n];
        if (n < 100)
        {
            var tens = Tens[n / 10];
            var remainder = n % 10;
            return remainder == 0 ? tens : $"{tens}-{Ones[remainder]}";
        }

        var hundredsDigit = n / 100;
        var rest = n % 100;
        var hundredsPart = $"{Ones[hundredsDigit]} hundred";
        return rest == 0 ? hundredsPart : $"{hundredsPart} and {ConvertUnderThousand(rest)}";
    }

    private static string Capitalize(string words) =>
        words.Length == 0 ? words : char.ToUpperInvariant(words[0]) + words[1..];
}

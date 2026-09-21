using ChequeToWords.Core;
using Xunit;

namespace ChequeToWords.Core.Tests;

public class ChequeAmountValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryParse_RejectsEmptyInput(string? input)
    {
        var ok = ChequeAmountValidator.TryParse(input, out _, out _, out var error);

        Assert.False(ok);
        Assert.Equal("Enter a cheque amount.", error);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("12.34.56")]
    [InlineData("1e5")] // scientific notation not accepted
    [InlineData("--5")]
    public void TryParse_RejectsNonNumericInput(string input)
    {
        var ok = ChequeAmountValidator.TryParse(input, out _, out _, out var error);

        Assert.False(ok);
        Assert.Equal("Enter a valid number, e.g. 1234.56.", error);
    }

    [Theory]
    [InlineData("-0.01")]
    [InlineData("-100")]
    public void TryParse_RejectsNegativeAmounts(string input)
    {
        var ok = ChequeAmountValidator.TryParse(input, out _, out _, out var error);

        Assert.False(ok);
        Assert.Equal("Cheque amount cannot be negative.", error);
    }

    [Fact]
    public void TryParse_RejectsAmountAboveMaxSupported()
    {
        var tooLarge = (ChequeAmountValidator.MaxSupportedAmount + 1).ToString("F2");

        var ok = ChequeAmountValidator.TryParse(tooLarge, out _, out _, out var error);

        Assert.False(ok);
        Assert.Contains("too large", error);
    }

    [Fact]
    public void TryParse_AcceptsAmountAtMaxSupported()
    {
        var ok = ChequeAmountValidator.TryParse(
            ChequeAmountValidator.MaxSupportedAmount.ToString("F2"), out var amount, out var wasRounded, out var error);

        Assert.True(ok);
        Assert.Null(error);
        Assert.Equal(ChequeAmountValidator.MaxSupportedAmount, amount);
        Assert.False(wasRounded);
    }

    [Theory]
    [InlineData("$1,234.56", 1234.56)]
    [InlineData(" 1234.56 ", 1234.56)]
    [InlineData("1,000,000", 1000000)]
    [InlineData("+42.00", 42.00)]
    public void TryParse_IsForgivingOfCommonFormatting(string input, double expected)
    {
        var ok = ChequeAmountValidator.TryParse(input, out var amount, out _, out var error);

        Assert.True(ok);
        Assert.Null(error);
        Assert.Equal((decimal)expected, amount);
    }

    [Theory]
    [InlineData("12.345", "12.35")] // midpoint rounds away from zero
    [InlineData("12.344", "12.34")] // rounds to nearest
    [InlineData("12.346", "12.35")]
    public void TryParse_RoundsToTwoDecimalPlacesAndFlagsIt(string input, string expectedRounded)
    {
        var ok = ChequeAmountValidator.TryParse(input, out var amount, out var wasRounded, out _);

        Assert.True(ok);
        Assert.Equal(decimal.Parse(expectedRounded), amount);
        Assert.True(wasRounded);
    }

    [Fact]
    public void TryParse_DoesNotFlagRoundingWhenInputAlreadyHasTwoDecimals()
    {
        var ok = ChequeAmountValidator.TryParse("12.30", out _, out var wasRounded, out _);

        Assert.True(ok);
        Assert.False(wasRounded);
    }
}

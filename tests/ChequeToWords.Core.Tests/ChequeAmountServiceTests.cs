using ChequeToWords.Core;
using Xunit;

namespace ChequeToWords.Core.Tests;

public class ChequeAmountServiceTests
{
    [Fact]
    public void Convert_ValidAmount_ReturnsSuccessWithWords()
    {
        var result = ChequeAmountService.Convert("1234.56");

        Assert.True(result.Success);
        Assert.Null(result.Error);
        Assert.Equal(
            "One thousand, two hundred and thirty-four dollars and fifty-six cents.",
            result.Words);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("-5")]
    public void Convert_InvalidInput_ReturnsFailureWithoutWords(string? input)
    {
        var result = ChequeAmountService.Convert(input);

        Assert.False(result.Success);
        Assert.Null(result.Words);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void Convert_RoundedInput_FlagsWasRounded()
    {
        var result = ChequeAmountService.Convert("12.345");

        Assert.True(result.Success);
        Assert.True(result.WasRounded);
        Assert.Equal("Twelve dollars and thirty-five cents.", result.Words);
    }
}

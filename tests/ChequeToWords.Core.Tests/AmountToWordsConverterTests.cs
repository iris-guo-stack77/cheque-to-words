using ChequeToWords.Core;
using Xunit;

namespace ChequeToWords.Core.Tests;

public class AmountToWordsConverterTests
{
    [Theory]
    // Adam's worked example
    [InlineData("1234.56", "One thousand, two hundred and thirty-four dollars and fifty-six cents.")]
    // zero
    [InlineData("0", "Zero dollars only.")]
    [InlineData("0.00", "Zero dollars only.")]
    // cents only, no dollars — and no "oh-five" reading
    [InlineData("0.05", "Zero dollars and five cents.")]
    [InlineData("0.50", "Zero dollars and fifty cents.")]
    // whole-dollar amounts: "only", no "and zero cents"
    [InlineData("100.00", "One hundred dollars only.")]
    [InlineData("21", "Twenty-one dollars only.")]
    // singular dollar / singular cent
    [InlineData("1.00", "One dollar only.")]
    [InlineData("1.01", "One dollar and one cent.")]
    [InlineData("2.01", "Two dollars and one cent.")]
    // teens are irregular, not "ten-three"
    [InlineData("13.00", "Thirteen dollars only.")]
    [InlineData("19.19", "Nineteen dollars and nineteen cents.")]
    // hundreds: internal "and"
    [InlineData("999.99", "Nine hundred and ninety-nine dollars and ninety-nine cents.")]
    // group boundary: comma when the trailing group has a hundreds digit
    [InlineData("1234.00", "One thousand, two hundred and thirty-four dollars only.")]
    // group boundary: "and" (not comma) when the trailing group is purely tens/ones
    [InlineData("1005.00", "One thousand and five dollars only.")]
    [InlineData("1050.00", "One thousand and fifty dollars only.")]
    // clean multiples of a scale word
    [InlineData("100000.00", "One hundred thousand dollars only.")]
    [InlineData("1000000.00", "One million dollars only.")]
    [InlineData("1000000.06", "One million dollars and six cents.")]
    [InlineData("1000000000.00", "One billion dollars only.")]
    // a middle group (thousands) can be entirely zero and gets skipped, not read as "zero thousand"
    [InlineData("1234000006.00", "One billion, two hundred and thirty-four million and six dollars only.")]
    // trailing zero in cents isn't read as a separate digit ("fifty", not "five-zero")
    [InlineData("12.50", "Twelve dollars and fifty cents.")]
    // upper bound this converter supports
    [InlineData(
        "999999999999.99",
        "Nine hundred and ninety-nine billion, nine hundred and ninety-nine million, nine hundred and ninety-nine thousand, nine hundred and ninety-nine dollars and ninety-nine cents."
    )]
    public void Convert_ProducesExpectedWords(string amount, string expected)
    {
        var result = AmountToWordsConverter.Convert(decimal.Parse(amount));

        Assert.Equal(expected, result);
    }
}

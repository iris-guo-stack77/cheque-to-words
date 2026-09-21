namespace ChequeToWords.Web.Models;

public class ChequeAmountViewModel
{
    public string? Amount { get; set; }
    public string? Words { get; set; }
    public string? Error { get; set; }
    public bool WasRounded { get; set; }
}

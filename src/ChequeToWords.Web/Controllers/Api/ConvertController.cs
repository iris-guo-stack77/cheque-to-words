using ChequeToWords.Core;
using Microsoft.AspNetCore.Mvc;

namespace ChequeToWords.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class ConvertController : ControllerBase
{
    public record ConvertRequest(string? Amount);

    public record ConvertResponse(bool Success, string? Words, string? Error, bool WasRounded);

    // Backs the live preview in site.js. No conversion/validation logic here —
    // it all lives in ChequeToWords.Core so it's covered by the same unit tests
    // that back the server-rendered fallback in HomeController.
    [HttpPost]
    public ActionResult<ConvertResponse> Post([FromBody] ConvertRequest request)
    {
        var result = ChequeAmountService.Convert(request.Amount);
        return Ok(new ConvertResponse(result.Success, result.Words, result.Error, result.WasRounded));
    }
}

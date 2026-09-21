using System.Diagnostics;
using ChequeToWords.Core;
using ChequeToWords.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChequeToWords.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View(new ChequeAmountViewModel());

    // Server-rendered fallback for when JavaScript is unavailable — the live
    // preview in site.js calls Api/ConvertController for the normal path.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(ChequeAmountViewModel model)
    {
        var result = ChequeAmountService.Convert(model.Amount);
        model.Words = result.Words;
        model.Error = result.Error;
        model.WasRounded = result.WasRounded;
        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

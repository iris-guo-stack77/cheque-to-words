> Draft only — not sent. Review and edit before replying to Adam.

**To:** Adam Clinckett
**Subject:** Re: Round 2 coding task — Cheque Amount to Words

Hi Adam,

Thanks for sending this through. Please find the completed solution attached as a
zip (`ChequeToWords_IrisGuo.zip`) — happy to share it as a GitHub link instead if
that's easier to review.

**What I built:** An ASP.NET Core MVC app (C#, .NET 10) that converts a cheque
amount into words — e.g. `1234.56` → *"One thousand, two hundred and thirty-four
dollars and fifty-six cents."* The conversion logic sits in its own class library,
independent of the web layer, with 48 automated tests (95.6% line coverage). The UI
shows the words live as you type, with validation for negative, non-numeric, and
out-of-range input.

**To run it:** `dotnet run` from `src/ChequeToWords.Web`, then open the URL it
prints.

**Testing and edge cases:** Covered in detail in `README.md` — zero amounts,
negative input, singular vs plural "dollar/cent", rounding beyond two decimal
places, very large amounts, and non-numeric input, among others.

I went with ASP.NET Core MVC rather than a desktop/WPF app since the role centres
on ASP.NET and C# web development.

On the AI-assisted side: I used Claude Code to help write this, with a small
project harness (`CLAUDE.md` for coding standards, a `verify.sh` script running
build + full test suite + style check) that every change had to pass before I
accepted it — plus manually exercising the edge cases through the running app.

Happy to walk through any part of the code or my reasoning in the next round.

Kind regards,
Iris

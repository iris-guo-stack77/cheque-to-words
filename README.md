# Cheque Amount to Words

A small ASP.NET Core MVC app that converts a cheque amount (e.g. `1234.56`) into
British/NZ English words (e.g. *"One thousand, two hundred and thirty-four dollars
and fifty-six cents."*), built for the BookingTimes round-2 take-home assessment.

## Running it

Requires the [.NET SDK](https://dotnet.microsoft.com/download) (built and tested on
.NET 10).

```bash
cd src/ChequeToWords.Web
dotnet run
```

Open the URL it prints (e.g. `http://localhost:5017`). Type an amount — the words
update live as you type.

To run the unit tests, or the full build+test+style check:

```bash
dotnet test                 # just the tests
./scripts/verify.sh         # build + test + style check, one command
```

## Architecture

```
src/ChequeToWords.Core/   Plain C# class library — the actual conversion logic.
                           No ASP.NET dependency, so it's independently unit tested.
src/ChequeToWords.Web/    ASP.NET Core MVC. Controllers only orchestrate:
                           parse request -> call Core -> shape response.
tests/...Core.Tests/      xUnit tests for the Core library (48 tests).
```

**Why ASP.NET Core MVC (Razor), not WPF/MVVM:** BookingTimes' JD lists ASP.NET/C#
and customer-facing web interfaces, and in the round-1 interview the interviewer
said the long-term plan is to have everything "running on Razor." MVC on ASP.NET
Core matches that direction directly, and runs cross-platform via `dotnet run` —
WPF/MVVM is Windows-only and wasn't an option to build or test on my machine.

**Why the conversion logic is hand-written, not a library call:** Packages like
Humanizer already have a `ToWords()` for this. I didn't use one here — this
exercise is specifically about the logic, so I implemented the grouping/rounding
algorithm myself rather than wrapping someone else's. Humanizer (or similar) would
be a reasonable choice for a production system where reinventing this isn't the
point.

## The conversion rules

- Numbers are grouped in 3s (thousand / million / billion). Within a group, the
  hundreds digit joins the tens/ones with **"and"**: `234` → *"two hundred and
  thirty-four"*.
- Between groups: a **comma** if the lower group has a hundreds digit (`1234` →
  *"one thousand, two hundred and thirty-four"*); **"and"** if the lower group is
  only tens/ones (`1005` → *"one thousand and five"*). This is the standard British/
  NZ long-form reading and is what produces Adam's exact example output.
- Whole-dollar amounts (no cents) are written as *"...dollars only."* rather than
  *"...and zero cents"* — this matches how amounts are conventionally written on an
  actual cheque.
- `decimal` is used throughout (never `double`/`float`) so currency values don't
  pick up binary floating-point rounding error.

## What I tested, and the edge cases I considered

All of the below are covered by automated xUnit tests (48 total —
`AmountToWordsConverterTests`, `ChequeAmountValidatorTests`,
`ChequeAmountServiceTests`), plus manual testing through the running UI and direct
API calls (`curl`).

| Case | Behaviour | Why it matters |
|---|---|---|
| Negative amount (`-12.50`) | Rejected with "Cheque amount cannot be negative." | A cheque can't be written for a negative amount |
| Zero (`0`, `0.00`) | "Zero dollars only." | Easy to get wrong if zero isn't special-cased |
| Cents only, no dollars (`0.05`) | "Zero dollars and five cents." | `05` must read as "five", not "oh-five" |
| Whole dollars, no cents (`100.00`) | "One hundred dollars only." | Deliberately doesn't say "and zero cents" — see above |
| Singular dollar/cent (`1.01`) | "One dollar and one cent." | Singular vs plural unit words |
| Teens (`13`, `19`) | "Thirteen", "Nineteen" | Can't be derived from "ten" + digit — they're irregular in English |
| Trailing zero in cents (`12.50`) | "...fifty cents", not "five-zero cents" | Cents is read as a two-digit number, not digit-by-digit |
| More than 2 decimal places (`12.345`) | Rounded to the nearest cent (`12.35`), UI shows a "rounded" note | A cheque amount can't have fractional cents |
| Non-numeric input (`"abc"`, empty) | Rejected with a clear message, no crash | Users will mistype |
| `$` and thousands separators (`"$1,234.56"`) | Parsed the same as `1234.56` | People naturally type amounts this way |
| Leading/trailing whitespace | Trimmed before parsing | Copy-pasted input often has this |
| Very large amount (over ~1 trillion) | Rejected with a "too large" message instead of crashing or producing garbled output | The word-grouping only goes up to billions |
| A middle group of zero, e.g. `1,234,000,006` | "One billion, two hundred and thirty-four million and six dollars only." | The thousands group is entirely zero and is skipped, not read as "zero thousand" |
| Scientific notation (`"1e5"`) | Rejected as an invalid number | Not a sensible way to write a cheque amount |

### Manually verified through the UI

- Typing Adam's example (`1234.56`) live-updates to the exact expected sentence.
- Negative, non-numeric, empty, and over-limit inputs all show a clear inline error
  and never throw an unhandled exception.
- The form still works with JavaScript disabled (full-page POST to the same
  `ChequeAmountService.Convert` — no logic duplicated between the two paths).

## AI-assisted development

Built with Claude Code assistance. Every change was checked by running
`scripts/verify.sh` (build + full test suite + `dotnet format` style check) and by
exercising the edge cases above through the running app and direct API calls —
not accepted on the assistant's say-so alone. See `CLAUDE.md` for the project's
coding standards.

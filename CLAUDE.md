# ChequeToWords — Coding Standards & Project Rules

Take-home coding assessment for BookingTimes (round 2 technical test). Converts a cheque
amount (e.g. `1234.56`) into words (e.g. "One thousand, two hundred and thirty-four
dollars and fifty-six cents.").

## Architecture rules

- `ChequeToWords.Core` must have **zero** ASP.NET/Web dependencies. It is a plain C#
  class library so the conversion logic can be unit tested in isolation and reused
  outside a web context.
- All business logic (number-to-words conversion, validation) lives in `Core`.
  `ChequeToWords.Web` controllers only orchestrate: parse the request, call `Core`,
  shape the response. No conversion/validation logic in controllers or views.
- `ChequeToWords.Web` → references `Core`. `ChequeToWords.Core.Tests` → references
  `Core`. `Core` references nothing else in this solution.

## Code rules

- Money is always `decimal`. Never `double`/`float` for amounts (avoids binary
  floating-point rounding errors on currency).
- Number-to-words follows **British/NZ English** conventions, not US:
  - "and" joins the hundreds digit to the tens/ones within a 3-digit group
    (`234` → "two hundred **and** thirty-four").
  - Comma joins group to group when the lower group has a hundreds digit; "and"
    joins group to group when the lower group is purely tens/ones with no hundreds
    digit (`1005` → "one thousand **and** five").
- No third-party number-to-words library (e.g. Humanizer) for the core algorithm —
  it's hand-written on purpose, to demonstrate the underlying logic for this
  assessment. Noted as a production alternative in the README, not used here.
- Nullable reference types enabled; treat nullable warnings as things to fix, not
  suppress.
- No comments explaining *what* code does — only *why*, when it's non-obvious
  (e.g. a rounding or formatting decision that isn't dictated by the spec).

## Testing rules

- Every edge case in the README's edge-case table must have a corresponding xUnit
  `[Theory]`/`[InlineData]` case in `AmountToWordsConverterTests`.
- Before considering any change done, run `scripts/verify.sh` and confirm it exits
  clean (build + tests + format check all pass).

## AI-assisted development

This project is being built with Claude Code assistance. Every AI-generated change
is checked by running `scripts/verify.sh` (build + full test suite + style check) and
by manually exercising the UI for the edge cases in the README before it's considered
accepted — not accepted on the model's say-so alone.

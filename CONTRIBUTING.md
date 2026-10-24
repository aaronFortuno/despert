# Contributing

Small, focused changes are welcome. Keep the everyday interface to one active/inactive switch. Describe the problem, what changes, and how you checked it.

## Translations

`src/Translations.cs` contains one entry per message. Each array is ordered **English, Catalan, Spanish, Basque, Galician**. `src/Strings.cs` maps Windows `CurrentUICulture` to a supported language and falls back to English.

When improving a translation, preserve `{0}` and `{1}` placeholders. An `&` in a menu label marks its keyboard shortcut; avoid duplicate shortcuts among the four visible actions, for both activation states. Keep menu text concise. Changes to the about text should keep Value for Value voluntary.

For another language, add the language code in `Strings.cs`, a value to every translation array, and coverage in `tests/LanguageChecks.cs`. Include regional-culture and English-fallback cases. Please mention whether a translation has had native-speaker review.

## Local checks

```powershell
.\prepare-toolchain.ps1
.\build.ps1 -Test
```

Automated checks must not send input, request power, register startup, open external links, or write to the clipboard. Language checks render the passive About dialog offscreen. Actual long idle tests, session startup, and visual scaling checks are manual.

Use C# 5 and .NET Framework 4.8. Keep dependencies out of the running application. Do not modify device policies or Explorer's icon preferences. Normal builds consume canonical ICO files; regenerate icons only when intentionally changing artwork.

See `AGENTS.md` for the remaining repository constraints. Contributions to original code and artwork are under MIT.

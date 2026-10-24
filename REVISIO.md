# Local review · Despert 1.0.0

24 October 2026. First public release prepared locally. The user confirmed the previous UI and more than two hours of successful idle operation with the retained mechanism. The new About dialog and languages have automated local coverage; final interactive checks on the managed device remain manual.

## Changes reviewed

Removed both informational dialogs and the tray-visibility menu item. Successful startup changes are silent; errors still appear. The four native menu actions are activation, Windows startup, About, and Exit.

About shows Aarón Fortuño, the repository, portfolio, MIT license and a voluntary Value for Value message. The Lightning address matches the author's existing public Pentagrama/Plec content. No QR generator, payment SDK, new runtime library, or network fetch is involved. Address copying and external links require a click. The dialog owns and disposes its icon and heading font; repeated activation reuses the open dialog.

English, Catalan, Spanish, Basque and Galician live in one translation table. Selection uses Windows display-language `CurrentUICulture`, independent of regional number/date preferences. Unknown and invariant cultures fall back to English. Menus, tooltips, About, and application-owned error messages use this table. Windows/.NET exception details may use the language supplied by the OS.

Kimi produced the initial translation table in one bounded `kimi-code/kimi-for-coding` call at low effort. It completed with three usage records in about 62 seconds: 5,555 uncached input tokens, 6,912 cached input tokens and 3,687 output tokens. Codex reviewed wording, corrected grammar and menu shortcuts, implemented the dialog and integration, and ran checks. This does not constitute native-speaker certification for every language; translation improvements are welcome.

`Logic.cs` is unchanged. Changes to `Native.cs` only localize error strings; power, input, idle timing and session behavior are retained. Startup remains opt-in, current-user-only, and inactive on every new process. No automatic registry migration or Explorer preference writes were added.

## Local validation

- 35 logic checks and 14 pure startup-command checks.
- INPUT and MENUITEMINFO binary contracts in 32- and 64-bit processes.
- 136 resource, native-menu, appearance and session-notification checks.
- 542 language and passive-dialog checks: complete translation arrays, matching placeholders, regional languages, English fallback, unique menu shortcuts, Enter/Escape, and offscreen renderings of all five languages.

Menus are constructed without displaying them; only the passive About dialog is rendered offscreen for visual review. Automated checks do not start the active app, inject input, request power, register startup, open links, or copy clipboard data. No remote application checks are performed.

The pinned Microsoft compiler/reference packages and canonical ICO assets produce deterministic builds. Packaging verifies a local rebuild. A second verification rebuilds the exported source from the complete ZIP in a different path and compares the executable byte for byte. ZIP bytes themselves are not claimed reproducible. Checksums do not replace trust in the distribution source or an author signature.

## Limits

The existing optional dark-menu integration uses guarded internal uxtheme exports, with a standard native-menu fallback. Actual themes, display scaling, startup and tray preference persistence depend on the Windows device and require manual checks. This release does not claim to override all managed-device policies.

Historical local reviews: [0.1](docs/REVISIO-0.1.0.md), [0.2](docs/REVISIO-0.2.0.md), [0.3](docs/REVISIO-0.3.0.md), [0.4](docs/REVISIO-0.4.0.md).

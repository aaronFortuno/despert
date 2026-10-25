# Privacy policy

Despert is a local, open-source Windows tray utility maintained by [Aarón Fortuño](https://github.com/aaronFortuno). It has no user accounts, telemetry, analytics or advertising.

- It does not collect or transmit personal data, read documents, or record keystrokes or pointer positions. It uses Windows session-idle timing and current mouse-button state to determine when an activity event may be sent. The dormant alternative movement mode can read the current pointer position, but no position is logged or transmitted.
- It reads Windows appearance preferences to draw the tray icon. If explicitly enabled, Windows startup stores only Despert's executable path in the current user's startup registry entry; disabling the option removes that entry.
- Repository, license and portfolio links open the default browser only after a click. Those websites then handle the browser request under their own privacy policies.
- The Lightning QR and public contribution address are embedded locally. Despert does not contact the wallet service, create an invoice, or initiate a payment. The copy button writes the public address to the clipboard only on request.
- Despert has no background network communication. Downloading releases and preparing build tools involve GitHub and Microsoft/NuGet respectively; those activities are separate from the running app.

For questions, use the [project's issue tracker](https://github.com/aaronFortuno/despert/issues). Issues are public: do not post personal data or private school documents.

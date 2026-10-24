# Local review · Despert 1.0.1

24 October 2026. This patch adds the author's existing Lightning QR and documents the Windows publisher warning. It **does not resolve Authenticode signing**; the executable remains unsigned. No remote application tests were performed.

## QR implementation and provenance

The SVG is copied unchanged from the author's existing Pentagrama/Plec QR. A narrowly scoped local build tool converts its square-cell path to a canonical 45 × 45 module matrix. Normal builds embed the matrix; the app draws it using integer-sized black/white squares, retaining the four-module white quiet zone. There is no SVG renderer, barcode library, payment SDK or network request in the app. The address and copy button remain available beside the QR.

The QR's accessible description is localized into all five interface languages. The existing layout is retained and enlarged vertically for the QR. `Logic.cs`, `Native.cs`, startup, input and power behavior are unchanged from 1.0.0.

## Local validation

- 35 logic checks, 14 pure startup-command checks, and INPUT/MENUITEMINFO binary contracts in 32/64 bits.
- 136 resource, native-menu, appearance and session checks.
- 558 language/dialog/QR checks, including exact black/white module rendering and quiet-zone preservation at 135, 180, 225 and 270 pixels; the passive About dialog is rendered offscreen in all five languages.
- An independent local ZXing-C++ 2.3.0 development-only reader decoded the four standalone QR images and all five About renders. All nine contained the original LNURL. Its Bech32 checksum was verified, and the decoded endpoint was `https://walletofsatoshi.com/.well-known/lnurlp/aptswim903`, matching the displayed address. The endpoint was not contacted.

The optional development-only QR decoder is not included in release packages and is not required for normal builds/tests. Automated checks do not start the active application, inject input, request power, register startup, open links, copy clipboard data or initiate a payment. Actual UI use and sleep behavior remain manual checks.

Normal packaging verifies a deterministic rebuild. The exported source from the complete ZIP is also prepared and rebuilt in another local path and compared byte for byte with the distributed unsigned executable.

## Publisher investigation

`Get-AuthenticodeSignature` reports `NotSigned` for 1.0.0. No available code-signing certificate was found in the current-user certificate store in the checked environment. Friendly product/company file metadata was added in 1.0.1; this does not establish a trusted Windows publisher.

[Signing options and their prerequisites](docs/SIGNING.md) are documented using Microsoft and SignPath primary sources. No certificate was purchased, no signing application was submitted, and no third-party approval is claimed. No security setting or trust store was modified. Future Authenticode signing will require adapting the current exact-byte verification: signed distribution files and the deterministic unsigned build must have separate hashes.

The guarded internal uxtheme dark-menu integration and manual device compatibility limits remain as described in the [1.0.0 review](docs/REVISIO-1.0.0.md).

# Windows publisher and SmartScreen status

Checked on 24 October 2026. **Despert 1.0.0 and 1.0.1 are not Authenticode-signed.** Windows can show an unknown publisher or SmartScreen reputation warning when running a downloaded copy. Version 1.0.1 adds a Lightning QR; it does not resolve the signing warning.

An unknown publisher is consistent with a missing trusted digital signature; it is different from a named malware detection. That distinction is not a safety verdict. If Microsoft reports a specific threat name, investigate that detection separately.

The author's name in file properties, the open-source license, GitHub hosting and SHA-256 manifests do not establish a Windows-trusted publisher. They provide other useful information, but cannot replace Authenticode signing. The friendly product/company metadata in 1.0.1 is not a digital signature.

## Practical signing options

| Path | Fit for Despert | What remains |
| --- | --- | --- |
| SignPath Foundation | Free code signing for qualifying open-source projects; can preserve portable distribution | Application review, project acceptance, signing policy, source-to-artifact build integration and release approval. Foundation certificates identify SignPath Foundation as publisher. |
| CA-issued code-signing certificate | Can sign the portable executable with a validated identity | A paid service, identity validation and protected signing keys. Vendor eligibility and terms must be checked before purchase. |
| Microsoft Store with MSIX | Store signs an accepted MSIX package | A different packaged distribution, developer account, Store submission and acceptance; does not retroactively sign GitHub's portable executable. |

SignPath is being evaluated for this MIT project, but eligibility is **not assumed**. Its conditions also exclude tools that circumvent security measures. Because Despert can affect managed idle behavior, the project must describe its actual operation and let the Foundation assess that condition. The application is being prepared and is pending submission; it has not been accepted and no signing rights are claimed. See the [Code signing policy](CODE_SIGNING_POLICY.md), [privacy policy](PRIVACY.md) and [manual build workflow](GITHUB_ACTIONS.md).

Microsoft's current documentation limits Azure Artifact Signing identity validation for individuals to the USA and Canada; EU availability applies to organizations. It should not be presented as immediately available to a Spain-based individual without checking eligibility.

A trusted signature supplies a verified publisher identity. **It does not guarantee immediate SmartScreen reputation for a new binary.** EV certificates no longer guarantee an instant bypass either. A self-signed certificate is not a public-distribution solution, because other users' Windows installations do not trust it by default.

No Defender setting, trust store, download security marking or device policy is modified by Despert's packaging.

## Before a signed release

1. Select and obtain an approved signing service/certificate; do not publish an assertion of sponsorship or acceptance before approval.
2. Keep an immutable unsigned, deterministic build and its input manifest.
3. Sign a copy using Authenticode and a trusted timestamp, then validate the signature and publisher.
4. Record and publish separate unsigned-build and signed-distribution hashes. Signing changes the executable bytes; timestamps and external signatures are not reproduced by an ordinary source build. The current byte-for-byte verification intentionally compares unsigned releases and must be adapted before shipping signed ones.
5. Recreate archives and checksums from the final artifact and document the chosen trust chain.

Sources: [Microsoft SmartScreen reputation](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/smartscreen-reputation), [Microsoft signing options](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/code-signing-options), [SignPath Foundation](https://signpath.org/), [Foundation conditions](https://signpath.org/terms).

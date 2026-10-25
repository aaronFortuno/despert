# Code signing policy

**Status: SignPath Foundation application pending submission.** The maintainer is preparing the application. It has not yet been submitted or accepted, and no SignPath subscription, certificate use or signing integration has been approved. All currently distributed Despert executables remain unsigned.

## Proposed service and certificate

The proposed signing service is [SignPath.io](https://about.signpath.io/), with a certificate held by [SignPath Foundation](https://signpath.org/), subject to the Foundation's eligibility review and acceptance. This page does not claim that SignPath currently signs or endorses Despert. The required service/certificate attribution will be updated after approval, before any signed release is advertised.

## People and responsibilities

- Committer and reviewer: [Aarón Fortuño](https://github.com/aaronFortuno).
- Release and proposed signing approver: [Aarón Fortuño](https://github.com/aaronFortuno).

Changes from other contributors are to be reviewed by the maintainer. Signing requests, if enabled after acceptance, will require explicit approval by the maintainer. Multi-factor authentication must be enabled on GitHub and SignPath before signing is activated; this repository does not claim to have verified account settings.

## Build and release process

The [manual GitHub Actions workflow](https://github.com/aaronFortuno/despert/actions/workflows/build.yml) prepares pinned Microsoft tools, builds the app, runs passive checks, packages it, and verifies a rebuild from exported source in another directory. Actions are pinned to full commit identifiers. The workflow only has repository read permission, does not retain checkout credentials and does not publish a release or sign a file.

The maintainer explicitly authorized the first remote execution. The [hosted run for commit 24ddad8](https://github.com/aaronFortuno/despert/actions/runs/40169235981) completed successfully on 25 October 2026, including tests, packaging and exported-source verification. Its artifact was downloaded and its executable matched the published local 1.0.1 executable byte for byte. Artifacts identify the source commit and workflow run and are labelled unsigned. This successful build does not establish SignPath acceptance or signing integration.

Current release verification compares the unsigned executable byte for byte. Before signing is enabled, the process must retain unsigned build hashes, validate Authenticode/timestamps and publish separate signed-distribution hashes. No signed release will be represented as identical byte for byte to a normal unsigned rebuild.

## Privacy

Despert does not collect analytics, personal data, document contents or typed text, and does not make background network requests. Links in About only open on explicit user request; the Lightning QR is embedded locally. Address copying only writes to the clipboard when requested. See the [privacy policy](PRIVACY.md).

## Eligibility and accurate description

The application describes Windows power requests and zero-displacement mouse events after inactivity. These may affect idle-triggered sleep or automatic locking on managed devices. Despert does not unlock a locked session or change device policies. Whether this behavior fits the Foundation's conditions is for SignPath to assess. The project is newly published; established public adoption or reputation is not claimed.

[Foundation conditions](https://signpath.org/terms) · [Publisher warning and signing options](SIGNING.md)

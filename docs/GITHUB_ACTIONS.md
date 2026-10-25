# Manual GitHub Actions build

The workflow is deliberately manual. Pushing source or a tag does not launch it. No signing secrets, SignPath approval or automatic release publication are configured.

To run the workflow from the GitHub interface:

1. Open [Actions → Build Despert (manual)](https://github.com/aaronFortuno/despert/actions/workflows/build.yml).
2. Select **Run workflow**, choose **main**, then **Run workflow** again.
3. Check the run's build, packaging and exported-source verification steps. Local validation does not prove a hosted run has succeeded.
4. Download the **Despert-unsigned-…** artifact. It contains the versioned executable, build manifest, three release archives, checksums and `CI-PROVENANCE.json` identifying the source commit/run. All executables remain unsigned.
5. Retain the successful run URL as evidence for the SignPath application. Report a failure rather than describing an untested workflow as a successful build integration.

The Windows runner uses the same pinned compiler and reference packages as local builds. Normal tests do not inject input, request power, register startup, open links or change the clipboard; UI checks render passive controls offscreen. The maintainer has also authorized an agent to dispatch and inspect this build workflow; this does not replace manual visual checks of the actual app or Pages publications.

The equivalent local commands, from a fresh checkout with one current complete archive in `dist/`, are:

```powershell
.\prepare-toolchain.ps1
.\build.ps1 -Test
.\package.ps1 -SkipBuild
.\tools\VerifyExport.ps1
```

`VerifyExport.ps1` uses `.tools/downloads` as the existing package cache; a different verified cache can be supplied with `-PackageCache`. It leaves the exported source/rebuild under `build/` for inspection. The published 1.0.1 tag and its downloads are not replaced when this workflow/documentation is added to `main`.

After SignPath accepts the application, source-linked signing, credentials, manual signing approvals and final signed/unsigned hash handling must be configured separately.

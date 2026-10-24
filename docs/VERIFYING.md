# Rebuild and verify an executable

You can use Despert without compiling it. The complete release archive contains matching `portable/` and `source/` folders for those who want to audit and rebuild it.

## Check a download

```powershell
Get-FileHash .\Despert-1.0.0-complete.zip -Algorithm SHA256
```

Compare with the corresponding line in `Despert-1.0.0-SHA256SUMS.txt` from the project's release. The portable folder has its own `SHA256SUMS.txt`. SHA-256 compares content; obtain the manifest from a trusted source. It is not a developer signature, and MD5 is not used.

## Prepare tools

Requires Windows, PowerShell 5.1 or later, and .NET Framework 4.8. From the extracted `source/` folder:

```powershell
.\prepare-toolchain.ps1
```

This downloads official Microsoft NuGet packages pinned in `toolchain.json`: Microsoft.Net.Compilers.Toolset 4.14.0 and Microsoft.NETFramework.ReferenceAssemblies.net48 1.0.3. It verifies their fixed SHA-256 values before extraction into `.tools/`. No administrator rights, Visual Studio or .NET SDK are required. Nothing is installed system-wide. The app itself does not need network access.

For an offline package cache, pass `-PackageCache 'C:\path\to\packages'`; expected filenames are `compiler.nupkg` and `references.nupkg`. Do not change organizational script-execution policies to run the build.

## Rebuild and compare

```powershell
.\verify-build.ps1 -ReleaseDirectory ..\portable
```

The script validates manifests and source/resource inputs, rebuilds in a fresh folder under `build/`, and compares the executable's SHA-256. It does not execute Despert. Expected success text begins **VERIFICAT: executable identic byte per byte**.

`BUILDINFO.json` identifies the effective compiler, reference libraries, source, resources and output hashes. Deterministic compilation uses explicit references, stable input ordering, path normalization, LF line endings enforced by `.gitattributes`, and canonical ICO resources. It does not embed machine paths or variable build dates. Icon files are not rerendered in normal builds because GDI/PNG encoders can differ between environments.

Exact identity applies to `Despert.exe`, not ZIP bytes. Different sources, resources or tools can legitimately change the result; inspect the manifest rather than treating every mismatch as evidence of malice.

## Tests and packages

```powershell
.\build.ps1 -Test
.\package.ps1 -SkipBuild
```

The app is built at `dist/1.0.0/Despert.exe`. Tests cover logic, startup command construction, Win32 structures, resources, menus, session messages, translations and passive About layout. They do not inject input, request power, change startup, open links, or write clipboard data. Interactive UI and device idle behavior are checked manually.

Packaging verifies a fresh rebuild and exports portable, source and complete archives with SHA-256 manifests. It does not publish to GitHub. The development-only `-Legacy` build is not guaranteed reproducible and is unsuitable for release verification.

[Microsoft's deterministic compilation documentation](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/code-generation#deterministic).

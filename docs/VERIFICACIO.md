# Compilar i verificar Despert

El paquet complet conté `portable/` amb l'executable i `source/` amb tot el projecte. No cal compilar res per utilitzar Despert. Per auditar-lo i comprovar-ne la correspondència amb les fonts, segueix aquests passos.

## 1. Verificar una descàrrega

Les versions inclouen un fitxer **SHA256SUMS.txt**. A PowerShell:

```powershell
Get-FileHash .\Despert.exe -Algorithm SHA256
```

Compara el resultat amb la línia de `Despert.exe` del manifest publicat amb la mateixa versió. Per als ZIP, utilitza el manifest `Despert-1.0.1-SHA256SUMS.txt`.

SHA-256 verifica si dos fitxers tenen el mateix contingut. No substitueix una font de confiança per obtenir el manifest: convé consultar-lo a la publicació oficial del projecte. No és una signatura del desenvolupador. No s'utilitza MD5.

## 2. Preparar eines amb versions fixes

Requisits: Windows, PowerShell 5.1 o posterior i .NET Framework 4.8. No cal instal·lar Visual Studio, el SDK .NET ni tenir permisos d'administrador.

Dins de `source/`, executa:

```powershell
.\prepare-toolchain.ps1
```

Es descarreguen dos paquets oficials, amb versions i SHA-256 fixats a `toolchain.json`:

- [Microsoft.Net.Compilers.Toolset 4.14.0](https://www.nuget.org/packages/Microsoft.Net.Compilers.Toolset/4.14.0): compilador Roslyn per a .NET Framework.
- [Microsoft.NETFramework.ReferenceAssemblies.net48 1.0.3](https://www.nuget.org/packages/Microsoft.NETFramework.ReferenceAssemblies.net48/1.0.3): biblioteques de referència per compilar.

Es verifica cada paquet abans d'extreure'l a `.tools/`. No s'instal·la res al sistema i aquestes eines no es distribueixen com si fossin codi propi del projecte. La xarxa només és necessària per a aquesta preparació. Si ja tens els dos arxius en una memòria local, pots indicar `-PackageCache 'C:\ruta\als\paquets'`; els noms esperats són `compiler.nupkg` i `references.nupkg`.

No canviïs la política d'execució imposada per l'organització. Els scripts són llegibles i les ordres de compilació es poden inspeccionar o executar en un entorn de desenvolupament autoritzat.

## 3. Recompilar i comparar amb el paquet portable

Des de `source/`:

```powershell
.\verify-build.ps1 -ReleaseDirectory ..\portable
```

La comprovació:

1. Verifica el SHA-256 de l'executable i dels fitxers del paquet contra els manifests.
2. Comprova que cada font i recurs de compilació coincideixi amb `BUILDINFO.json`.
3. Recompila en una carpeta nova dins de `build/`, sense executar Despert.
4. Compara el SHA-256 del resultat amb el de l'executable distribuït.

Si tot coincideix mostra **VERIFICAT: executable identic byte per byte**. La compilació de prova es conserva a la ruta indicada per poder-la inspeccionar.

El procés utilitza compilació determinista, ordena els fitxers d'entrada, fixa versions de compilador i referències, normalitza les rutes i consumeix els ICO canònics del repositori. `BUILDINFO.json` registra els hashes d'entrades, eines, biblioteques i resultat. No conté credencials, rutes de l'usuari ni dates variables de compilació.

Els ICO no es regeneren en una compilació normal: el motor gràfic i la codificació PNG de cada Windows podrien produir bytes diferents. Són recursos de font inclosos, amb art vectorial original a `assets/` i el generador corresponent a `tools/BuildIcons.cs`. Si vols modificar l'art, `build.ps1 -RegenerateIcons` ho permet; això canviarà legítimament el hash de l'executable.

La coincidència exacta es refereix a **Despert.exe**, no als ZIP: els contenidors poden variar per dates dels fitxers o per la biblioteca de compressió. Si canvies fonts, recursos o eines, el resultat pot deixar de coincidir. Un hash diferent no prova, per si sol, que el codi sigui maliciós: primer cal contrastar les entrades a `BUILDINFO.json`.

## Compilar i executar les proves

```powershell
.\build.ps1 -Test
```

La sortida és `dist/1.0.1/Despert.exe`. Les proves comproven la lògica, les estructures Win32, les icones, les entrades i marques del menú, la construcció de les rutes d'inici i els avisos de sessió. No generen moviments de ratolí ni peticions d'energia, no registren l'aplicació a l'inici i no obren Configuració. Les proves visuals, d'inici de sessió i de repòs a l'equip real són manuals.

L'opció de desenvolupament `-Legacy` permet usar el compilador antic de Windows, però el resultat es marca com a no determinista i no serveix per a aquesta comprovació de la publicació.

## Preparar els paquets públics

```powershell
.\package.ps1
```

Compila, executa les proves, verifica una recompilació i genera els ZIP portable, source i complete, amb els seus manifests SHA-256. No publica ni envia res a GitHub.

La compilació determinista de C# està documentada per [Microsoft](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/code-generation#deterministic).

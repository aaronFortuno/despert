# Despert

Una petita utilitat portable per a Windows per mantenir l'ordinador despert amb un clic. Codi obert i art original sota MIT. Creat per Aarón Fortuño.

[Descarrega la darrera versió](https://github.com/aaronFortuno/despert/releases/latest) · [Documentació completa en anglès](../README.md) · [Portfoli de l'autor](https://aaronfortuno.github.io/portfolio/)

## Ús

Extreu el paquet portable i obre **Despert.exe**. Necessita Windows 10/11 amb .NET Framework 4.8. No necessita instal·lació ni permisos d'administrador. Si ja tens una versió oberta, tanca-la des de **Surt** abans d'obrir la nova.

Un clic a la tassa alterna actiu/desactiu: **amb fum està actiu; sense fum està desactivat**. Comença sempre desactivat, també quan s'obre amb Windows. Un cop activat continua indefinidament fins que el desactives o surts. Arrossega la tassa des del desplegable **^** fins al costat del rellotge si la vols visible.

Amb el botó dret tens **Activar/Desactivar**, **Iniciar amb Windows**, **Sobre…** i **Surt**. L'inici amb Windows és opcional, per al teu usuari i desmarcat per defecte. Deixa el fitxer en una ubicació fixa abans de marcar-ho. Si el mous, desmarca i torna a marcar l'opció des de la nova ubicació. Desmarca-la abans d'eliminar el programa. **Surt** només tanca l'execució actual.

El diàleg **Sobre…** mostra versió, autor, repositori, portfoli, llicència MIT i un petit apartat V4V. Compartir, traduir, aportar idees o fer una contribució voluntària són maneres de retornar valor. L'adreça Lightning és **aptswim903@walletofsatoshi.com**, amb botó per copiar-la. Totes les funcions continuen sent gratuïtes.

## Idiomes i funcionament

La interfície segueix l'idioma de visualització de Windows: català, castellà, euskera, gallec i anglès. Les variants regionals utilitzen la llengua corresponent; per a la resta, anglès.

Quan està actiu, demana a Windows mantenir el sistema i la pantalla actius. Després de 45 segons d'inactivitat de sessió envia un esdeveniment de ratolí sense desplaçament. L'entrada recent ajorna els impulsos i no s'envien mentre hi ha un botó del ratolí premut. No envia tecles, clics ni roda.

Bloquejar, suspendre o desconnectar la sessió pausa els efectes; reprendre o desbloquejar els recupera si continuava activat. No canvia polítiques d'energia o de gestió. No té anuncis, comptes, telemetria, serveis ni drivers; funciona sense connexió. Només obre els enllaços de Sobre després d'un clic i només copia l'adreça quan ho demanes.

## Fonts i verificació

El paquet **complete.zip** inclou `portable/` i `source/` de la mateixa versió. Des de `source/`, amb PowerShell:

```powershell
.\prepare-toolchain.ps1
.\verify-build.ps1 -ReleaseDirectory ..\portable
```

La preparació descarrega eines oficials amb versions i hashes fixats, sense instal·lar-les al sistema. La verificació recompila sense executar l'aplicació i compara l'executable byte per byte. No cal Visual Studio ni el SDK .NET. Els manifests són SHA-256, no MD5. Una coincidència no equival a una signatura de l'autor.

L'usuari ha confirmat més de dues hores sense repòs automàtic a l'equip gestionat amb el mecanisme que conserva aquesta versió. Les comprovacions locals i els límits consten a la [revisió](../REVISIO.md). La compatibilitat concreta amb cada equip s'ha de comprovar al dispositiu.

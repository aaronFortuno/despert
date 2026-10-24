# Revisió · Despert 0.3.0

24 d'octubre de 2026. Estat: implementació i comprovacions locals completades; nova aparença pendent de prova de l'usuari. No s'ha publicat cap versió remota.

## Evidència d'ús

L'usuari confirma més de dues hores sense interacció i amb el resultat esperat a l'equip Windows gestionat. La confirmació correspon a la versió 0.2.0. Logic.cs i Native.cs, que contenen el mecanisme d'inactivitat i energia, es conserven en aquesta iteració.

## Aparença

Art original MIT amb tassa de costat, sense farciment de color, perspectiva, plat ni detalls de cafè. Dues corbes llargues identifiquen l'estat actiu. Hi ha recursos de 12/16/20/24/32/40/48/64/128/256 píxels i variants de tinta negra i blanca. El tema es consulta en lectura i es refresca amb els avisos de tema i mida de Windows. En alt contrast es prioritza el color del text del sistema.

Els marcs petits dels ICO utilitzen DIB BGRA amb màscara de transparència. La prova va detectar que System.Drawing.Icon de .NET Framework podia interpretar incorrectament els marcs petits codificats només en PNG. Després de canviar el format, passen les comprovacions de càrrega, monocromia i fum a 12/16 píxels. El marc de 256 píxels es conserva en PNG i es valida descodificant-lo directament.

## Menú

HMENU amb CreatePopupMenu, InsertMenuItemW i TrackPopupMenuEx: Windows dibuixa fonts, marges i navegació. Només té Activar/Desactivar i Surt. Els errors del menú no aturen una sessió activa.

Per al tema fosc s'utilitzen opcionalment exports interns de uxtheme, limitats a versions compatibles, carregats de System32 i consultats dinàmicament. No són API documentades de Windows. Si no estan disponibles, funciona el menú nadiu estàndard; no es garanteix aparença fosca en qualsevol versió. No es forcen en alt contrast. La comprovació local confirma que els exports opcionals estan disponibles en aquest Windows; l'aspecte real el validarà l'usuari.

## Kimi i correccions

Un encàrrec limitat a Program.cs i NativeMenu.cs amb kimi-code/kimi-for-coding, esforç baix. Ha acabat amb informe final en 225 segons aproximadament. Registres: 6 peticions amb ús, 18.498 tokens d'entrada sense caché, 41.728 de caché i 12.016 de sortida. No hi ha lectures inicial/final de quota d'aquesta iteració ni conversió verificada a euros. No s'han fet inferències de correcció.

Codex ha corregit el lookup d'exports per ordinal (punter enter, no cadena amb #), el sentit del color de fallback i l'alt contrast, la recàrrega de mida sense canvi de tema i les convencions explícites dels delegates nadius. Ha preparat l'art, els recursos i tota la compilació/verificació. Els canvis de Kimi necessitaven revisió dels contractes Win32; no es considera una prova d'estalvi només perquè hagi acabat.

## Compilació i verificació

- Compilador oficial Microsoft.Net.Compilers.Toolset 4.14.0 i biblioteques de referència net48 1.0.3, preparats localment amb SHA-256 de paquet fixat.
- Compilació determinista amb /noconfig, referències explícites, ordre de fonts estable, /pathmap, llenguatge C# 5 i sense símbols de depuració variables.
- Els ICO canònics del repositori són entrades de compilació; no es tornen a renderitzar en cada compilació.
- BUILDINFO.json registra el compilador, les dependències, les referències, les fonts, els recursos i el SHA-256 del resultat, sense dades personals.
- verify-build.ps1 comprova la integritat i recompila en una carpeta nova. La verificació final es fa també des de les fonts del paquet complet, extretes en una ruta diferent.

La coincidència binària es refereix a l'executable, no al contenidor ZIP. No és una signatura d'autor. Les eines es descarreguen de NuGet oficial quan l'usuari prepara la compilació; l'aplicació no fa peticions de xarxa.

## Proves locals

35 assertions de lògica; INPUT i MENUITEMINFO en processos de 32 i 64 bits; 72 assertions de recursos i receptor de sessió. Inclouen diferenciació de fum i monocromia a 12/16 píxels en els dos temes, càrrega de les quatre variants, text Unicode i dues entrades del menú nadiu, extracció de la icona de l'executable i combinacions de bloqueig/suspensió/represa.

Les proves creen menús sense mostrar-los i invoquen el receptor de sessió directament. No executen el context actiu de Despert ni generen moviments, bloqueig real o peticions d'energia. No s'ha fet comprovació remota. La prova visual del nou menú i escalat, i la prova d'ús del nou executable, les fa l'usuari.

Historial: [0.1.0](docs/REVISIO-0.1.0.md), [0.2.0](docs/REVISIO-0.2.0.md).

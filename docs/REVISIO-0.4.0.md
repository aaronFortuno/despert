# Revisió · Despert 0.4.0

24 d'octubre de 2026. Implementació i comprovacions locals completades. Pendent de prova manual del menú, inici de sessió i preferència de visibilitat. No s'ha publicat en remot.

## Canvi d'aquesta versió

Inici opcional amb Windows al menú contextual, amb marca nativa. Inicialment no hi ha cap entrada configurada per Despert. L'aplicació només escriu o elimina el seu valor `Despert` a `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` en resposta al clic explícit de l'usuari. L'ordre conté la ruta completa entre cometes i es desa com a REG_SZ. No hi ha escriptures automàtiques en executar, actualitzar o compilar l'aplicació.

La marca representa una entrada configurada, no que Windows garanteixi executar-la: es respecta la decisió d'Aplicacions d'inici i de les polítiques de l'equip. No es modifica StartupApproved, HKLM, Explorer ni cap política. Cada procés nou comença desactivat; no es desa la intenció activa entre execucions. La ruta no es trasllada silenciosament: després de moure l'executable, cal desmarcar i tornar a marcar l'opció des de la nova ubicació.

L'acció **Mostrar la tassa a la barra…** explica com arrossegar-la al costat del rellotge i obre la pàgina documentada `ms-settings:taskbar`. La visibilitat final la tria l'usuari a Windows. No s'ofereix una marca fictícia que prometi forçar-la ni es manipula la memòria de preferències d'Explorer. Conservar la ubicació de l'executable ajuda a mantenir l'elecció, sense garantia universal en tots els Windows gestionats.

Fonts primàries: [àrea de notificació](https://learn.microsoft.com/en-us/windows/win32/shell/notification-area), [personalització de la barra](https://support.microsoft.com/en-us/windows/experience/personalization/customize-the-taskbar-in-windows), [Run](https://learn.microsoft.com/en-us/windows/win32/setupapi/run-and-runonce-registry-keys), [pàgines de configuració](https://learn.microsoft.com/en-us/windows/apps/develop/launch/launch-settings).

## Revisió del codi

Codex ha completat la connexió entre menú i accions, ha rebutjat rutes relatives a una unitat (`C:Despert.exe`) o a l'arrel (`\Despert.exe`), ha mantingut les cometes i ha limitat l'ordre a 260 caràcters abans i després de normalitzar-la. Es rebutgen cometes incrustades, CR/LF i NUL. Els errors d'accés es comuniquen sense aturar el motor. La retirada d'inici elimina només el valor propi, mai tota la clau.

El menú té quatre accions i dos separadors. La marca i la disponibilitat de la preferència es construeixen amb estats nadius; si la lectura falla, les altres accions continuen disponibles. El muntatge real del menú es comparteix amb la comprovació automàtica, que no l'arriba a mostrar. Els recursos de text i HMENU es netegen també en excepció. Es conserva la integració opcional del tema fosc i les seves limitacions descrites a la revisió anterior.

`Logic.cs` i `Native.cs` coincideixen per SHA-256 amb la versió 0.3.0. Es conserva el mecanisme amb què l'usuari va confirmar més de dues hores sense repòs automàtic en la versió 0.2.0. Aquesta evidència és d'aquell equip, no de tots els dispositius.

## Kimi

Una única crida a `kimi-code/kimi-for-coding`, esforç baix, límit de vuit passos i sense inferències de correcció. Va deixar el helper d'inici i els canvis de menú escrits, però va esgotar els passos abans de connectar Program.cs o produir l'informe final. No es considera una implementació acabada ni una prova superada. Codex ha fet la integració pendent i la revisió.

El registre local atribueix vuit peticions amb ús: 14.200 tokens d'entrada sense caché, 69.120 de caché i 3.107 de sortida, en uns 66 segons. No hi ha lectures de quota abans/després ni conversió a euros. Aquest resultat indica que el límit de passos era massa curt per a lectures i canvis en tres fitxers; cal valorar l'abast o els passos per encàrrec, a més del model i els tokens de sortida.

## Validació local i paquet

- 35 comprovacions de lògica.
- 14 comprovacions pures de construcció de l'ordre d'inici: espais, accents, UNC, percentatges literals, rutes relatives, injecció d'arguments i límits de longitud.
- Estructures INPUT i MENUITEMINFO en processos de 32 i 64 bits.
- 136 comprovacions de recursos, tema, menú i avisos de sessió. Inclouen les vuit combinacions d'estat actiu, inici configurat i disponibilitat de lectura del menú real.

Les proves no registren Despert a l'inici, no obren Configuració, no executen el context actiu ni generen entrada o peticions d'energia. La prova real d'escriptura/retirada d'inici, reinici i persistència de la tassa és manual de l'usuari. No s'ha canviat la configuració d'inici de l'equip durant aquesta revisió.

Es conserva el compilador oficial fixat, els recursos canònics i la compilació determinista. `BUILDINFO.json` i els manifests SHA-256 acompanyen els paquets portable, fonts i complet. La verificació recompila sense executar l'aplicació i compara els bytes del resultat, també des de les fonts extretes del paquet complet en una altra ruta. La igualtat s'aplica a l'executable, no al ZIP, i no equival a una signatura de l'autor.

Historial: [0.1.0](docs/REVISIO-0.1.0.md), [0.2.0](docs/REVISIO-0.2.0.md), [0.3.0](docs/REVISIO-0.3.0.md).

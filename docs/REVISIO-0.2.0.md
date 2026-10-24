# Revisió · Despert 0.2.0

24 d'octubre de 2026. Estat: compilació i verificació local completades; pendent de prova visual i de repòs de l'usuari. Historial inicial: [0.1.0](docs/REVISIO-0.1.0.md).

## Canvi de producte

- Només Activar/Desactivar i Surt. Un clic esquerre alterna estat; botó dret obre el menú.
- Activació indefinida, sense límit de durada, opcions de simulació, interval configurable, diagnòstic ni restricció d'alimentació.
- Interval intern fix de 45 segons, amb el mateix mecanisme ZeroMove de la primera versió per poder comparar efectivitat.
- Tassa sense fum en estat inactiu; tassa fumejant en estat actiu. Art vectorial original, sense dependències externes, amb nou mides ICO de 16 a 256 píxels.
- Recursos incrustats per a la safata i icona Win32 per a l'Explorador. L'executable es pot copiar sol: no necessita carregar les icones des de fitxers externs.
- En bloquejar, suspendre o desconnectar la sessió, pausa els efectes sense canviar la intenció de l'interruptor. Reprèn només si encara estava activat. En desactivar, sortir, error o final de sessió, allibera la petició d'energia.

Es conserva la versió anterior a dist/Despert.exe. La nova és dist/0.2.0/Despert.exe: no se sobreescriu un executable que l'usuari pugui tenir obert. Cal sortir de la versió anterior abans d'obrir-ne una altra perquè comparteixen el control d'instància única.

## Resultat de Kimi i revisió

Un únic encàrrec, limitat a Program.cs, amb l'àlies kimi-code/kimi-for-coding i esforç baix. Ha acabat amb codi de sortida 0 i informe final en aproximadament 71 segons. El lliurament compilava. Ha fet una lectura inicial en una ruta incorrecta, una lectura del fitxer correcte, una escriptura i una edició; no ha modificat altres fitxers.

Codex ha corregit dos punts de la lògica de pausa: la constant de represa automàtica és 18 decimal (0x12), i activar durant una pausa no ha de mantenir una petició d'energia transitòria ni confondre l'estat de pausa amb la intenció de l'usuari. S'ha eliminat un flag redundant. No s'han fet més crides de correcció a Kimi.

Codex ha preparat l'art, el generador d'icones, la incrustació de recursos, la documentació i les comprovacions locals. Les fonts d'aquests elements són al projecte i tenen la mateixa llicència MIT.

## Comprovacions locals executades

- Compilació C# 5 amb avisos tractats com a errors, sense instal·lar paquets ni descarregar dependències.
- 35 assertions del motor mantingudes i passades. El motor conserva capacitats internes que aquesta interfície ja no exposa.
- Contracte INPUT Win32 comprovat en 32 i 64 bits.
- Càrrega de les dues icones incrustades mitjançant el carregador real de la safata, ús després de tancar els streams, diferència dels estats a mida petita i comprovació dels nou marcs ICO.
- Extracció de la icona des de l'executable compilat, confirmant el recurs de l'Explorador. Inspecció visual de les tasses sobre fons clar i fosc i de la icona extreta.
- Comprovació de les combinacions bloqueig/suspensió, desbloqueig durant suspensió, represa automàtica, represa repetida i reconnexió durant bloqueig. Es crida directament el receptor de prova: no es bloqueja ni se suspèn realment el PC.

Les comprovacions de recursos i avisos de sessió sumen 32 assertions addicionals, amb font a tests/ReleaseChecks.cs. No executen el context de l'aplicació ni generen entrada o peticions d'energia. La selecció de marcs ICO de 256 píxels de System.Drawing.Icon a .NET Framework té un comportament diferent de la selecció de mides petites: aquest marc s'ha validat descodificant directament el PNG de 256x256 del directori ICO.

No s'ha fet una prova d'interacció sobre la sessió de treball de l'usuari. La interfície, la càrrega en diferents DPI i l'efectivitat contra les polítiques de l'equip gestionat continuen pendents de la seva prova. No s'ha publicat ni comprovat res en remot.

## Seguiment de consum

Registres natius de Kimi: 5 peticions amb ús, 11.590 tokens d'entrada sense caché, 24.832 de caché i 3.756 de sortida. Model atribuït pel client: kimi-code/kimi-for-coding; no és una verificació independent dels pesos del servei. Límit per procés: 7 passos, 4.096 tokens de sortida per pas, un intent per pas i 300 segons.

No hi ha lectura inicial/final de quota per a aquesta iteració. Els darrers valors 20% i 9% corresponen al final de la prova anterior i no s'han reutilitzat com si fossin mesures actuals. No es converteixen aquests tokens a euros o percentatge de quota.

L'encàrrec més petit ha arribat a un lliurament complet i ha necessitat menys correccions del codi de Kimi que l'esquelet inicial. Això afavoreix provar tasques acotades amb revisió; no és una comparativa controlada de cost o qualitat entre models.

## Llicència

Codi, art original i documentació sota MIT. Sense recursos copiats de Caffeine o de GNOME. Els registres de coordinació i sessions queden fora del paquet públic.

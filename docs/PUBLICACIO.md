# Preparar la publicació a GitHub

1. Revisa la nova aparença, l'inici opcional amb Windows i la visibilitat després de reiniciar. Confirma la prova d'ús local. La prova manual de més de dues hores correspon a la versió 0.2.0 i al dispositiu de l'usuari; no és una validació de tots els Windows gestionats.
2. Crea un repositori públic amb el codi de `source/` o del ZIP de fonts. Conserva LICENSE, README, els scripts, els recursos i la carpeta `.github/`.
3. Exclou `.tools/`, `build/` i `dist/`, tal com indica `.gitignore`. No hi afegeixis els registres privats del CLI ni altres projectes.
4. Crea una publicació per a l'etiqueta `v1.0.1` i adjunta `Despert-1.0.1-portable.zip`, `Despert-1.0.1-source.zip`, `Despert-1.0.1-complete.zip` i `Despert-1.0.1-SHA256SUMS.txt`.
5. Enllaça la publicació i el README al Nus. Recomana el paquet complet a qui vulgui tenir fonts i executable junts.

El formulari de problemes de `.github/ISSUE_TEMPLATE/` permet recollir versió de Windows, versió de Despert, durada i context de la prova sense afegir opcions a l'aplicació.

Text suggerit per a la publicació:

> Despert és una petita utilitat portable per a Windows amb un únic interruptor per mantenir temporalment despert l'ordinador. Comença desactivada. Té una tassa monocroma amb fum per identificar l'estat i menú contextual nadiu. Permet iniciar-se opcionalment amb Windows, sempre desactivada. La interfície segueix l'idioma de Windows en català, castellà, euskera, gallec o anglès, i Sobre mostra autor, repositori, portfoli, MIT i V4V voluntari. Inclou fonts, instruccions de compilació reproduïble i manifests SHA-256. La compatibilitat concreta amb cada equip gestionat s'ha de comprovar al dispositiu.

La publicació es prepara a https://github.com/aaronFortuno/despert i a la seva secció Releases. Les comprovacions locals no inclouen comprovacions en remot.

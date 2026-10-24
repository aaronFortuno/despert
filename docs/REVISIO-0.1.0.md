# Revisió inicial · Despert 0.1.0

24 d'octubre de 2026. Estat: prototip revisat i compilat localment, pendent de prova manual de l'usuari. No s'ha publicat cap versió remota.

## Resultat disponible

Aplicació de safata en català, inicialment desactivada, amb tres modes: només petició d'energia, ratolí sense desplaçament i moviment mínim d'anada i tornada. Durades de 30/60/120 minuts o indefinida; inactivitat de 30/45/60 segons. Només endollat per defecte. El canvi de durada aplica a la propera activació; canviar mode o inactivitat reinicia conservadorament l'interval sense allargar la sessió.

Els impulsos només s'envien després de la inactivitat mínima, amb comprovació de botons premuts i una segona comprovació immediatament abans d'injectar. El mode mínim envia dos moviments oposats en una sola crida. No s'envien tecles, clics ni roda; no s'utilitzen hooks d'entrada. Una carrera de temps amb l'activitat humana i els efectes del moviment sobre les aplicacions no es poden excloure només amb revisió estàtica.

En bloquejar, desconnectar una sessió remota, suspendre, caducar o desendollar amb l'opció només endollat, es desactiva i allibera la petició d'energia. No es reactiva automàticament després. La recepció efectiva d'aquests avisos a l'equip de destinació està pendent de prova manual.

## Comprovacions executades

- Compilació amb C# 5 i .NET Framework del Windows local, nivell d'avisos 4 i avisos tractats com a errors. Sense dependències descarregades.
- 35 assertions de lògica passades: inici desactivat, espera inicial, activitat recent, intervals entre impulsos, límits exactes, expiració, botons, alimentació, reinici de sessió, configuració, canvi de mode i wrap del comptador de Windows.
- Contracte binari Win32 comprovat en processos de 64 i 32 bits: INPUT de 40 i 28 bytes, amb desplaçament de la unió i del punter correctes.
- Revisió de les rutes de sortida, error i disposició de recursos, del fil que manté la petició d'energia i del registre dels avisos de sessió.
- Revisió de fonts: sense xarxa, telemetria, instal·lació, canvis de polítiques o registre, captura de tecles o ús d'entrada de teclat. Les estructures de la unió Win32 inclouen la definició de teclat per respectar l'ABI, però no s'envia aquest tipus d'entrada.

Les proves no executen la interfície ni generen entrada, bloqueig, suspensió o peticions d'energia. No s'ha activat Despert automàticament a l'ordinador de l'usuari.

## Correccions de la revisió de Codex

El lliurament de Kimi era parcial i no compilava. S'han corregit la mida INPUT declarada com a constant, referències a una classe inexistent i ús de propietats de menú no disponibles. També s'han corregit:

- Comparació d'un interval en mil·lisegons amb segons i absència d'espera inicial; la lògica ara rep un únic rellotge monotònic testable.
- Icona invisible en iniciar, durada fixa de 60 minuts i controls sense connexió funcional; selecció exclusiva de cada grup.
- Errors del registre de bloqueig ignorats, fallades de consulta del punter i direcció del moviment a les vores.
- Alliberament de recursos en errors i sortida; avís i estat visible quan falla una petició.
- Proves que abans acceptaven l'absència d'espera inicial o no comprovaven l'interval complet.

S'ha conservat una còpia privada del lliurament original per comparar-lo; no forma part de la distribució pública. Codex ha fet aquestes correccions directament, sense més inferències Kimi.

## Pendent de l'usuari

Validar icona i menú, compatibilitat amb Google Docs/Slides/PowerPoint, comportament del moviment mínim, temporitzador i aturada en bloquejar o suspendre. Comparar els tres modes durant més temps que el límit habitual de l'equip gestionat. Una petició d'energia o SendInput acceptada no demostra que la política eviti la suspensió.

## Model i consum observat

Una execució d'inferència amb Kimi Code CLI 2.3.1, àlies explícit `kimi-code/kimi-for-coding`, esforç `low`, màxim 7 passos, 4.096 tokens de sortida per pas, un intent per pas i 300 segons de límit extern. No s'ha modificat la configuració global ni reactivat altres tasques Kimi. Un primer intent local no va poder escriure l'emmagatzematge del CLI i es va aturar abans de la inferència.

L'execució d'inferència va durar aproximadament 72 segons i es va aturar pel límit de passos, sense informe final. Va crear els quatre fitxers de l'esquelet. Els registres natius atribueixen 7 peticions a l'àlies seleccionat: 12.839 tokens d'entrada sense caché, 42.752 d'entrada de caché i 7.082 de sortida. No és una verificació independent de la versió interna del servei.

L'usuari ha comunicat 15% → 20% a la finestra de cinc hores i 8% → 9% mensual. Són +5 i +1 punts percentuals observats en comptadors compartits, sense conversió verificada a euros ni atribució exclusiva a aquesta tasca. El resultat requeria correccions substancials: aquesta prova encara no acredita que el repartiment estalviï prou quota o temps.

La selecció evita K3.1 Pro i HighSpeed. La [documentació de models de Kimi](https://www.kimi.ai/help/kimi-code/models) descriu l'àlies estàndard i les diferències de quota; no ofereix una garantia del consum d'aquest encàrrec. Els [límits del procés](https://www.kimi.com/code/docs/en/kimi-code-cli/configuration/env-vars) s'han aplicat només a aquesta execució.

## Llicència i distribució

Codi original sota MIT. Es poden compartir les fonts, la documentació i l'executable amb l'avís de llicència. No s'han incorporat fonts d'altres projectes. El prototip compilat no està signat i encara no s'ha validat a l'equip gestionat.

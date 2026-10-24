# Despert

Aplicació local de safata per a Windows. Codi original sota MIT. Codex coordina i revisa; Kimi implementa l'encàrrec acotat; l'usuari valida la interfície i l'efectivitat a l'equip gestionat.

- C# 5 i .NET Framework 4.8, Windows Forms. L'aplicació no necessita paquets externs, xarxa, instal·lador, drivers, permisos d'administrador ni modificacions de polítiques. Les eines de compilació són paquets oficials fixats a toolchain.json, preparats localment amb verificació SHA-256.
- Inicialment desactivada. Un sol interruptor: activació indefinida fins desactivació o sortida. Interval intern fix de 45 segons. Sense opcions de mode, durada, alimentació o diagnòstic a la interfície. Mai enviar tecles, clics ni roda.
- Inici amb Windows opcional, desmarcat per defecte, només per a l'usuari actual. Canviar exclusivament el valor propi Despert a HKCU Run després d'un clic explícit; mai a l'arrencada de l'aplicació ni durant proves. No conservar l'estat actiu entre processos. La visibilitat de la safata la decideix l'usuari als ajustos de Windows; no modificar Explorer ni polítiques per forçar-la.
- Qualsevol entrada de teclat/ratolí ajorna els impulsos. Els impulsos propis també poden actualitzar el comptador de sessió; no etiquetar-lo com a activitat física exclusiva.
- Alliberar peticions d'energia en desactivar, sortir, bloquejar o suspendre. Mentre està bloquejat o suspès no enviar impulsos. Conservar la intenció de l'interruptor fins que l'usuari la canviï; reprendre després de desbloquejar si encara estava activat.
- L'usuari fa les proves visuals i les proves llargues de repòs. Les proves automàtiques han de ser sense efectes d'entrada ni d'energia.
- Publicar només quan l'usuari ho demani explícitament; ha autoritzat el repositori públic Despert i la primera versió. No executar l'aplicació activa automàticament. No alterar altres projectes ni la configuració global de Kimi.
- Interfície en català, castellà, euskera, gallec i anglès, seleccionada per CurrentUICulture de Windows, amb fallback anglès. Mantenir claus i placeholders complets. Sobre inclou portfoli, repositori, MIT i V4V voluntari; no obrir xarxa ni copiar al porta-retalls sense clic explícit.
- En projectes remots comprovats localment, la comprovació remota la fa l'usuari.
- Conservar compilació determinista i verificar el paquet de fonts net en una ruta diferent. No regenerar ICO automàticament en compilacions normals.

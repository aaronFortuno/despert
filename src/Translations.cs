// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Aarón Fortuño

using System;
using System.Collections.Generic;

namespace Despert
{
    internal static class Translations
    {
        internal static Dictionary<string, string[]> Create()
        {
            return new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                // English, Catalan, Spanish, Basque, Galician
                { "Activate", new string[] { "&Activate", "&Activar", "&Activar", "&Aktibatu", "&Activar" } },
                { "Deactivate", new string[] { "&Deactivate", "&Desactivar", "&Desactivar", "&Desaktibatu", "&Desactivar" } },
                { "StartWindows", new string[] { "Start with &Windows", "Iniciar amb &Windows", "Iniciar con &Windows", "Hasi &Windows-ekin", "Iniciar con &Windows" } },
                { "Unavailable", new string[] { "unavailable", "no disponible", "no disponible", "ez dago erabilgarri", "non dispoñíbel" } },
                { "About", new string[] { "A&bout…", "So&bre…", "A&cerca de…", "&Honi buruz…", "A&cerca de…" } },
                { "Exit", new string[] { "E&xit", "&Surt", "&Salir", "Ir&ten", "&Saír" } },
                { "StatusOn", new string[] { "Despert: active", "Despert: actiu", "Despert: activo", "Despert: aktibo", "Despert: activo" } },
                { "StatusOff", new string[] { "Despert: inactive", "Despert: inactiu", "Despert: inactivo", "Despert: desaktibo", "Despert: inactivo" } },
                { "StatusError", new string[] { "Despert: stopped after an error", "Despert: aturat després d'un error", "Despert: detenido tras un error", "Despert: errore baten ondoren gelditu da", "Despert: detido tras un erro" } },
                { "SessionError", new string[] { "Could not register session notifications. Despert cannot start.", "No s'han pogut registrar les notificacions de sessió. Despert no pot iniciar-se.", "No se pudieron registrar las notificaciones de sesión. Despert no puede iniciarse.", "Ezin izan dira saioaren jakinarazpenak erregistratu. Despert ezin da abiatu.", "Non se puideron rexistrar as notificacións de sesión. Despert non pode iniciarse." } },
                { "MissingIcon", new string[] { "Icon resource not found: {0}", "No s'ha trobat el recurs de la icona: {0}", "No se encontró el recurso del icono: {0}", "Ez da ikono-baliabidea aurkitu: {0}", "Non se atopou o recurso da icona: {0}" } },
                { "ReadStartupError", new string[] { "Could not read the Windows startup setting: {0}", "No s'ha pogut llegir la configuració d'inici del Windows: {0}", "No se pudo leer la configuración de inicio de Windows: {0}", "Ezin izan da Windows abio-konfigurazioa irakurri: {0}", "Non se puido ler a configuración de inicio de Windows: {0}" } },
                { "MenuError", new string[] { "Could not open the menu: {0}", "No s'ha pogut obrir el menú: {0}", "No se pudo abrir el menú: {0}", "Ezin izan da menua ireki: {0}", "Non se puido abrir o menú: {0}" } },
                { "WriteStartupError", new string[] { "Could not change the Windows startup setting: {0}", "No s'ha pogut canviar la configuració d'inici del Windows: {0}", "No se pudo cambiar la configuración de inicio de Windows: {0}", "Ezin izan da Windows abio-konfigurazioa aldatu: {0}", "Non se puido cambiar a configuración de inicio de Windows: {0}" } },
                { "IconError", new string[] { "Could not update the icons: {0}", "No s'han pogut actualitzar les icones: {0}", "No se pudieron actualizar los iconos: {0}", "Ezin izan dira ikonoak eguneratu: {0}", "Non se puideron actualizar as iconas: {0}" } },
                { "Error", new string[] { "Error: {0}", "Error: {0}", "Error: {0}", "Errorea: {0}", "Erro: {0}" } },
                { "AlreadyRunning", new string[] { "Despert is already running.", "Despert ja s'està executant.", "Despert ya se está ejecutando.", "Despert jada exekutatzen ari da.", "Despert xa está executándose." } },
                { "Stopped", new string[] { "Despert has stopped: {0}", "Despert s'ha aturat: {0}", "Despert se ha detenido: {0}", "Despert gelditu da: {0}", "Despert detívose: {0}" } },
                { "StartupOpenError", new string[] { "Could not open the current user's startup settings.", "No s'ha pogut obrir la configuració d'inici de l'usuari actual.", "No se pudo abrir la configuración de inicio del usuario actual.", "Ezin izan dira uneko erabiltzailearen abio-ezarpenak ireki.", "Non se puido abrir a configuración de inicio do usuario actual." } },
                { "PathEmpty", new string[] { "The executable path cannot be empty.", "La ruta de l'executable no pot estar buida.", "La ruta del ejecutable no puede estar vacía.", "Fitxategi exekutagarriaren bidea ezin da hutsik egon.", "A ruta do executábel non pode estar baleira." } },
                { "PathInvalid", new string[] { "The executable path contains invalid characters.", "La ruta de l'executable conté caràcters no vàlids.", "La ruta del ejecutable contiene caracteres no válidos.", "Fitxategi exekutagarriaren bideak baliogabeko karaktereak ditu.", "A ruta do executábel contén caracteres non válidos." } },
                { "PathLong", new string[] { "The startup command exceeds the maximum permitted length.", "L'ordre d'inici supera la longitud màxima permesa.", "El comando de inicio supera la longitud máxima permitida.", "Abio-komandoak gehienezko luzera gainditzen du.", "A orde de inicio supera a lonxitude máxima permitida." } },
                { "PathAbsolute", new string[] { "The executable path must be absolute.", "La ruta de l'executable ha de ser absoluta.", "La ruta del ejecutable debe ser absoluta.", "Fitxategi exekutagarriaren bideak absolutua izan behar du.", "A ruta do executábel debe ser absoluta." } },
                { "PathExe", new string[] { "The executable path must point to an .exe file.", "La ruta de l'executable ha d'apuntar a un fitxer .exe.", "La ruta del ejecutable debe apuntar a un archivo .exe.", "Bideak .exe fitxategi bat adierazi behar du.", "A ruta do executábel debe apuntar a un ficheiro .exe." } },
                { "IdleError", new string[] { "Could not read session idle time.", "No s'ha pogut llegir el temps d'inactivitat de la sessió.", "No se pudo leer el tiempo de inactividad de la sesión.", "Ezin izan da saioaren inaktibitate-denbora irakurri.", "Non se puido ler o tempo de inactividade da sesión." } },
                { "PowerError", new string[] { "Could not request the system and display to stay awake.", "No s'ha pogut demanar que el sistema i la pantalla es mantinguin actius.", "No se pudo solicitar que el sistema y la pantalla permanezcan activos.", "Ezin izan da eskatu sistema eta pantaila esna mantentzea.", "Non se puido solicitar que o sistema e a pantalla permanezan activos." } },
                { "PointerError", new string[] { "Could not read the pointer position.", "No s'ha pogut llegir la posició del punter.", "No se pudo leer la posición del puntero.", "Ezin izan da erakuslearen posizioa irakurri.", "Non se puido ler a posición do punteiro." } },
                { "InputError", new string[] { "SendInput sent {0} of {1} inputs; the session has stopped.", "SendInput ha enviat {0} de {1} entrades; la sessió s'ha aturat.", "SendInput envió {0} de {1} entradas; la sesión se ha detenido.", "SendInput-ek {0}/{1} sarrera bidali ditu; saioa gelditu da.", "SendInput enviou {0} de {1} entradas; a sesión detívose." } },
                { "Description", new string[] { "Keep your Windows computer awake with one click.", "Mantén el teu ordinador Windows despert amb un sol clic.", "Mantén tu ordenador Windows despierto con un solo clic.", "Mantendu zure Windows ordenagailua esna klik bakar batez.", "Mantén o teu ordenador Windows esperto cun só clic." } },
                { "Credits", new string[] { "Created by Aarón Fortuño. Code and artwork under the MIT license.", "Creat per Aarón Fortuño. Codi i gràfics sota la llicència MIT.", "Creado por Aarón Fortuño. Código y gráficos bajo la licencia MIT.", "Aarón Fortuñok egina. Kodea eta diseinua MIT lizentziapean.", "Creado por Aarón Fortuño. Código e gráficos baixo a licenza MIT." } },
                { "Repository", new string[] { "Source code on GitHub", "Codi font a GitHub", "Código fuente en GitHub", "Iturburu-kodea GitHub-en", "Código fonte en GitHub" } },
                { "Portfolio", new string[] { "More projects by the author", "Més projectes de l'autor", "Más proyectos del autor", "Egilearen proiektu gehiago", "Máis proxectos do autor" } },
                { "License", new string[] { "MIT license", "Llicència MIT", "Licencia MIT", "MIT lizentzia", "Licenza MIT" } },
                { "V4VTitle", new string[] { "Value for Value · V4V", "Value for Value · V4V", "Value for Value · V4V", "Value for Value · V4V", "Value for Value · V4V" } },
                { "V4VBody", new string[] { "If Despert is useful to you, you can give value back: share it, suggest improvements, translate it or make a voluntary contribution.", "Si Despert et resulta útil, pots retornar valor: comparteix-lo, suggereix millores, tradueix-lo o fes una contribució voluntària.", "Si Despert te resulta útil, puedes devolver valor: compártelo, sugiere mejoras, tradúcelo o haz una contribución voluntaria.", "Despert erabilgarria bazaizu, balioa itzul dezakezu: partekatu, hobekuntzak iradoki, itzul ezazu edo ekarpen boluntario bat egin.", "Se Despert che resulta útil, podes devolver valor: compárteo, suxire melloras, tradúceo ou fai unha contribución voluntaria." } },
                { "Copy", new string[] { "Copy address", "Copia l'adreça", "Copiar dirección", "Kopiatu helbidea", "Copiar enderezo" } },
                { "Copied", new string[] { "Address copied", "Adreça copiada", "Dirección copiada", "Helbidea kopiatu da", "Enderezo copiado" } },
                { "CopyError", new string[] { "Could not copy the address: {0}", "No s'ha pogut copiar l'adreça: {0}", "No se pudo copiar la dirección: {0}", "Ezin izan da helbidea kopiatu: {0}", "Non se puido copiar o enderezo: {0}" } },
                { "LinkError", new string[] { "Could not open the link: {0}", "No s'ha pogut obrir l'enllaç: {0}", "No se pudo abrir el enlace: {0}", "Ezin izan da esteka ireki: {0}", "Non se puido abrir a ligazón: {0}" } },
                { "Close", new string[] { "Close", "Tanca", "Cerrar", "Itxi", "Pechar" } }
            };
        }
    }
}

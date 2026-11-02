# Firma de publicación de Little Legion

Certificado generado el 2 de noviembre de 2026. Identidad: CN=Little Legion; RSA de 4096 bits; alias littlelegion; vencimiento: 20 de marzo de 2054.

El certificado público está en little-legion-release-certificate.pem. Su huella SHA-256 es CEC0CDD8B4A66995A401748F35E4C56C01F9349047012335A629FAE77BE00BB3.

## Archivos privados locales

- private/little-legion-release.keystore: clave privada para firmar los APK.
- private/release-signing.json: ruta del keystore, alias y contraseñas generadas aleatoriamente.

La carpeta private está excluida de Git y tiene permisos locales restringidos. No adjuntar su contenido a tickets, paquetes de recursos ni logs. El certificado público no contiene la clave privada y no sirve para firmar.

Guardar una copia cifrada de **ambos archivos privados** en un lugar seguro y separado de este equipo antes de publicar. Esta tarea no crea una copia externa. Conservar esta misma clave para las futuras actualizaciones; no regenerarla después de publicar.

## Compilar

El menú Frontier/Build Quest APK y scripts/unity.sh quest usan ProjectBuilder.BuildQuest. Este método carga private/release-signing.json, activa el keystore propio y compila sin Development Build. Si falta la configuración de firma, la compilación se detiene en lugar de generar un APK con firma de debug.

Las contraseñas se leen al compilar y no se incluyen en el código fuente ni en ProjectSettings. Unity/Gradle pueden escribirlas en archivos generados de Library, también excluidos de Git; no compartir esa carpeta. Para otro equipo, restaurar los archivos privados de forma segura en esta misma carpeta. Las rutas de keystorePath se resuelven desde la raíz del proyecto.

Un APK firmado con esta clave no puede actualizar directamente el APK anterior firmado con Android Debug. En un visor que tenga ese APK de prueba habrá que desinstalarlo antes; desinstalar puede borrar preferencias, historial y partidas locales. No se desinstala ni instala nada automáticamente.

La firma de publicación no reemplaza la validación física en Quest, las verificaciones de Meta ni la preparación final de la ficha del store.

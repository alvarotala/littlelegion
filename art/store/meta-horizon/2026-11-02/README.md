# Little Legion — recursos para Meta Horizon Store

Preparados el 2 de noviembre de 2026 en dev. Arte promocional basado en la identidad de la landing: geometría low-poly, base industrial cian/amarilla y ejército biológico rosa. Las imágenes generadas son ilustraciones promocionales; no son capturas del juego.

## Entrega

| Archivo en upload/ | Dimensiones | Uso |
|---|---|---|
| hero-cover.png | 3000 × 900 | Hero |
| cover-landscape.png | 2560 × 1440 | Portada horizontal |
| cover-square.png | 1440 × 1440 | Portada cuadrada |
| cover-portrait.png | 1008 × 1440 | Portada vertical |
| mini-landscape.png | 1080 × 360 | Miniatura horizontal |
| icon-512.png | 512 × 512 | Icono del store |
| logo-transparent.png | hasta 1440 px de alto | Logo con alfa |

Las portadas y el icono se exportan como PNG RGB de 8 bits por canal. El logo se exporta como PNG RGBA. Las portadas contienen solamente el nombre del juego. Los originales están en masters/; las exportaciones ajustan tamaño y, cuando hace falta, recortan los bordes para respetar la proporción. Algunas exportaciones amplían el original generado; ampliar no añade detalle nativo.

app-icons/ contiene versiones cuadradas del icono existente para la app, de 48 a 1024 px. El original es de 512 px; el de 1024 es una ampliación. El icono está integrado en Assets/Frontier/Branding/LittleLegionIcon.png y ProjectBuilder.Configure() lo asigna automáticamente. Se compiló Builds/Quest/LittleLegion.apk el 2 de noviembre de 2026; firmado con el certificado de publicación propio CN=Little Legion; firma v2 e icono embebido verificados. Ver Signing/README.md en la raíz del proyecto para la custodia de la clave. El usuario confirmó que probó el juego en Quest.

store-copy.txt contiene un borrador en español e inglés. prompts.json conserva las instrucciones usadas con la herramienta integrada image_gen. export.swift permite repetir la exportación técnica en macOS; export-all.sh reproduce las salidas.

## Capturas del juego

La selección recomendada más reciente está en screenshots-v2/: dos batallas, un ejército industrial y primeros planos de trabajadores de ambas facciones. ZIP: LittleLegion-Meta-Screenshots-v2-2560x1440.zip. El primer juego de capturas se conserva en screenshots/.

screenshots/ contiene cinco imágenes distintas de 2560 × 1440, capturadas a resolución nativa con BoardView y la simulación del juego en Unity. Muestran economía industrial en Forest Watch, base biológica en el desierto, combate en Forest Watch, ejército en Neon Loop y expansión industrial en el desierto. No tienen texto publicitario, logos añadidos ni retoque generativo.

Las partidas avanzan con las reglas normales y decisiones de la IA para ambos jugadores, conservando niebla de guerra y visibilidad del jugador. No se agregan unidades ni recursos de exhibición. Las imágenes proceden del render de Unity Editor; no se presentan como capturas tomadas desde el visor. Cada PNG tiene un JSON de procedencia con mapa, facción, tick y encuadre. StoreScreenshotCapture.Render permite repetir la captura.

El ZIP LittleLegion-Meta-Screenshots-2560x1440.zip contiene únicamente los cinco PNG. El tráiler y su miniatura quedan pendientes.

## Estado de publicación

Este paquete prepara las gráficas y el texto; no acredita que el juego esté aprobado ni listo para enviar. El APK ya está firmado para publicación. Quedan el vídeo y completar la ficha y las verificaciones del Developer Dashboard. No se hizo push ni se subieron recursos a Meta.

Fuente de dimensiones y reglas, consultada el 2 de noviembre de 2026: [Asset design guidelines de Meta](https://developers.meta.com/vr/resources/asset-guidelines/). Confirmar los campos que muestre el Dashboard al cargar la ficha.

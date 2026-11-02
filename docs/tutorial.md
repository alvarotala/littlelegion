# Tutorial guiado

**Tutorial guiado**, en el menú principal, inicia una lección local en el mapa de entrenamiento. Usa la simulación normal, con sus costes, construcción, producción, entregas de recursos y condición de victoria. El enemigo no tiene un estratega de IA durante la lección: las acciones siguen una secuencia obligatoria hasta destruir su base.

Cada lección comienza con una idea breve y el juego en pausa. **Continuar** habilita la práctica de ese paso; nunca omite el objetivo. El paso siguiente aparece cuando el jugador ejecuta la acción y se produce el resultado requerido. Una línea violeta conecta la ventana de instrucciones con la unidad, recurso, lugar o control correspondiente. Las órdenes ajenas al objetivo se rechazan y las opciones de producción o construcción se limitan a la lección actual.

El recorrido básico tiene los mismos **13 pasos en escritorio y Quest**:

1. Conocer el objetivo y la ventana flotante del tutorial.
2. Seleccionar un trabajador.
3. Moverlo al marcador.
4. Recolectar minerales y esperar una entrega.
5. Seleccionar la base principal.
6. Entrenar un trabajador.
7. Construir un edificio de suministro.
8. Construir un edificio militar.
9. Entrenar tres soldados a distancia.
10. Seleccionar los soldados con un rectángulo.
11. Usar **Avanzar y atacar** hasta el marcador.
12. Atacar la base enemiga y destruir todos sus edificios.
13. Volver al menú principal o repetir tras la victoria.

Las ideas breves presentan trabajadores, minerales, costes, población, construcción, producción y combate. Construir, entrenar y activar Avanzar y atacar se enseñan desde los botones del menú in-game en ambos dispositivos. La guía señala el botón real antes de pedir un destino en el mapa.

La rueda, los grupos de control, el gas y la navegación por pausa quedan fuera del recorrido. La rueda y los grupos no se activan durante esta práctica. El jugador puede abrir pausa o ajustes cuando lo necesite y regresar al mismo objetivo. Las partidas normales conservan esos controles y todas las reglas del juego.

## Controles, pausas y presentación

Las instrucciones aparecen en una ventana flotante independiente que se acomoda al foco de la lección. Durante las explicaciones se sitúa sobre el menú in-game. Cuando la acción ocurre en el mapa, se sitúa detrás del objetivo desde la vista del jugador, con una separación visible por encima para dejar libre el objeto y el punto de acción. Ajusta su tamaño al espacio disponible y se coloca directamente en esa posición al cambiar de objetivo para no cruzarse sobre él; el menú in-game se oscurece para quedar en segundo plano. Cuando hay que usar una opción del menú, la guía se coloca junto a esa opción y el menú recupera su aspecto normal. La consola conserva su tamaño y distribución. Moverla o cambiar su escala también acomoda la guía. El diseño usa fondo oscuro, letras blancas y acentos violetas, con un número de lección, barra de progreso, título corto, una idea y la próxima acción. El tono es cercano y sencillo: «¡En marcha!», «Tu pequeña legión» o «¡Lo lograste!».

La línea violeta dirige la atención al objetivo del momento: una unidad, un recurso, el contorno de una construcción o un botón del menú. Los botones del menú reciben también un contorno violeta. La ventana permanece disponible durante la pausa y al consultar ajustes o guías; se oculta durante la colocación del tablero, la confirmación de abandonar y las interrupciones de foco o seguimiento. Al terminar, presenta las opciones para volver al menú principal o repetir. El pie de la consola identifica la práctica guiada sin mostrar la dificultad de IA guardada para partidas normales.

Una lección se divide en acciones pequeñas. Para construir, primero pide seleccionar un trabajador, después elegir el edificio y luego colocarlo en el contorno. La instrucción cambia a «Espera a que termine la construcción» cuando el trabajador empieza. Entrenar también distingue entre seleccionar el productor, añadir las unidades que faltan y esperar su llegada. Si faltan recursos, pide dar un momento a los recolectores. Cada cambio actualiza el texto y el destino de la línea, de modo que la ventana muestra lo que toca hacer ahora. Una animación breve, un sonido y una vibración del control señalan los cambios de estado para que el jugador los advierta sin perder el objetivo.

Si el jugador abre un menú durante un objetivo, la guía le indica volver y reanudar antes de continuar con órdenes en el mapa. Si cambia la selección mientras prepara una construcción o un destino de ataque, primero le pide cancelar ese modo con su botón configurado y después volver a seleccionar. La guía explica la recuperación; no cancela acciones ni cambia selecciones automáticamente.

Los nombres de los botones se obtienen del perfil actual de `HotkeyBindings`. Los botones de consola usan gatillo/A o X en Quest y clic izquierdo en escritorio, independientemente del remapeo de acciones del juego. Seleccionar, dar órdenes y cancelar muestran sus controles configurados. Los textos están disponibles en inglés y español.

Durante una explicación se detienen la simulación y los controles de juego; el mapa adquiere un tono gris para indicar que la atención está en la guía. El botón **Continuar** de la ventana y la navegación de la consola siguen disponibles. Se conservan la idea y los mensajes de la lección, pero se oculta la instrucción duplicada de pulsar Gatillo/A sobre **Continuar**. Las instrucciones de controles aparecen durante la práctica, cuando el jugador debe realizar una acción.

Mientras una lección espera un gesto —por ejemplo, seleccionar un trabajador o elegir una construcción—, el reloj de la simulación también espera. En esa espera los controles necesarios siguen habilitados, pero los trabajadores no agotan los depósitos y los soldados no actúan por adelantado. El tiempo avanza cuando ya se aceptó una orden que debe completarse, o cuando hace falta reunir recursos para pagar la acción indicada. Construcción, producción, desplazamiento, entregas y combate mantienen sus tiempos y reglas reales.

El inicio pasa por el flujo normal de colocación del tablero. La pausa del tutorial tiene un motivo propio: continuar una explicación no libera las pausas de menú, colocación, foco o seguimiento del visor. Salir o reiniciar limpia el estado de la lección y conserva por separado la configuración de partidas normales. El tutorial no se añade al historial ni genera una repetición.

## Integración y validación

`GuidedTutorial` define la secuencia, las acciones permitidas y las condiciones de avance. `GameSession.Tutorial` integra el director con la sesión y la simulación. El runtime informa de órdenes aceptadas y selección; observar un resultado real completa el objetivo. `CommandPanel.TutorialContent` resuelve los textos cortos y la acción inmediata según la selección, los modos de construcción o ataque y `AcceptedOrderCount`. Solo consulta la vista filtrada del jugador para la presentación. `CommandPanel.Tutorial` mantiene el canvas independiente, los botones para continuar, reiniciar o salir y las líneas hacia el mapa o la consola. Sus controles usan la misma entrada del panel, sin modificar la distribución del HUD.

La validación contempla el recorrido completo de ambas facciones en escritorio y Touch, el rechazo de órdenes ajenas al objetivo, entregas y producción reales, la independencia de pausas, la selección rectangular del ejército y la victoria sin gas ni grupos. Las comprobaciones de presentación del Editor contemplan la entrada principal, las ventanas pausada, activa y final, su independencia respecto al HUD, los cambios de instrucciones y de objetivo de la línea, su posición según el foco, la atenuación del menú y del mapa, los botones reales de la consola en ambos dispositivos, la rueda inactiva durante el tutorial y disponible después, la presencia de la guía en menús, las traducciones, los controles remapeados y los límites de texto. Las pruebas existentes de rueda, atajos y localización siguen siendo relevantes para detectar regresiones. Los resultados concretos se registran en `docs/validation.md`.

La simulación de escritorio, las capturas del Editor y una compilación Android verifican aspectos distintos. La comodidad, legibilidad e interacción sobre un visor Quest físico requieren una prueba en ese dispositivo.

Para repetir las comprobaciones:

```sh
bash scripts/check-core.sh --tutorial-only
unity run . --editor-version 6000.3.26f1 --timeout 600 --log-file TestResults/unity-guided-tutorial.log --no-tail -- -buildTarget StandaloneOSX --burst-disable-compilation -executeMethod Frontier.Editor.GuidedTutorialPreview.RunFocusBatch
```

Las capturas del tutorial quedan en `TestResults/GuidedTutorial/` y las del efecto gris en `TestResults/TutorialBoardFocus/`. El batch añade las regresiones del menú y de la rueda unificada, sin generar un APK.

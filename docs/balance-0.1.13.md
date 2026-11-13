# Balance 0.1.13

Ambas facciones conservan las mismas estadísticas por rol. Los tiempos siguientes son segundos de simulación (20 ticks/s), sin desplazamientos, recolección ni esperas de cola. El objetivo de partidas de 15–20 minutos es una hipótesis de diseño, no una duración garantizada.

## Unidades

| Rol | Vida | Daño / intervalo | Minerales / gas | Entrenamiento | Población |
| --- | ---: | --- | ---: | ---: | ---: |
| Trabajador | 90 | 4 / 1 s | 50 / 0 | 12 s | 1 |
| Explorador | 140 | 6 / 0,8 s | 60 / 0 | 10 s | 1 |
| Cuerpo a cuerpo | 400 | 14 / 1,2 s | 125 / 0 | 22 s | 1 |
| A distancia | 240 | 18 / 1,5 s | 100 / 0 | 18 s | 1 |
| Asedio | 420 | 60 / 3 s | 280 / 150 | 45 s | **2** |
| Mago / apoyo | 180 | 8 / 1,5 s | 200 / 125 | 36 s | **2** |

Se conservan velocidades, alcance, visión y colisiones. El asedio hace un 50 % más de daño directo a edificios y un 25 % de su daño de arma a otros enemigos a menos de dos celdas del centro del objetivo. El bono contra edificios no se transfiere al daño secundario. Cada víctima aplica su propia armadura.

Las dos habilidades de cada mago conservan alcance 8 y radio 3. Daño en área: 55 de daño, 60 de energía y 12 s de recarga. Curación: hasta 70 de vida a unidades aliadas, 50 de energía y 12 s de recarga. Energía máxima 100, regeneración 2/s; las habilidades no escalan con mejoras de armas ni armadura. Los trabajadores siguen reparando y ninguna facción regenera vida pasivamente.

## Edificios

| Rol | Minerales / gas | Construcción | Vida | Capacidad |
| --- | ---: | ---: | ---: | ---: |
| Centro principal | 350 / 0 | 40 s | 2400 | 10 |
| Extractor | 100 / 0 | 20 s | 700 | 0 |
| Suministro | 100 / 0 | 18 s | 650 | 15 |
| Cuartel | 175 / 0 | 30 s | 1200 | 0 |
| Arsenal / Crisálida | 300 / 200 | 60 s | 1400 | 0 |
| Laboratorio / Santuario | 250 / 175 | 50 s | 1100 | 0 |
| Centro de mejoras | 175 / 75 | 35 s | 1000 | 0 |
| Defensa | 160 / 0 | 30 s | 800 | 0 |

La defensa inflige 20 de daño cada segundo, conservando alcance 9. Arsenal + primer asedio cuesta 580 minerales / 350 gas y requiere 105 segundos de producción encadenada, además de sus requisitos y economía.

## Mejoras

Ataque y armadura avanzan por separado. Cada nivel de ataque suma un 12 % del daño base de armas; cada nivel de armadura reduce un 8 % del daño de armas, hasta 24 %. Se multiplican ambos factores, con daño mínimo 1 por impacto. A nivel III contra nivel III, el multiplicador combinado es 1,0336. Las mejoras afectan a unidades y edificios.

| Nivel, por rama | Minerales / gas | Tiempo | Requisito adicional |
| --- | ---: | ---: | --- |
| I | 150 / 100 | 45 s | Centro de mejoras terminado |
| II | 275 / 200 | 65 s | Nivel anterior + Arsenal / Crisálida terminado |
| III | 400 / 300 | 90 s | Nivel anterior + Laboratorio / Santuario terminado |

Los requisitos se comprueban al ordenar la investigación. Una investigación pagada continúa aunque se pierda su edificio de tecnología; destruir el centro que la investiga conserva el comportamiento de pérdida de cola existente.

## Economía y población

Los tres mapas tienen 3600 por mineral y 4800 por gas, el doble de sus reservas anteriores. El generador de Desser Alert usa los mismos valores. Se conservan la carga de 8 y las oportunidades de extracción cada 8 ticks.

El óptimo sigue siendo 2 trabajadores por mineral y 3 por gas. Con `n` asignados y óptimo `o`, cada trabajador recibe `min(1, (o + min(n-o, o) × 0,25) / n)` de su extracción nominal. Los excedentes aportan rendimiento reducido y el conjunto se limita al 125 % del óptimo. Se cuentan también quienes entregan carga; los viajes y la congestión siguen afectando el ingreso real. La extracción fraccionaria se acumula y solo descuenta recursos al producir una unidad entera, sin crear recursos ni perder cargas parciales.

El límite es 100 puntos de población y 30 edificios por jugador. La cola reserva el consumo completo de cada unidad. Cancelar devuelve esos puntos; si se pierde capacidad, una unidad terminada espera hasta que quepa. Las estadísticas de cantidad de unidades siguen contando individuos; HUD, órdenes de construcción e IA cuentan puntos donde corresponde.

Se mantienen 400 minerales, cero gas, seis trabajadores y una base con capacidad 10 al empezar. La IA respeta el mismo catálogo y las mismas reglas: Easy apunta a 24 trabajadores / 50 puntos militares, Normal a 30 / 70 y Hard a 36 / 64. Puede destinar a ejército los puntos económicos que no necesita, según saturación y bases. El primer momento de considerar tecnología es 4:00 / 3:30 / 3:00, respectivamente; luego debe pagar y construir normalmente. La academia se considera desde 6:00 / 5:15 / 4:30. Mantiene los primeros ataques desde 5:00 / 3:30 / 2:30, con los mínimos de ejército existentes; los máximos por oleada son 32 / 54 / 64 individuos.

## Compatibilidad

Versión de aplicación 0.1.13, código Android 14, `ReplayRulesVersion=7`. Los informes anteriores siguen disponibles, pero sus replays requieren las reglas anteriores. Cliente y servidor deben compartir reglas y contenido; este cambio local no despliega el servidor ni publica una build.

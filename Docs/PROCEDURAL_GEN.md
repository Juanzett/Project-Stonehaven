# Procedural Generation - Project Stonehaven

## Resumen
- Mundo finito (ej: 512x512 tiles).
- Seed determinista (System.Random(seed)).
- Biomas con Perlin Noise (umbrales).
- Ciudades grandes = prefabs instanciados (semi-procedural).
- Cuevas: Cellular Automata + Drunkard Walk para túneles.
- Guardado: seed + delta de cambios del jugador.

## Pipeline (simplificado)
1. Definir seed.
2. Generar mapas de ruido: bioma, altura, humedad.
3. Clasificar tiles por umbrales en biomas.
4. Post-procesado (suavizado, correcciones).
5. Colocación de ciudades con reglas (distancia, plano, no agua).
6. Generar cuevas/subsuelo y conexiones a superficie.
7. Poblar recursos y entidades por bioma.

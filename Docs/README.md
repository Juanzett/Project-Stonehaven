# Project Stonehaven

**Project Stonehaven** es un juego 2D top-down desarrollado en Unity (C#), enfocado en supervivencia, exploración, gestión y combate en un mundo generado proceduralmente y semi-proceduralmente. El proyecto busca combinar mecánicas de crafting, construcción, navegación, raids y gestión de ciudades en un mundo con biomas diversos y mapas finitos determinados por semillas.

---

## 🎮 Visión del Juego

- Mundo finito generado a partir de una **seed determinista**, garantizando rejugabilidad.  
- Biomas principales: bosque, lago, montaña, praderas.  
- Cuevas y subsuelo con generación procedural conectadas al mundo principal.  
- Ciudades grandes instanciadas a partir de prefabs (semi-procedural).  
- Pueblos pequeños y granjas generados proceduralmente.  
- Eventos dinámicos como raids de enemigos, bosses marinos y terrestres.  
- Gestión de recursos, crafting avanzado y economía (contratación de trabajadores, creación de negocios).  
- Navegación en barcos y portales late-game para recorrer el mundo eficientemente.  

---

## 🛠 Mecánicas Principales

1. **Recolección y recursos**: talar, minar, pescar, cazar.  
2. **Crafting y refinado**: fundición de metales, pulido, fabricación de herramientas y armas.  
3. **Construcción y gestión**: casas, talleres, negocios, granjas y expansión de ciudades.  
4. **Combate y defensa**: enemigos normales, bosses y eventos de raids periódicos.  
5. **Mascotas y animales**: caza, domesticación y cría de mascotas.  
6. **Navegación y exploración**: barcos, viajes entre biomas y portales late-game.  

---

## 🌍 Procedural Generation

- **Mapa finito**: ejemplo inicial de 512x512 tiles (tile size 32x32).  
- **Seed determinista**: todos los mundos son reproducibles usando la misma semilla.  
- **Biomas**: generados con Perlin Noise; cada bioma tiene thresholds de altura, humedad y recursos.  
- **Ciudades grandes**: prefabs instanciados siguiendo reglas de distancia, posición y biome.  
- **Pueblos pequeños**: generados proceduralmente con reglas aleatorias dentro de ciertas restricciones.  
- **Cuevas**: Cellular Automata + Drunkard Walk para túneles; garantizando conexión con la superficie.  
- **Guardado**: se almacena la seed y solo los cambios aplicados por el jugador, optimizando espacio.

---

## 🗂 Estructura del Proyecto

Project-Stonehaven/
├── Assets/ # Sprites, prefabs, scripts, tiles
│ ├── Scripts/
│ │ └── WorldGenerator.cs
│ ├── Tiles/
│ ├── Sprites/
│ └── ...
├── Scenes/ # Escenas del juego
│ └── World.unity
├── Docs/ # Documentación del proyecto
│ ├── GAME_DESIGN.md
│ ├── PROCEDURAL_GEN.md
│ ├── MECHANICS.md
│ ├── OPTIMIZATION.md
│ ├── ROADMAP.md
│ └── CONTRIBUTING.md
├── Design/ # Diseño gráfico / prototipos
├── Builds/ # Builds de prueba
├── .gitignore
├── .gitattributes
├── LICENSE
└── README.md


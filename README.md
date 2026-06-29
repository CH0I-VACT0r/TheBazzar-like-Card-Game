# Bazaar-style Card Game Prototype

A Unity prototype inspired by The Bazaar that focuses on scalable gameplay architecture rather than complete game content.

The project emphasizes modular game systems including deckbuilding, inventory management, drag-and-drop interactions, ScriptableObject-based data management, and event-driven UI.


## Design Goals

The primary objective of this project was not to build a complete game, but to design reusable gameplay systems that can be extended to larger deckbuilding games.

The architecture prioritizes modularity, data-driven design, and maintainability.


## Technical Features

### Data-driven Card System

- ScriptableObject-based card definitions
- Separation of data and gameplay logic
- Easy expansion for new cards and items

### Deckbuilding

- Dynamic deck construction
- Card upgrades
- Card removal
- Deck validation

### Inventory System

- Grid-based inventory
- Item storage
- Equipment management
- Runtime inventory updates

### Drag & Drop

- Drag-and-drop interactions
- Slot validation
- Item swapping
- Visual feedback

### Event-driven UI

- Event-based communication between gameplay systems
- Decoupled UI architecture
- Automatic UI synchronization

### Battle System

- Real-Time combat prototype
- Card execution pipeline
- Damage calculation
- Status effect handling

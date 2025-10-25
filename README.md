# TestTask

Technical Unity repository with modular architecture for developing and testing various mini-projects.

## 🌐 WebGL Demo

A project built on the same architecture (TechnicalSample repository) is available on **Unity Play**:

**[▶️ Play on Unity Play](https://play.unity.com/en/games/8122daff-f003-466f-9eb4-fd11cb5fbcdc/idofront)**

This allows you to explore the modular architecture and its capabilities directly in your browser without downloading or installing anything.

## 🏗️ Architecture

The project is built on modular architecture principles, where each module is an isolated and independent component. The architecture follows the **MVP (Model-View-Presenter)** pattern with the possibility of using **Stateless** for state management.

### Core Principles:
- **Modularity**: Each module is encapsulated and independent
- **MVP Pattern**: Separation of logic into Model, View, and Presenter
- **Dependency Injection**: Using VContainer for dependency management
- **Asynchrony**: UniTask support for asynchronous operations
- **State Management**: Optional Stateless FSM for complex state coordination

### Architectural Patterns:
- **Standard MVP**: Single Model-View-Presenter per module (MainMenu, Converter)
- **Module Controller**: Coordinates state transitions and module lifecycle
- **Reactive Programming**: R3 integration for event-driven architecture

## 🛠️ Development Tools

### Module Creator
Built-in tool for automatically creating new modules:
- **Access**: `Tools > Create Module` in Unity Editor
- **Capabilities**:
  - Automatic folder structure creation
  - Generation of basic scripts (Installer, Presenter, View, Model)
  - Creation of Assembly Definition files
  - Generation of scenes and prefabs
  - Module type selection (Base, Additional, Test)

### Module Types:
- **Base**: Core application modules
- **Additional**: Additional modules
- **Test**: Test modules for debugging

## 📦 Existing Modules

### Base Modules
- **MainMenu**: Main application menu with standard MVP architecture
- **Bootstrap**: Game launch screen
- **ScrollSample**: Virtualized scroll list demonstration
  - 1000+ items with object pooling and virtualization
  - Placeholder-based optimization technique
  - Only visible items are active in memory
  - Demonstrates Object Pool & Factory patterns + DI integration
- **TimerSample**: Real-time display with zero allocations
  - Millisecond-precision timer (HH:mm:ss.fff)
  - Custom StringBuilder formatting for GC-free updates
  - Observable.EveryUpdate() pattern from R3
- **AnimationSample**: UI animation optimization
  - 5 animated + 100 static UI elements
  - DOTween + UniTask integration
  - Canvas Static optimization for static elements
  - Demonstrates proper cleanup patterns
- **RequestSample**: HTTP request handling
  - UnityWebRequest + UniTask async/await
  - External JSON configuration (RequestConfig.json)
  - Business logic in Model layer (MVP pattern)
  - Error handling and response parsing

### Test Modules
- **PopupsTester**: Test module for demonstrating the popup system
  - Implements PopupHub for modal window management
  - Contains test buttons for various scenarios
  - Demonstrates work with R3 (Reactive Extensions)

## 🏛️ Project Structure

```
Assets/
├── CodeBase/                 # Main codebase
│   ├── Core/                # System core
│   │   ├── Patterns/        # Architectural patterns
│   │   │   └── MVP/         # MVP interfaces
│   │   ├── Systems/         # System components
│   │   │   └── PopupHub/    # Popup management system
│   │   └── UI/              # UI components
│   ├── Services/            # Application services
│   ├── Editor/              # Editor tools
│   │   └── ModuleCreator/   # Module creator
│   └── Tests/               # Tests
├── Modules/                  # Application modules
│   ├── Base/                # Base modules
│   ├── Additional/          # Additional modules
│   └── Test/                # Test modules
└── Resources/                # Resources
```

## 🚀 Quick Start

1. **Clone the repository**
2. **Open the project in Unity** (Unity 6000.2.7f2 or newer)
3. **Explore existing modules**:
   - Open Bootstrap scene and press Play
   - Navigate through MainMenu to test different modules
   - Check performance modules (ScrollSample, TimerSample, AnimationSample, RequestSample)
4. **Create a new module** (optional):
   - In Unity Editor: `Tools > Create Module`
   - Choose module name
   - Select module type
   - Configure components to create
   - Click "Create Module"

## 📋 Requirements

- Unity 6000.2.7f2 or newer
- .NET Standard 2.1
- URP (Universal Render Pipeline) 2D
- Supported platforms: Windows, macOS, Linux, WebGL

## 🔧 Technologies

- **Unity 6000.2**: Main engine with URP 2D renderer
- **VContainer**: Dependency Injection container
- **UniTask**: Asynchronous operations and async/await patterns
- **R3**: Reactive Extensions for Unity (event-driven architecture)
- **Stateless**: Advanced state machine library for complex state management
- **DOTween**: Animation library for smooth UI transitions
- **TextMeshPro**: Advanced text rendering
- **Newtonsoft.Json**: JSON serialization/deserialization

## ⚡ Reactive Programming with R3

The repository extensively uses **R3 (Reactive Extensions for Unity)** for reactive programming patterns:

### Key Features:
- **Observable Streams**: Event-driven architecture with reactive data flows
- **UI Binding**: Automatic UI updates based on data changes
- **Event Handling**: Reactive event processing and composition
- **Memory Management**: Automatic subscription cleanup with `AddTo()` pattern

### Usage Examples:
```csharp
// Reactive button clicks with automatic cleanup
button.OnClickAsObservable()
    .Subscribe(_ => action.Invoke())
    .AddTo(this);

// Reactive data binding
dataStream
    .Where(x => x.IsValid)
    .Subscribe(UpdateUI)
    .AddTo(this);
```

### Benefits:
- **Declarative Code**: Clear data flow and event handling
- **Automatic Cleanup**: Prevents memory leaks with `AddTo()` pattern
- **Composition**: Easy combination of multiple event streams
- **Performance**: Efficient event processing and UI updates

## ⚡ Performance & Best Practices

The project demonstrates various optimization techniques and best practices:

### Memory Management
- **Object Pooling**: Reusable object pools with factory pattern
- **Zero Allocation**: GC-free updates using StringBuilder and custom formatting
- **Virtualization**: Only visible UI elements are active

### UI Optimization
- **Canvas Static**: Marking static UI elements for batch optimization
- **Rect Mask 2D**: Lightweight masking instead of heavy Mask component
- **Raycast Target**: Selective disabling for non-interactive elements
- **Layout Groups**: Automatic positioning with VerticalLayoutGroup

### Code Patterns
- **Async/Await**: UniTask for non-blocking operations
- **Reactive Programming**: R3 Observable streams for event handling
- **Dependency Injection**: VContainer for loose coupling
- **Factory Pattern**: Decoupled object creation with DI integration

## 📝 Features

- **Not a game**: This is a technical repository for development and testing
- **Mini-projects**: Each module represents a separate mini-project
- **Popup system**: Built-in modal window management system
- **Testing**: Support for creating test modules for debugging
- **Performance showcases**: Demonstrations of optimization techniques

## 🤝 Contributing

1. Fork the repository
2. Create a branch for new feature
3. Make changes
4. Create Pull Request

## 📄 License

[Specify project license]

---

**TestTask** - technical platform for modular development in Unity.

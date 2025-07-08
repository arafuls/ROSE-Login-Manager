# Services Directory Structure

This directory contains all the services used by the ROSE Login Manager application, organized into logical subdirectories for better maintainability and clarity.

## Directory Structure

```
Services/
├── Interfaces/           # Service interfaces (contracts)
│   ├── IDatabaseService.cs
│   ├── IEncryptionService.cs
│   ├── ILauncherSettingsService.cs
│   ├── IRoseClientService.cs
│   ├── IRosePatcherService.cs
│   └── IUserProfileService.cs
├── Implementations/      # Service implementations
│   ├── DatabaseService.cs
│   ├── EncryptionService.cs
│   ├── LauncherSettingsService.cs
│   ├── RoseClientService.cs
│   ├── RosePatcherService.cs
│   └── UserProfileService.cs
├── Models/              # Service-related models
│   └── ValidationResult.cs
├── ApplicationHostService.cs  # Application lifecycle service
└── README.md
```

## Organization Principles

### 1. Interface Segregation
- All service interfaces are placed in the `Interfaces/` directory
- Each interface follows the `I{ServiceName}Service` naming convention
- Interfaces use file-scoped namespaces for consistency

### 2. Implementation Separation
- All service implementations are placed in the `Implementations/` directory
- Implementations follow the `{ServiceName}Service` naming convention
- Each implementation implements its corresponding interface

### 3. Model Organization
- Service-specific models (like `ValidationResult`) are placed in the `Models/` directory
- This keeps service-related data structures separate from domain models

### 4. Special Services
- `ApplicationHostService.cs` remains in the root as it's a special lifecycle service
- It doesn't follow the typical interface/implementation pattern

## Naming Conventions

- **Interfaces**: `I{ServiceName}Service` (e.g., `IDatabaseService`)
- **Implementations**: `{ServiceName}Service` (e.g., `DatabaseService`)
- **Models**: `{ModelName}` (e.g., `ValidationResult`)

## Namespace Structure

- Interfaces: `ROSE_Login_Manager.Services.Interfaces`
- Implementations: `ROSE_Login_Manager.Services.Implementations`
- Models: `ROSE_Login_Manager.Services.Models`

## Benefits of This Structure

1. **Clear Separation**: Interfaces and implementations are clearly separated
2. **Easy Navigation**: Developers can quickly find what they're looking for
3. **Scalability**: Easy to add new services following the established pattern
4. **Maintainability**: Related code is grouped together logically
5. **Testability**: Interfaces are easily mockable for unit testing

## Migration Notes

When migrating existing code to use the new structure:

1. Update `using` statements to reference the new namespaces
2. Update dependency injection registrations if needed
3. Ensure all interface references point to the `Interfaces` namespace
4. Update any direct service instantiation to use the `Implementations` namespace 
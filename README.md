# Rose Online Login Manager

A secure, user-friendly desktop application for managing Rose Online game accounts and client settings.

## Features

### Implemented
- **User Profile Management**: Create, read, update, and delete game account profiles
- **Secure Data Storage**: Encrypt and store sensitive user data in SQLite database
- **Launcher Settings**: JSON-based configuration for application settings
- **Modern UI**: Built with WPF-UI for a modern, fluent design
- **MVVM Architecture**: Clean separation of concerns using CommunityToolkit.Mvvm
- **Dependency Injection**: Proper service registration and lifecycle management
- **Logging**: Comprehensive logging with Serilog
- **Error Handling**: Robust error handling and user feedback

### In Progress
- **Game Launcher & Auto-Login**: Launch game client and automatically input credentials
- **Game Client Settings**: TOML configuration file management
- **Profile Creation/Editing UI**: Dialog windows for profile management
- **Game Update/Patching**: Basic update checking mechanism

### Planned
- **Advanced Patching**: Differential updates and torrent-based patching
- **Multi-Language Support**: Internationalization
- **Theming Engine**: Flexible theming system
- **System Tray Integration**: Minimize to tray functionality

## Technology Stack

- **Framework**: .NET 9 WPF
- **UI Library**: WPF-UI 4.0.2
- **MVVM**: CommunityToolkit.Mvvm 8.4.0
- **Database**: SQLite with Microsoft.Data.Sqlite
- **ORM**: Dapper for data access
- **Encryption**: Windows DPAPI for secure password storage
- **Configuration**: JSON (launcher settings) and TOML (game settings)
- **Logging**: Serilog with file output
- **Dependency Injection**: Microsoft.Extensions.Hosting

## Architecture

The application follows the MVVM (Model-View-ViewModel) pattern with a layered architecture:

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │    Views    │  │ ViewModels  │  │      Commands       │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                    Business Logic Layer                     │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │   Services  │  │ Validation  │  │   Error Handling    │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                     Data Access Layer                       │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │   SQLite    │  │   JSON      │  │       TOML          │  │
│  │  Database   │  │   Files     │  │      Files          │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

## Project Structure

```
ROSE-Login-Manager/
├── Models/                     # Data models
│   ├── UserProfile.cs         # Game account profile
│   ├── LauncherSettings.cs    # Application settings
│   └── GameClientSettings.cs  # Game configuration
├── Services/                   # Business logic services
│   ├── IEncryptionService.cs  # Password encryption
│   ├── EncryptionService.cs   # DPAPI implementation
│   ├── IDatabaseService.cs    # Database operations
│   ├── DatabaseService.cs     # SQLite implementation
│   ├── IUserProfileService.cs # Profile CRUD operations
│   ├── UserProfileService.cs  # Profile management
│   ├── ILauncherSettingsService.cs # Settings management
│   └── LauncherSettingsService.cs  # JSON settings
├── ViewModels/                 # MVVM ViewModels
│   ├── ViewModelBase.cs       # Base ViewModel class
│   ├── MainViewModel.cs       # Main application logic
│   ├── Pages/                 # Page-specific ViewModels
│   └── Windows/               # Window ViewModels
├── Views/                      # WPF Views
│   ├── Pages/                 # Application pages
│   └── Windows/               # Application windows
└── Resources/                  # Application resources
```

## Getting Started

### Prerequisites
- .NET 9 SDK
- Visual Studio 2022 or later (recommended)
- Windows 10/11

### Building the Application

1. Clone the repository:
```bash
git clone <repository-url>
cd ROSE-Login-Manager
```

2. Open the solution in Visual Studio:
```bash
start "ROSE Login Manager.sln"
```

3. Restore NuGet packages and build:
```bash
dotnet restore
dotnet build
```

4. Run the application:
```bash
dotnet run
```

### Configuration

The application stores data in the following locations:

- **Database**: `%APPDATA%\RoseOnlineLoginManager\LoginManager.db`
- **Settings**: `%APPDATA%\RoseOnlineLoginManager\LauncherSettings.json`
- **Logs**: `%APPDATA%\RoseOnlineLoginManager\logs\app-YYYY-MM-DD.log`

## Security

- **Password Encryption**: Uses Windows DPAPI for machine/user-specific encryption
- **No Hardcoded Secrets**: All sensitive data is encrypted at rest
- **Input Validation**: Comprehensive validation for all user inputs
- **Error Handling**: Sensitive information is never exposed in error messages

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- [WPF-UI](https://github.com/lepoco/wpfui) for the modern UI components
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) for MVVM support
- [Dapper](https://github.com/DapperLib/Dapper) for data access
- [Serilog](https://serilog.net/) for logging 
# SemanticCode

<div align="center">

![SemanticCode Logo](SemanticCode/Assets/favicon.ico)

**A Modern Claude Code Configuration Management Tool**

[![Release](https://img.shields.io/github/v/release/AIDotNet/SemanticCode)](https://github.com/AIDotNet/SemanticCode/releases)
[![Build Status](https://github.com/AIDotNet/SemanticCode/actions/workflows/release.yml/badge.svg)](https://github.com/AIDotNet/SemanticCode/actions)
[![.NET](https://img.shields.io/badge/.NET-10.0-512bd4)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Avalonia](https://img.shields.io/badge/Avalonia-11.3.3-blue)](https://avaloniaui.net/)
[![License](https://img.shields.io/github/license/AIDotNet/SemanticCode)](LICENSE)

[Features](#-features) • [Installation](#-installation) • [Usage](#-usage-guide) • [Development](#-development) • [Contributing](#-contributing)

</div>

## 📖 Overview

SemanticCode is a modern, cross-platform desktop application designed to simplify the management of Claude Code configurations. Built with .NET 10 and Avalonia UI, it provides an intuitive graphical interface for managing API settings, model configurations, and advanced Claude Code parameters without manual JSON editing.

### 🌟 Why SemanticCode?

- **🎯 Specialized**: Purpose-built for Claude Code with deep integration
- **🚀 Modern**: Built on .NET 10 and Avalonia UI for native cross-platform experience
- **⚡ High Performance**: Supports AOT compilation for fast startup and low memory usage
- **🔧 User-Friendly**: Intuitive GUI eliminates manual configuration file editing
- **🛡️ Reliable**: Complete configuration validation and error handling
- **🌐 Agent Hub**: Integrated agent marketplace for discovering and installing AI agents

## ✨ Features

### 🎛️ Configuration Management
- **API Configuration**: Manage Anthropic API keys, base URLs, and authentication settings
- **Model Selection**: Support for latest Claude models including Sonnet 4, Haiku, and more
- **Performance Tuning**: Adjust token limits, temperature values, context sizes, and other parameters
- **Tools Management**: Enable/disable specific Claude Code tools and features
- **Memory Settings**: Configure Claude Code memory persistence and cleanup

### 🤖 Agent Hub Integration
- **Agent Discovery**: Browse and discover AI agents from the community hub
- **One-Click Installation**: Download and install agents directly from the interface
- **Local Agent Management**: View, organize, and manage installed agents
- **Agent Directory**: Automatic management of the `.claude/agents` directory
- **Cache Management**: Intelligent caching system for improved performance

### 🖥️ Modern User Interface
- **Fluent Design**: Based on FluentAvalonia UI with Windows 11-style modern interface
- **Responsive Layout**: Adapts to different screen sizes and resolutions
- **Real-time Feedback**: Configuration changes take effect immediately
- **System Tray Integration**: Background operation with tray icon and quick access menu
- **Multi-language Ready**: Designed for internationalization support

### 🔄 Version Management
- **Auto-Update Check**: Automatically checks for latest versions on GitHub
- **Version Display**: Clear display of current version and available updates
- **Update Notifications**: Built-in update notification system
- **Release Integration**: Seamless integration with GitHub releases

### 🛠️ System Integration
- **Configuration Sync**: Fully compatible with Claude Code native configuration files
- **Directory Management**: Automatic management of `.claude` configuration directory
- **Backup & Recovery**: Support for configuration reset and default value restoration
- **Cross-Platform**: Native support for Windows, Linux, and macOS

## 📥 Installation

### System Requirements

- **Operating System**: Windows 10/11, Linux (Ubuntu 20.04+), macOS 10.15+
- **Runtime**: No .NET Runtime required (self-contained deployment)
- **Memory**: Minimum 512MB RAM
- **Storage**: Approximately 100MB available space

### Download & Install

#### 📦 Pre-built Releases (Recommended)

Download from [Releases](https://github.com/AIDotNet/SemanticCode/releases) page:

**Windows Users:**
```bash
# Download and extract
wget https://github.com/AIDotNet/SemanticCode/releases/latest/download/SemanticCode-windows-x64.zip
unzip SemanticCode-windows-x64.zip

# Run
./SemanticCode.Desktop.exe
```

**Linux Users:**
```bash
# Download and extract
wget https://github.com/AIDotNet/SemanticCode/releases/latest/download/SemanticCode-linux-x64.tar.gz
tar -xzf SemanticCode-linux-x64.tar.gz

# Set permissions and run
chmod +x SemanticCode.Desktop
./SemanticCode.Desktop
```

**macOS Users:**
```bash
# Download and extract
wget https://github.com/AIDotNet/SemanticCode/releases/latest/download/SemanticCode-macos-x64.tar.gz
tar -xzf SemanticCode-macos-x64.tar.gz

# Set permissions and run
chmod +x SemanticCode.Desktop
./SemanticCode.Desktop
```

#### 🔨 Build from Source

```bash
# Clone repository
git clone https://github.com/AIDotNet/SemanticCode.git
cd SemanticCode

# Restore dependencies
dotnet restore

# Build project
dotnet build --configuration Release

# Publish for your platform
dotnet publish SemanticCode.Desktop/SemanticCode.Desktop.csproj \
  -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -o ./publish/win-x64
```

## 🚀 Usage Guide

### Initial Setup

1. **Launch Application**: Run the SemanticCode executable
2. **Navigate to Settings**: Click "Claude Code Settings" in the left menu
3. **Configure API**: Enter your Anthropic API key
4. **Select Models**: Choose appropriate primary and fast models
5. **Adjust Parameters**: Modify token limits and temperature values as needed
6. **Save Settings**: Click save to apply configuration

### Configuration Options

| Setting | Description | Default | Example |
|---------|-------------|---------|---------|
| API Key | Anthropic API authentication key | None | `sk-ant-api03-...` |
| Base URL | API server address | `https://api.anthropic.com` | Support for proxy servers |
| Primary Model | Main conversation model | `claude-sonnet-4-20250514` | Latest Sonnet 4 model |
| Fast Model | Background task model | `claude-3-5-haiku-20241022` | For quick responses |
| Max Tokens | Single request token limit | `4096` | Adjust based on model |
| Temperature | Response randomness control | `0.7` | Range: 0.0-2.0 |
| Debug Mode | Enable detailed logging | `false` | For development debugging |

### Agent Hub Usage

1. **Browse Agents**: Navigate to "Agent Hub" to discover available agents
2. **Install Agents**: Click "Install" on any agent to download and install
3. **Manage Agents**: Use "Agents Management" to view and organize installed agents
4. **Agent Directory**: Agents are automatically saved to `~/.claude/agents/`

### Configuration File Locations
- **Windows**: `%USERPROFILE%\.claude\settings.json`
- **Linux/macOS**: `~/.claude/settings.json`

## 🏗️ Technical Architecture

### Core Technology Stack

- **UI Framework**: [Avalonia UI 11.3.3](https://avaloniaui.net/) - Cross-platform XAML UI framework
- **UI Library**: [FluentAvalonia 2.4.0](https://github.com/amwx/FluentAvalonia) - Fluent Design components
- **Runtime**: [.NET 10.0](https://dotnet.microsoft.com/download/dotnet/10.0) - Latest .NET platform
- **Architecture**: MVVM (Model-View-ViewModel) with ReactiveUI
- **Build System**: MSBuild + GitHub Actions

### Project Structure

```
SemanticCode/
├── SemanticCode/                     # Core UI Library
│   ├── ViewModels/                   # View Models
│   │   ├── MainViewModel.cs          # Main window view model
│   │   ├── ClaudeCodeSettingsViewModel.cs # Settings page
│   │   ├── AgentHubViewModel.cs      # Agent hub functionality
│   │   └── AgentsManagementViewModel.cs # Agent management
│   ├── Views/                        # Views and Windows
│   │   ├── MainView.axaml            # Main interface layout
│   │   ├── MainWindow.axaml          # Application window
│   │   └── SessionHistoryWindow.axaml # Session history
│   ├── Pages/                        # Page Components
│   │   ├── HomeView.axaml            # Home page
│   │   ├── ClaudeCodeSettingsView.axaml # Settings interface
│   │   ├── AgentHubView.axaml        # Agent discovery
│   │   └── AgentsManagementView.axaml # Agent management
│   ├── Services/                     # Business Logic Services
│   │   ├── ClaudeCodeSettingsService.cs # Configuration management
│   │   ├── AgentHubService.cs        # Agent hub integration
│   │   ├── AgentDirectoryService.cs  # Local agent management
│   │   ├── UpdateService.cs          # Update checking
│   │   └── VersionService.cs         # Version management
│   ├── Models/                       # Data Models
│   │   ├── ClaudeCodeSettings.cs     # Configuration models
│   │   ├── AgentModel.cs             # Agent data structures
│   │   ├── AgentHubModels.cs         # Hub response models
│   │   └── ValidationResult.cs       # Validation results
│   └── Assets/                       # Resources
│       └── favicon.ico               # Application icon
├── SemanticCode.Desktop/             # Desktop App Launcher
│   ├── Program.cs                    # Application entry point
│   └── app.manifest                  # Windows app manifest
├── .github/workflows/                # CI/CD Configuration
│   └── release.yml                   # Automated build & release
├── setup/                            # Installation Scripts
│   └── setup.iss                     # Inno Setup script
└── Directory.Packages.props          # Package version management
```

### Design Patterns

#### MVVM Architecture
- **Models**: Data structures and business logic (`ClaudeCodeSettings`, `AgentModel`)
- **Views**: XAML user interfaces (`.axaml` files)
- **ViewModels**: UI logic controllers (`*ViewModel.cs` files)

#### Service Layer Design
- **Configuration Service**: Handles settings file read/write and validation
- **Agent Hub Service**: Manages agent discovery and installation with caching
- **Version Service**: Manages version checking and update notifications
- **Dependency Injection**: Constructor injection for loose coupling

#### Reactive Programming
Built on ReactiveUI for reactive data binding:
```csharp
// Property change notification
public string ApiKey
{
    get => _apiKey;
    set => this.RaiseAndSetIfChanged(ref _apiKey, value);
}

// Command binding with validation
SaveCommand = ReactiveCommand.CreateFromTask(
    SaveSettingsAsync, 
    this.WhenAnyValue(x => x.HasChanges)
);
```

### AOT Compilation Support

SemanticCode supports Ahead-of-Time (AOT) compilation with benefits:

- **Fast Startup**: Eliminates JIT compilation overhead
- **Low Memory Usage**: Reduced runtime memory consumption
- **Native Performance**: Near-native application execution speed
- **Simplified Deployment**: No .NET Runtime installation required

## 🚀 Development

### Development Environment Setup

1. **Install .NET 10 SDK**:
   ```bash
   # Windows (using winget)
   winget install Microsoft.DotNet.SDK.10
   
   # macOS (using Homebrew)
   brew install dotnet
   
   # Linux (Ubuntu)
   sudo apt-get install -y dotnet-sdk-10.0
   ```

2. **Clone and Build**:
   ```bash
   git clone https://github.com/AIDotNet/SemanticCode.git
   cd SemanticCode
   dotnet restore
   dotnet build
   ```

3. **Run Development Version**:
   ```bash
   dotnet run --project SemanticCode.Desktop
   ```

### Development Tools

Recommended development environment:
- **IDE**: Visual Studio 2022, JetBrains Rider, or VS Code
- **Debug Tools**: Avalonia DevTools (integrated)
- **Version Control**: Git
- **Package Management**: NuGet with Central Package Management

### Debugging Tips

1. **Avalonia DevTools**: Press `F12` in Debug mode
2. **Logging**: Use `System.Diagnostics.Debug.WriteLine()`
3. **Breakpoint Debugging**: Standard IDE breakpoint support

### Code Standards

- **Naming Convention**: Follow C# standard naming conventions
- **Code Formatting**: Use EditorConfig for consistent formatting
- **Documentation**: Provide XML documentation comments for public APIs
- **Async Programming**: Prefer `async/await` patterns

## 🤝 Contributing

We welcome contributions of all kinds! Whether it's code contributions, bug reports, or feature suggestions.

### How to Contribute

1. **Fork the Repository**
2. **Create Feature Branch**: `git checkout -b feature/amazing-feature`
3. **Commit Changes**: `git commit -m 'Add amazing feature'`
4. **Push Branch**: `git push origin feature/amazing-feature`
5. **Create Pull Request**

### Development Contribution Guidelines

#### 🐛 Reporting Issues
- Use [Issue Templates](https://github.com/AIDotNet/SemanticCode/issues/new/choose)
- Provide detailed reproduction steps
- Include system environment information
- Add relevant log output

#### 💡 Feature Suggestions
- Describe feature requirements in Issues
- Explain use cases for the feature
- Discuss implementation feasibility

#### 🔧 Code Contributions
- Follow existing code style
- Add necessary unit tests
- Update relevant documentation
- Ensure CI checks pass

### Roadmap

- [ ] **v0.2.0**: Configuration import/export functionality
- [ ] **v0.3.0**: Multi-configuration profile management
- [ ] **v0.4.0**: Usage statistics and analytics integration
- [ ] **v0.5.0**: Plugin system support
- [ ] **v1.0.0**: Full production release

## 📄 License

This project is open source under the [MIT License](LICENSE).

## 🙏 Acknowledgments

- [Avalonia UI](https://avaloniaui.net/) - Excellent cross-platform UI framework
- [FluentAvalonia](https://github.com/amwx/FluentAvalonia) - Beautiful Fluent Design component library
- [ReactiveUI](https://www.reactiveui.net/) - Powerful reactive MVVM framework
- [Anthropic](https://www.anthropic.com/) - Providing powerful Claude AI services

## 📞 Contact

- **GitHub Issues**: [Report issues or suggestions](https://github.com/AIDotNet/SemanticCode/issues)
- **GitHub Discussions**: [Technical discussions and community](https://github.com/AIDotNet/SemanticCode/discussions)

---

<div align="center">

**If this project helps you, please consider giving it a ⭐ Star!**

Made with ❤️ by [AIDotNet](https://github.com/AIDotNet)

</div>
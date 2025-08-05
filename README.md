# SemanticCode

<div align="center">

![SemanticCode Logo](SemanticCode/Assets/avalonia-logo.ico)

**一个现代化的 Claude Code 配置管理工具**

[![Release](https://img.shields.io/github/v/release/AIDotNet/SemanticCode)](https://github.com/AIDotNet/SemanticCode/releases)
[![Build Status](https://github.com/AIDotNet/SemanticCode/actions/workflows/release.yml/badge.svg)](https://github.com/AIDotNet/SemanticCode/actions)
[![.NET](https://img.shields.io/badge/.NET-9.0-512bd4)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![Avalonia](https://img.shields.io/badge/Avalonia-11.3.2-blue)](https://avaloniaui.net/)
[![License](https://img.shields.io/github/license/AIDotNet/SemanticCode)](LICENSE)

[功能特性](#功能特性) • [安装](#安装) • [使用指南](#使用指南) • [技术架构](#技术架构) • [开发](#开发) • [贡献](#贡献)

</div>

## 📖 项目简介

SemanticCode 是一个专为 Claude Code 用户设计的现代化配置管理工具。它提供了直观的图形界面，帮助用户轻松管理 Claude Code 的各种配置选项，包括 API 设置、模型选择、性能参数等。

### 🌟 为什么选择 SemanticCode？

- **🎯 专业化**: 专门为 Claude Code 设计，深度集成其配置体系
- **🚀 现代化**: 基于 .NET 9 和 Avalonia UI，提供原生跨平台体验
- **⚡ 高性能**: 支持 AOT 编译，启动快速，内存占用小
- **🔧 易用性**: 直观的图形界面，无需手动编辑配置文件
- **🛡️ 可靠性**: 完整的配置验证和错误处理机制

## ✨ 功能特性

### 🎛️ 配置管理
- **API 配置**: 管理 Anthropic API 密钥、基础 URL 和认证设置
- **模型选择**: 支持最新的 Claude 模型系列，包括 Sonnet 4、Haiku 等
- **性能调优**: 调节 Token 限制、温度值、上下文大小等参数
- **工具管理**: 启用/禁用特定的 Claude Code 工具功能

### 🖥️ 用户界面
- **现代化设计**: 基于 FluentAvalonia UI，提供类似 Windows 11 的现代界面
- **响应式布局**: 适配不同屏幕尺寸和分辨率
- **实时反馈**: 配置变更即时生效，状态信息实时更新
- **多语言支持**: 完整的中文界面，符合国内用户习惯

### 🔄 版本管理
- **自动更新检查**: 自动检查 GitHub 发布的最新版本
- **版本信息显示**: 清晰显示当前版本和可用更新
- **一键更新**: 简化的更新流程，保持软件最新状态

### 🛠️ 系统集成
- **配置文件同步**: 与 Claude Code 原生配置文件完全兼容
- **目录管理**: 自动管理 `.claude` 配置目录
- **备份恢复**: 支持配置重置和默认值恢复

## 📥 安装

### 系统要求

- **操作系统**: Windows 10/11, Linux (Ubuntu 20.04+), macOS 10.15+
- **运行时**: 无需安装 .NET Runtime（自包含部署）
- **内存**: 最少 512MB RAM
- **存储**: 约 100MB 可用空间

### 下载安装

#### 📦 预编译版本 (推荐)

从 [Releases](https://github.com/AIDotNet/SemanticCode/releases) 页面下载适合您系统的版本：

**Windows 用户:**
```bash
# 下载并解压
wget https://github.com/AIDotNet/SemanticCode/releases/latest/download/SemanticCode-windows-x64.zip
unzip SemanticCode-windows-x64.zip

# 运行
./SemanticCode.Desktop.exe
```

**Linux 用户:**
```bash
# 下载并解压
wget https://github.com/AIDotNet/SemanticCode/releases/latest/download/SemanticCode-linux-x64.tar.gz
tar -xzf SemanticCode-linux-x64.tar.gz

# 设置执行权限并运行
chmod +x SemanticCode.Desktop
./SemanticCode.Desktop
```

#### 🔨 从源码构建

```bash
# 克隆仓库
git clone https://github.com/AIDotNet/SemanticCode.git
cd SemanticCode

# 构建项目
dotnet restore
dotnet build --configuration Release

# 发布
dotnet publish SemanticCode.Desktop/SemanticCode.Desktop.csproj \
  -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -o ./publish
```

## 🚀 使用指南

### 首次配置

1. **启动应用**: 运行 SemanticCode 可执行文件
2. **导航到设置**: 点击左侧菜单中的"Claude Code设置"
3. **配置 API**: 输入您的 Anthropic API 密钥
4. **选择模型**: 选择适合的主要模型和快速模型
5. **调整参数**: 根据需求调整 Token 限制和温度值
6. **保存设置**: 点击保存按钮应用配置

### 配置项说明

| 配置项 | 说明 | 默认值 | 示例 |
|--------|------|--------|------|
| API 密钥 | Anthropic API 认证密钥 | 无 | `sk-ant-api03-...` |
| 基础 URL | API 服务器地址 | `https://api.anthropic.com` | 支持代理服务器 |
| 主要模型 | 主要对话模型 | `claude-sonnet-4-20250514` | 最新 Sonnet 4 模型 |
| 快速模型 | 后台任务模型 | `claude-3-5-haiku-20241022` | 用于快速响应 |
| 最大 Token | 单次请求 Token 限制 | `4096` | 根据模型调整 |
| 温度值 | 回复随机性控制 | `0.7` | 0.0-2.0 之间 |
| 调试模式 | 启用详细日志 | `false` | 开发调试用 |

### 高级功能

#### 🔧 工具管理
SemanticCode 支持管理 Claude Code 的各种工具功能：

- **文件操作工具**: 控制文件读写权限
- **代码执行工具**: 管理代码运行环境
- **网络访问工具**: 配置网络请求权限
- **系统集成工具**: 控制系统级操作

#### 📁 配置文件位置
- **Windows**: `%USERPROFILE%\.claude\settings.json`
- **Linux/macOS**: `~/.claude/settings.json`

#### 🔄 配置同步
SemanticCode 生成的配置文件与官方 Claude Code 完全兼容，可以无缝切换使用。

## 🏗️ 技术架构

### 核心技术栈

- **前端框架**: [Avalonia UI 11.3.2](https://avaloniaui.net/) - 跨平台 XAML UI 框架
- **UI 库**: [FluentAvalonia 2.4.0](https://github.com/amwx/FluentAvalonia) - Fluent Design 风格组件
- **运行时**: [.NET 9.0](https://dotnet.microsoft.com/download/dotnet/9.0) - 最新 .NET 平台
- **架构模式**: MVVM (Model-View-ViewModel) 与 ReactiveUI
- **构建系统**: MSBuild + GitHub Actions

### 项目结构

```
SemanticCode/
├── SemanticCode/                 # 核心 UI 库
│   ├── ViewModels/              # 视图模型层
│   │   ├── MainViewModel.cs     # 主窗口视图模型
│   │   ├── ClaudeCodeSettingsViewModel.cs  # 设置页面视图模型
│   │   └── HomeViewModel.cs     # 首页视图模型
│   ├── Views/                   # 视图层
│   │   ├── MainView.axaml       # 主界面布局
│   │   └── MainWindow.axaml     # 主窗口
│   ├── Pages/                   # 页面组件
│   │   ├── HomeView.axaml       # 首页界面
│   │   └── ClaudeCodeSettingsView.axaml  # 设置界面
│   ├── Services/                # 服务层
│   │   ├── ClaudeCodeSettingsService.cs  # 配置管理服务
│   │   └── VersionService.cs    # 版本检查服务
│   ├── Models/                  # 数据模型
│   │   ├── ClaudeCodeSettings.cs # 配置数据模型
│   │   └── ValidationResult.cs  # 验证结果模型
│   └── Assets/                  # 资源文件
│       └── avalonia-logo.ico    # 应用图标
├── SemanticCode.Desktop/        # 桌面应用启动器
│   ├── Program.cs               # 应用入口点
│   └── app.manifest             # Windows 应用清单
├── .github/workflows/           # CI/CD 配置
│   └── release.yml              # 自动构建发布
└── Directory.Packages.props     # 包版本管理
```

### 设计模式

#### MVVM 架构
- **Model**: 数据模型和业务逻辑 (`ClaudeCodeSettings`, `VersionInfo`)
- **View**: XAML 用户界面 (`.axaml` 文件)
- **ViewModel**: 界面逻辑控制器 (`*ViewModel.cs` 文件)

#### 服务层设计
- **配置服务** (`ClaudeCodeSettingsService`): 处理配置文件的读写和验证
- **版本服务** (`VersionService`): 管理版本检查和更新通知
- **依赖注入**: 使用构造函数注入实现松耦合

#### 响应式编程
基于 ReactiveUI 实现响应式数据绑定：
```csharp
// 属性变更自动通知
public string ApiKey
{
    get => _apiKey;
    set => this.RaiseAndSetIfChanged(ref _apiKey, value);
}

// 命令绑定
SaveCommand = ReactiveCommand.CreateFromTask(
    SaveSettingsAsync, 
    this.WhenAnyValue(x => x.HasChanges)
);
```

### AOT 编译优化

SemanticCode 支持 Ahead-of-Time (AOT) 编译，带来以下优势：

- **快速启动**: 消除 JIT 编译开销
- **小内存占用**: 减少运行时内存使用
- **原生性能**: 接近原生应用的执行效率
- **简化部署**: 无需预装 .NET Runtime

AOT 配置 (`SemanticCode.Desktop.csproj`):
```xml
<PropertyGroup>
    <PublishAot>true</PublishAot>
    <TrimMode>lite</TrimMode>
    <PublishTrimmed>true</PublishTrimmed>
    <JsonSerializerIsReflectionEnabledByDefault>true</JsonSerializerIsReflectionEnabledByDefault>
</PropertyGroup>
```

## 🚀 开发

### 开发环境设置

1. **安装 .NET 9 SDK**:
   ```bash
   # Windows (使用 winget)
   winget install Microsoft.DotNet.SDK.9
   
   # macOS (使用 Homebrew)
   brew install dotnet
   
   # Linux (Ubuntu)
   sudo apt-get install -y dotnet-sdk-9.0
   ```

2. **克隆并构建**:
   ```bash
   git clone https://github.com/AIDotNet/SemanticCode.git
   cd SemanticCode
   dotnet restore
   dotnet build
   ```

3. **运行开发版本**:
   ```bash
   dotnet run --project SemanticCode.Desktop
   ```

### 开发工具

推荐的开发环境：
- **IDE**: Visual Studio 2022, JetBrains Rider, 或 VS Code
- **调试工具**: Avalonia DevTools (已集成)
- **版本控制**: Git
- **包管理**: NuGet (Central Package Management)

### 调试技巧

1. **Avalonia DevTools**: 在 Debug 模式下按 `F12` 打开
2. **日志输出**: 使用 `System.Diagnostics.Debug.WriteLine()`
3. **断点调试**: IDE 中正常设置断点即可

### 代码规范

- **命名约定**: 遵循 C# 标准命名规范
- **代码格式**: 使用 EditorConfig 统一格式
- **注释**: 对公共 API 提供 XML 文档注释
- **异步编程**: 优先使用 `async/await` 模式

## 🤝 贡献

我们欢迎所有形式的贡献！无论是代码贡献、问题报告还是功能建议。

### 贡献方式

1. **Fork 仓库**
2. **创建功能分支**: `git checkout -b feature/amazing-feature`
3. **提交更改**: `git commit -m 'Add some amazing feature'`
4. **推送分支**: `git push origin feature/amazing-feature`
5. **创建 Pull Request**

### 开发贡献指南

#### 🐛 报告问题
- 使用 [Issue 模板](https://github.com/AIDotNet/SemanticCode/issues/new/choose)
- 提供详细的重现步骤
- 包含系统环境信息
- 添加相关的日志输出

#### 💡 功能建议
- 在 Issues 中描述新功能需求
- 说明功能的使用场景
- 讨论实现方案的可行性

#### 🔧 代码贡献
- 遵循现有的代码风格
- 添加必要的单元测试
- 更新相关文档
- 确保 CI 检查通过

### 路线图

- [ ] **v0.2.0**: 添加配置文件导入/导出功能
- [ ] **v0.3.0**: 支持多配置文件管理
- [ ] **v0.4.0**: 集成使用统计和分析
- [ ] **v0.5.0**: 添加插件系统支持
- [ ] **v1.0.0**: 完整的生产版本

## 📄 许可证

本项目基于 [MIT 许可证](LICENSE) 开源。

## 🙏 致谢

- [Avalonia UI](https://avaloniaui.net/) - 出色的跨平台 UI 框架
- [FluentAvalonia](https://github.com/amwx/FluentAvalonia) - 美观的 Fluent Design 组件库
- [ReactiveUI](https://www.reactiveui.net/) - 强大的响应式 MVVM 框架
- [Anthropic](https://www.anthropic.com/) - 提供强大的 Claude AI 服务

## 📞 联系我们

- **GitHub Issues**: [报告问题或建议](https://github.com/AIDotNet/SemanticCode/issues)
- **GitHub Discussions**: [技术讨论和交流](https://github.com/AIDotNet/SemanticCode/discussions)

---

<div align="center">

**如果这个项目对您有帮助，请考虑给它一个 ⭐ Star！**

Made with ❤️ by [AIDotNet](https://github.com/AIDotNet)

</div>
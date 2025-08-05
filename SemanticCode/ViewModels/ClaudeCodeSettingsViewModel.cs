using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using SemanticCode.Models;
using SemanticCode.Services;

namespace SemanticCode.ViewModels;

public class ClaudeCodeSettingsViewModel : ViewModelBase
{
    private ClaudeCodeSettings _settings;
    
    public string Title { get; } = "Claude Code 设置";
    
    // Labels
    public string ApiKeyLabel { get; } = "API 密钥";
    public string BaseUrlLabel { get; } = "API 基础URL";
    public string ModelLabel { get; } = "主要模型";
    public string SmallFastModelLabel { get; } = "后台快速模型";
    public string MaxTokensLabel { get; } = "最大 Token 数";
    public string TemperatureLabel { get; } = "温度";
    public string DebugModeLabel { get; } = "调试模式";
    public string MaxContextTokensLabel { get; } = "最大上下文 Token 数";
    public string MemoryPathLabel { get; } = "记忆文件路径";
    public string DisabledToolsLabel { get; } = "禁用的工具";
    
    // Status
    private string _statusMessage = "正在加载配置...";
    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }
    
    private bool _isLoading = true;
    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }
    
    private bool _hasChanges = false;
    public bool HasChanges
    {
        get => _hasChanges;
        set => this.RaiseAndSetIfChanged(ref _hasChanges, value);
    }
    
    // API Configuration
    private string _apiKey = string.Empty;
    public string ApiKey
    {
        get => _apiKey;
        set
        {
            this.RaiseAndSetIfChanged(ref _apiKey, value);
            HasChanges = true;
        }
    }
    
    private string _baseUrl = "https://api.anthropic.com";
    public string BaseUrl
    {
        get => _baseUrl;
        set
        {
            this.RaiseAndSetIfChanged(ref _baseUrl, value);
            HasChanges = true;
        }
    }
    
    private string _selectedModel = "claude-sonnet-4-20250514";
    public string SelectedModel
    {
        get => _selectedModel;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedModel, value);
            HasChanges = true;
        }
    }
    
    private string _selectedSmallFastModel = "claude-3-5-haiku-20241022";
    public string SelectedSmallFastModel
    {
        get => _selectedSmallFastModel;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedSmallFastModel, value);
            HasChanges = true;
        }
    }
    
    private int _maxTokens = 4096;
    public int MaxTokens
    {
        get => _maxTokens;
        set
        {
            this.RaiseAndSetIfChanged(ref _maxTokens, value);
            HasChanges = true;
        }
    }
    
    private double _temperature = 0.7;
    public double Temperature
    {
        get => _temperature;
        set
        {
            this.RaiseAndSetIfChanged(ref _temperature, value);
            HasChanges = true;
        }
    }
    
    private bool _debugMode = false;
    public bool DebugMode
    {
        get => _debugMode;
        set
        {
            this.RaiseAndSetIfChanged(ref _debugMode, value);
            HasChanges = true;
        }
    }
    
    private int _maxContextTokens = 100000;
    public int MaxContextTokens
    {
        get => _maxContextTokens;
        set
        {
            this.RaiseAndSetIfChanged(ref _maxContextTokens, value);
            HasChanges = true;
        }
    }
    
    private string _memoryPath = string.Empty;
    public string MemoryPath
    {
        get => _memoryPath;
        set
        {
            this.RaiseAndSetIfChanged(ref _memoryPath, value);
            HasChanges = true;
        }
    }
    
    private string _disabledTools = string.Empty;
    public string DisabledTools
    {
        get => _disabledTools;
        set
        {
            this.RaiseAndSetIfChanged(ref _disabledTools, value);
            HasChanges = true;
        }
    }
    
    // Available Options
    public List<string> AvailableModels { get; }
    public List<string> AvailableSmallFastModels { get; }
    public List<string> AvailableBaseUrls { get; }
    
    // Commands
    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> ResetCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenSettingsDirectoryCommand { get; }
    
    public ClaudeCodeSettingsViewModel()
    {
        _settings = new ClaudeCodeSettings();
        
        AvailableModels = ClaudeCodeSettingsService.GetAvailableModels();
        AvailableSmallFastModels = ClaudeCodeSettingsService.GetAvailableSmallFastModels();
        AvailableBaseUrls = new List<string>
        {
            "https://api.anthropic.com",
            "https://api.token-ai.cn",
            "https://api.openai.com/v1",
            "https://api.deepseek.com"
        };
        
        // Initialize commands
        LoadCommand = ReactiveCommand.CreateFromTask(LoadSettingsAsync);
        SaveCommand = ReactiveCommand.CreateFromTask(SaveSettingsAsync, this.WhenAnyValue(x => x.HasChanges));
        ResetCommand = ReactiveCommand.CreateFromTask(ResetSettingsAsync);
        OpenSettingsDirectoryCommand = ReactiveCommand.Create(OpenSettingsDirectory);
        
        // Load settings on initialization
        _ = Task.Run(LoadSettingsAsync);
    }
    
    private async Task LoadSettingsAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "正在加载配置...";
            
            _settings = await ClaudeCodeSettingsService.LoadSettingsAsync();
            
            // Update UI properties
            ApiKey = _settings.Env.AnthropicAuthToken ?? string.Empty;
            BaseUrl = _settings.Env.AnthropicBaseUrl ?? "https://api.anthropic.com";
            SelectedModel = _settings.Env.AnthropicModel ?? "claude-sonnet-4-20250514";
            SelectedSmallFastModel = _settings.Env.AnthropicSmallFastModel ?? "claude-3-5-haiku-20241022";
            MaxTokens = _settings.Env.AnthropicMaxTokens ?? 4096;
            Temperature = _settings.Env.AnthropicTemperature ?? 0.7;
            DebugMode = _settings.Env.ClaudeCodeDebug ?? false;
            MaxContextTokens = _settings.Env.ClaudeCodeMaxContextTokens ?? 100000;
            MemoryPath = _settings.Env.ClaudeCodeMemoryPath ?? string.Empty;
            DisabledTools = _settings.Env.ClaudeCodeToolsDisabled ?? string.Empty;
            
            HasChanges = false;
            StatusMessage = "配置加载完成";
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载配置失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private async Task SaveSettingsAsync()
    {
        try
        {
            StatusMessage = "正在验证配置...";
            
            // Update settings object
            _settings.Env.AnthropicAuthToken = string.IsNullOrWhiteSpace(ApiKey) ? null : ApiKey;
            _settings.Env.AnthropicBaseUrl = string.IsNullOrWhiteSpace(BaseUrl) ? null : BaseUrl;
            _settings.Env.AnthropicModel = string.IsNullOrWhiteSpace(SelectedModel) ? null : SelectedModel;
            _settings.Env.AnthropicSmallFastModel = string.IsNullOrWhiteSpace(SelectedSmallFastModel) ? null : SelectedSmallFastModel;
            _settings.Env.AnthropicMaxTokens = MaxTokens;
            _settings.Env.AnthropicTemperature = Temperature;
            _settings.Env.ClaudeCodeDebug = DebugMode;
            _settings.Env.ClaudeCodeMaxContextTokens = MaxContextTokens;
            _settings.Env.ClaudeCodeMemoryPath = string.IsNullOrWhiteSpace(MemoryPath) ? null : MemoryPath;
            _settings.Env.ClaudeCodeToolsDisabled = string.IsNullOrWhiteSpace(DisabledTools) ? null : DisabledTools;
            
            // 验证配置
            var validation = ClaudeCodeSettingsService.ValidateSettings(_settings);
            
            if (!validation.IsValid)
            {
                StatusMessage = $"配置验证失败: {validation.GetErrorMessage()}";
                return;
            }
            
            StatusMessage = "正在保存配置...";
            await ClaudeCodeSettingsService.SaveSettingsAsync(_settings);
            
            HasChanges = false;
            
            if (validation.HasWarnings)
            {
                StatusMessage = $"配置保存成功 (有警告): {validation.GetWarningMessage()}";
            }
            else
            {
                StatusMessage = "配置保存成功";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"保存配置失败: {ex.Message}";
        }
    }
    
    private async Task ResetSettingsAsync()
    {
        try
        {
            StatusMessage = "正在重置配置...";
            
            _settings = ClaudeCodeSettingsService.CreateDefaultSettings();
            await ClaudeCodeSettingsService.SaveSettingsAsync(_settings);
            await LoadSettingsAsync();
            
            StatusMessage = "配置重置完成";
        }
        catch (Exception ex)
        {
            StatusMessage = $"重置配置失败: {ex.Message}";
        }
    }
    
    private void OpenSettingsDirectory()
    {
        try
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var claudeDir = Path.Combine(userProfile, ".claude");
            
            if (!Directory.Exists(claudeDir))
            {
                Directory.CreateDirectory(claudeDir);
            }
            
            System.Diagnostics.Process.Start("explorer.exe", claudeDir);
        }
        catch (Exception ex)
        {
            StatusMessage = $"打开配置目录失败: {ex.Message}";
        }
    }
}
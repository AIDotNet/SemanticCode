using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using ReactiveUI;
using SemanticCode.Models;
using SemanticCode.Services;
using System.Collections.ObjectModel;

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

    public string BaseUrl
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            HasChanges = true;
        }
    } = "https://api.anthropic.com";

    public string SelectedModel
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            HasChanges = true;
        }
    } = "claude-sonnet-4-20250514";

    public string SelectedSmallFastModel
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            HasChanges = true;
        }
    } = "claude-3-5-haiku-20241022";

    private int? _maxTokens = null;

    public int? MaxTokens
    {
        get => _maxTokens;
        set
        {
            this.RaiseAndSetIfChanged(ref _maxTokens, value);
            HasChanges = true;
        }
    }

    private double? _temperature = null;

    public double? Temperature
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

    private int? _maxContextTokens = null;

    public int? MaxContextTokens
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

    // Profile Management
    private List<ClaudeCodeProfileInfo> _profiles = new();
    private List<ClaudeCodeProfileInfo> _filteredProfiles = new();
    private string _searchText = string.Empty;
    private bool _isProfileDropDownOpen = false;

    public List<ClaudeCodeProfileInfo> Profiles
    {
        get => _profiles;
        private set => this.RaiseAndSetIfChanged(ref _profiles, value);
    }

    public List<ClaudeCodeProfileInfo> FilteredProfiles
    {
        get => _filteredProfiles;
        private set => this.RaiseAndSetIfChanged(ref _filteredProfiles, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchText, value);
            FilterProfiles();
        }
    }

    public bool IsProfileDropDownOpen
    {
        get => _isProfileDropDownOpen;
        set => this.RaiseAndSetIfChanged(ref _isProfileDropDownOpen, value);
    }

    public int TotalProfilesCount => Profiles?.Count ?? 0;
    public int FilteredProfilesCount => FilteredProfiles?.Count ?? 0;

    private ClaudeCodeProfileInfo? _selectedProfile;

    public ClaudeCodeProfileInfo? SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedProfile, value);
            if (value != null)
            {
                Dispatcher.UIThread.InvokeAsync(async () => { await SwitchToProfileAsync(value.Name); });
            }
        }
    }

    private string _newProfileName = string.Empty;

    public string NewProfileName
    {
        get => _newProfileName;
        set => this.RaiseAndSetIfChanged(ref _newProfileName, value);
    }

    private string _newProfileDescription = string.Empty;

    public string NewProfileDescription
    {
        get => _newProfileDescription;
        set => this.RaiseAndSetIfChanged(ref _newProfileDescription, value);
    }

    // Commands
    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> ResetCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenSettingsDirectoryCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadProfilesCommand { get; }
    public ReactiveCommand<Unit, Unit> CreateProfileCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteProfileCommand { get; }
    public ReactiveCommand<Unit, Unit> SetDefaultProfileCommand { get; }
    public ReactiveCommand<Unit, Unit> DuplicateProfileCommand { get; }

    public ClaudeCodeSettingsViewModel()
    {
        _settings = new ClaudeCodeSettings();

        AvailableModels = ClaudeCodeSettingsService.GetAvailableModels();
        AvailableSmallFastModels = ClaudeCodeSettingsService.GetAvailableSmallFastModels();
        AvailableBaseUrls = new List<string>
        {
            "https://api.anthropic.com",
            "https://api.token-ai.cn",
            "https://api.deepseek.com"
        };

        // Initialize commands
        LoadCommand = ReactiveCommand.CreateFromTask(LoadSettingsAsync);
        SaveCommand = ReactiveCommand.CreateFromTask(SaveSettingsAsync, this.WhenAnyValue(x => x.HasChanges));
        ResetCommand = ReactiveCommand.CreateFromTask(ResetSettingsAsync);
        OpenSettingsDirectoryCommand = ReactiveCommand.Create(OpenSettingsDirectory);
        LoadProfilesCommand = ReactiveCommand.CreateFromTask(LoadProfilesAsync);
        CreateProfileCommand = ReactiveCommand.CreateFromTask(CreateProfileAsync,
            this.WhenAnyValue(x => x.NewProfileName).Select(name => !string.IsNullOrWhiteSpace(name)));
        DeleteProfileCommand = ReactiveCommand.CreateFromTask(DeleteProfileAsync,
            this.WhenAnyValue(x => x.SelectedProfile).Select(profile => profile != null && profile.Name != "default"));
        SetDefaultProfileCommand = ReactiveCommand.CreateFromTask(SetDefaultProfileAsync,
            this.WhenAnyValue(x => x.SelectedProfile).Select(profile => profile != null && !profile.IsDefault));
        DuplicateProfileCommand = ReactiveCommand.CreateFromTask(DuplicateProfileAsync,
            this.WhenAnyValue(x => x.SelectedProfile).Select(profile => profile != null));

        // Load settings on initialization
        _ = Task.Run(async () =>
        {
            await LoadProfilesAsync();
            await LoadSettingsAsync();
        });
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
            BaseUrl = _settings.Env.AnthropicBaseUrl;
            SelectedModel = _settings.Env.AnthropicModel ?? "claude-sonnet-4-20250514";
            SelectedSmallFastModel = _settings.Env.AnthropicSmallFastModel ?? "claude-3-5-haiku-20241022";
            MaxTokens = _settings.Env.AnthropicMaxTokens;
            Temperature = _settings.Env.AnthropicTemperature;
            DebugMode = _settings.Env.ClaudeCodeDebug ?? false;
            MaxContextTokens = _settings.Env.ClaudeCodeMaxContextTokens;
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
            _settings.Env.AnthropicSmallFastModel =
                string.IsNullOrWhiteSpace(SelectedSmallFastModel) ? null : SelectedSmallFastModel;
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

    private async Task LoadProfilesAsync()
    {
        try
        {
            StatusMessage = "正在加载配置档案...";
            Profiles = await ClaudeCodeProfileService.GetAllProfilesAsync();

            if (SelectedProfile == null && Profiles.Count > 0)
            {
                var currentProfileName = await GetCurrentProfileNameAsync();
                SelectedProfile = Profiles.FirstOrDefault(p => p.Name == currentProfileName) ?? Profiles.First();
            }

            StatusMessage = "配置档案加载完成";
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载配置档案失败: {ex.Message}";
        }
    }

    private async Task<string> GetCurrentProfileNameAsync()
    {
        try
        {
            var manager = await ClaudeCodeProfileService.LoadProfileManagerAsync();
            return manager.CurrentProfile;
        }
        catch
        {
            return "default";
        }
    }

    private async Task SwitchToProfileAsync(string profileName)
    {
        try
        {
            StatusMessage = "正在切换配置档案...";

            var profile = await ClaudeCodeProfileService.LoadProfileAsync(profileName);
            _settings = profile.Settings;

            // Update UI properties
            ApiKey = _settings.Env.AnthropicAuthToken ?? string.Empty;

            BaseUrl = _settings.Env.AnthropicBaseUrl;

            SelectedModel = _settings.Env.AnthropicModel ?? "claude-sonnet-4-20250514";
            SelectedSmallFastModel = _settings.Env.AnthropicSmallFastModel ?? "claude-3-5-haiku-20241022";
            MaxTokens = _settings.Env.AnthropicMaxTokens;
            Temperature = _settings.Env.AnthropicTemperature;
            DebugMode = _settings.Env.ClaudeCodeDebug ?? false;
            MaxContextTokens = _settings.Env.ClaudeCodeMaxContextTokens;
            MemoryPath = _settings.Env.ClaudeCodeMemoryPath ?? string.Empty;
            DisabledTools = _settings.Env.ClaudeCodeToolsDisabled ?? string.Empty;

            await ClaudeCodeProfileService.SetCurrentProfileAsync(profileName);
            HasChanges = false;
            StatusMessage = $"已切换到配置档案: {profileName}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"切换配置档案失败: {ex.Message}";
        }
    }

    private async Task CreateProfileAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NewProfileName))
            {
                StatusMessage = "请输入配置档案名称";
                return;
            }

            if (Profiles.Any(p => p.Name.Equals(NewProfileName, StringComparison.OrdinalIgnoreCase)))
            {
                StatusMessage = "配置档案名称已存在";
                return;
            }

            StatusMessage = "正在创建配置档案...";

            // Update settings object with current values
            _settings.Env.AnthropicAuthToken = string.IsNullOrWhiteSpace(ApiKey) ? null : ApiKey;
            _settings.Env.AnthropicBaseUrl = string.IsNullOrWhiteSpace(BaseUrl) ? null : BaseUrl;
            _settings.Env.AnthropicModel = string.IsNullOrWhiteSpace(SelectedModel) ? null : SelectedModel;
            _settings.Env.AnthropicSmallFastModel =
                string.IsNullOrWhiteSpace(SelectedSmallFastModel) ? null : SelectedSmallFastModel;
            _settings.Env.AnthropicMaxTokens = MaxTokens;
            _settings.Env.AnthropicTemperature = Temperature;
            _settings.Env.ClaudeCodeDebug = DebugMode;
            _settings.Env.ClaudeCodeMaxContextTokens = MaxContextTokens;
            _settings.Env.ClaudeCodeMemoryPath = string.IsNullOrWhiteSpace(MemoryPath) ? null : MemoryPath;
            _settings.Env.ClaudeCodeToolsDisabled = string.IsNullOrWhiteSpace(DisabledTools) ? null : DisabledTools;

            var newProfile = await ClaudeCodeProfileService.CreateProfileAsync(
                NewProfileName,
                NewProfileDescription,
                CloneSettings(_settings)
            );

            await LoadProfilesAsync();
            SelectedProfile = Profiles.FirstOrDefault(p => p.Name == NewProfileName);

            NewProfileName = string.Empty;
            NewProfileDescription = string.Empty;
            StatusMessage = $"配置档案 '{NewProfileName}' 创建成功";
        }
        catch (Exception ex)
        {
            StatusMessage = $"创建配置档案失败: {ex.Message}";
        }
    }

    private async Task DeleteProfileAsync()
    {
        try
        {
            if (SelectedProfile == null || SelectedProfile.Name == "default")
            {
                StatusMessage = "无法删除默认配置档案";
                return;
            }

            StatusMessage = "正在删除配置档案...";

            var success = await ClaudeCodeProfileService.DeleteProfileAsync(SelectedProfile.Name);
            if (success)
            {
                await LoadProfilesAsync();
                StatusMessage = $"配置档案 '{SelectedProfile.Name}' 删除成功";
            }
            else
            {
                StatusMessage = $"删除配置档案失败";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"删除配置档案失败: {ex.Message}";
        }
    }

    private async Task SetDefaultProfileAsync()
    {
        try
        {
            if (SelectedProfile == null)
            {
                StatusMessage = "请选择要设为默认的配置档案";
                return;
            }

            StatusMessage = "正在设置默认配置档案...";

            var success = await ClaudeCodeProfileService.SetDefaultProfileAsync(SelectedProfile.Name);
            if (success)
            {
                await LoadProfilesAsync();
                StatusMessage = $"配置档案 '{SelectedProfile.Name}' 已设为默认";
            }
            else
            {
                StatusMessage = $"设置默认配置档案失败";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"设置默认配置档案失败: {ex.Message}";
        }
    }

    private async Task DuplicateProfileAsync()
    {
        try
        {
            if (SelectedProfile == null)
            {
                StatusMessage = "请选择要复制的配置档案";
                return;
            }

            var duplicateName = $"{SelectedProfile.Name}_copy";
            var counter = 1;
            while (Profiles.Any(p => p.Name.Equals(duplicateName, StringComparison.OrdinalIgnoreCase)))
            {
                duplicateName = $"{SelectedProfile.Name}_copy_{counter++}";
            }

            StatusMessage = "正在复制配置档案...";

            var newProfile = await ClaudeCodeProfileService.DuplicateProfileAsync(
                SelectedProfile.Name,
                duplicateName,
                $"复制自 {SelectedProfile.Name}"
            );

            await LoadProfilesAsync();
            SelectedProfile = Profiles.FirstOrDefault(p => p.Name == duplicateName);
            StatusMessage = SelectedProfile != null ? $"配置档案 '{SelectedProfile.Name}' 复制成功" : "配置档案复制成功";
        }
        catch (Exception ex)
        {
            StatusMessage = $"复制配置档案失败: {ex.Message}";
        }
    }

    private void FilterProfiles()
    {
        if (Profiles == null)
        {
            FilteredProfiles = new List<ClaudeCodeProfileInfo>();
            return;
        }

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredProfiles = new List<ClaudeCodeProfileInfo>(Profiles);
        }
        else
        {
            var searchTerm = SearchText.ToLowerInvariant();
            FilteredProfiles = Profiles.Where(p => 
                p.Name.ToLowerInvariant().Contains(searchTerm) || 
                p.Description.ToLowerInvariant().Contains(searchTerm)
            ).ToList();
        }
        
        this.RaisePropertyChanged(nameof(FilteredProfilesCount));
    }

    private ClaudeCodeSettings CloneSettings(ClaudeCodeSettings source)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(source);
        return System.Text.Json.JsonSerializer.Deserialize<ClaudeCodeSettings>(json) ?? new ClaudeCodeSettings();
    }
}

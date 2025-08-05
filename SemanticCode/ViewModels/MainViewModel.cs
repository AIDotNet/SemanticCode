using ReactiveUI;
using SemanticCode.Services;
using System.Threading.Tasks;

namespace SemanticCode.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly VersionService _versionService;
    
    private VersionInfo? _versionInfo;

    public VersionInfo? VersionInfo
    {
        get => _versionInfo;
        set => this.RaiseAndSetIfChanged(ref _versionInfo, value);
    }

    public MainViewModel()
    {
        _versionService = new VersionService();
        
        // 异步检查版本更新，不阻塞UI
        Task.Run(async () =>
        {
            VersionInfo = await _versionService.CheckForUpdatesAsync();
        });
    }
}
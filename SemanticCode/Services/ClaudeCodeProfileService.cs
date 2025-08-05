using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SemanticCode.Models;

namespace SemanticCode.Services;

public class ClaudeCodeProfileService
{
    private static readonly JsonSerializerOptions JsonOptions = new(AppSettingsContext.Default.Options)
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static string GetProfilesDirectory()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(userProfile, ".claude", "profiles");
    }

    private static string GetProfileManagerPath()
    {
        return Path.Combine(GetProfilesDirectory(), "profile-manager.json");
    }

    private static string GetProfileFilePath(string profileName)
    {
        return Path.Combine(GetProfilesDirectory(), $"{profileName}.json");
    }

    public static async Task<ProfileManager> LoadProfileManagerAsync()
    {
        try
        {
            var managerPath = GetProfileManagerPath();

            if (!File.Exists(managerPath))
            {
                var defaultManager = CreateDefaultProfileManager();
                await SaveProfileManagerAsync(defaultManager);
                return defaultManager;
            }

            var json = await File.ReadAllTextAsync(managerPath);
            var manager = JsonSerializer.Deserialize<ProfileManager>(json, JsonOptions);

            return manager ?? CreateDefaultProfileManager();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading profile manager: {ex.Message}");
            return CreateDefaultProfileManager();
        }
    }

    public static async Task SaveProfileManagerAsync(ProfileManager manager)
    {
        try
        {
            var profilesDir = GetProfilesDirectory();

            if (!Directory.Exists(profilesDir))
            {
                Directory.CreateDirectory(profilesDir);
            }

            var managerPath = GetProfileManagerPath();
            var json = JsonSerializer.Serialize(manager, JsonOptions);

            await File.WriteAllTextAsync(managerPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving profile manager: {ex.Message}");
            throw;
        }
    }

    public static async Task<ClaudeCodeProfile> LoadProfileAsync(string profileName)
    {
        try
        {
            var profilePath = GetProfileFilePath(profileName);

            if (!File.Exists(profilePath))
            {
                if (profileName == "default")
                {
                    var defaultProfile = await CreateDefaultProfileAsync();
                    return defaultProfile;
                }
                else
                {
                    throw new FileNotFoundException($"Profile '{profileName}' not found");
                }
            }

            var json = await File.ReadAllTextAsync(profilePath);
            var profile = JsonSerializer.Deserialize<ClaudeCodeProfile>(json, JsonOptions);

            return profile ?? throw new InvalidOperationException($"Failed to deserialize profile '{profileName}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading profile '{profileName}': {ex.Message}");
            throw;
        }
    }

    public static async Task SaveProfileAsync(ClaudeCodeProfile profile)
    {
        try
        {
            var profilesDir = GetProfilesDirectory();

            if (!Directory.Exists(profilesDir))
            {
                Directory.CreateDirectory(profilesDir);
            }

            var profilePath = GetProfileFilePath(profile.Name);
            profile.UpdatedAt = DateTime.UtcNow;
            
            var json = JsonSerializer.Serialize(profile, JsonOptions);
            await File.WriteAllTextAsync(profilePath, json);

            var manager = await LoadProfileManagerAsync();
            
            var existingProfile = manager.Profiles.FirstOrDefault(p => p.Name == profile.Name);
            if (existingProfile != null)
            {
                existingProfile.Description = profile.Description;
                existingProfile.IsDefault = profile.IsDefault;
                existingProfile.UpdatedAt = profile.UpdatedAt;
            }
            else
            {
                manager.Profiles.Add(new ClaudeCodeProfileInfo
                {
                    Name = profile.Name,
                    Description = profile.Description,
                    IsDefault = profile.IsDefault,
                    CreatedAt = profile.CreatedAt,
                    UpdatedAt = profile.UpdatedAt
                });
            }

            await SaveProfileManagerAsync(manager);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving profile '{profile.Name}': {ex.Message}");
            throw;
        }
    }

    public static async Task<bool> DeleteProfileAsync(string profileName)
    {
        try
        {
            if (profileName == "default")
            {
                return false;
            }

            var profilePath = GetProfileFilePath(profileName);
            var manager = await LoadProfileManagerAsync();

            var profile = manager.Profiles.FirstOrDefault(p => p.Name == profileName);
            if (profile == null)
            {
                return false;
            }

            manager.Profiles.Remove(profile);
            
            if (manager.CurrentProfile == profileName)
            {
                manager.CurrentProfile = "default";
            }

            if (File.Exists(profilePath))
            {
                File.Delete(profilePath);
            }

            await SaveProfileManagerAsync(manager);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting profile '{profileName}': {ex.Message}");
            return false;
        }
    }

    public static async Task<List<ClaudeCodeProfileInfo>> GetAllProfilesAsync()
    {
        try
        {
            var manager = await LoadProfileManagerAsync();
            return manager.Profiles.OrderBy(p => p.Name).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting all profiles: {ex.Message}");
            return new List<ClaudeCodeProfileInfo>();
        }
    }

    public static async Task<ClaudeCodeProfile> GetCurrentProfileAsync()
    {
        try
        {
            var manager = await LoadProfileManagerAsync();
            return await LoadProfileAsync(manager.CurrentProfile);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting current profile: {ex.Message}");
            var defaultProfile = await LoadProfileAsync("default");
            var manager = await LoadProfileManagerAsync();
            manager.CurrentProfile = "default";
            await SaveProfileManagerAsync(manager);
            return defaultProfile;
        }
    }

    public static async Task<bool> SetCurrentProfileAsync(string profileName)
    {
        try
        {
            var manager = await LoadProfileManagerAsync();
            
            if (!manager.Profiles.Any(p => p.Name == profileName))
            {
                return false;
            }

            manager.CurrentProfile = profileName;
            await SaveProfileManagerAsync(manager);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting current profile to '{profileName}': {ex.Message}");
            return false;
        }
    }

    public static async Task<bool> SetDefaultProfileAsync(string profileName)
    {
        try
        {
            var manager = await LoadProfileManagerAsync();
            
            var profile = manager.Profiles.FirstOrDefault(p => p.Name == profileName);
            if (profile == null)
            {
                return false;
            }

            foreach (var p in manager.Profiles)
            {
                p.IsDefault = false;
            }

            profile.IsDefault = true;
            
            if (manager.CurrentProfile == "default")
            {
                manager.CurrentProfile = profileName;
            }

            await SaveProfileManagerAsync(manager);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting default profile to '{profileName}': {ex.Message}");
            return false;
        }
    }

    public static async Task<ClaudeCodeProfile> CreateProfileAsync(string name, string description, ClaudeCodeSettings settings)
    {
        try
        {
            var profile = new ClaudeCodeProfile
            {
                Name = name,
                Description = description,
                Settings = settings,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await SaveProfileAsync(profile);
            return profile;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating profile '{name}': {ex.Message}");
            throw;
        }
    }

    public static async Task<ClaudeCodeProfile> DuplicateProfileAsync(string sourceProfileName, string newProfileName, string description)
    {
        try
        {
            var sourceProfile = await LoadProfileAsync(sourceProfileName);
            
            var newProfile = new ClaudeCodeProfile
            {
                Name = newProfileName,
                Description = description,
                Settings = CloneSettings(sourceProfile.Settings),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await SaveProfileAsync(newProfile);
            return newProfile;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error duplicating profile '{sourceProfileName}' to '{newProfileName}': {ex.Message}");
            throw;
        }
    }

    private static ProfileManager CreateDefaultProfileManager()
    {
        var manager = new ProfileManager
        {
            CurrentProfile = "default",
            Profiles = new List<ClaudeCodeProfileInfo>
            {
                new ClaudeCodeProfileInfo
                {
                    Name = "default",
                    Description = "默认配置",
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            }
        };

        return manager;
    }
    
    private static async Task<ClaudeCodeProfile> CreateDefaultProfileAsync()
    {
        try
        {
            var defaultSettings = ClaudeCodeSettingsService.CreateDefaultSettings();
            var defaultProfile = new ClaudeCodeProfile
            {
                Name = "default",
                Description = "默认配置",
                Settings = defaultSettings,
                IsDefault = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            await SaveProfileAsync(defaultProfile);
            return defaultProfile;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating default profile: {ex.Message}");
            throw;
        }
    }

    private static ClaudeCodeSettings CloneSettings(ClaudeCodeSettings source)
    {
        var json = JsonSerializer.Serialize(source, JsonOptions);
        return JsonSerializer.Deserialize<ClaudeCodeSettings>(json, JsonOptions) ?? new ClaudeCodeSettings();
    }
}
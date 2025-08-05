using System;
using System.Collections.Generic;

namespace SemanticCode.Models;

public class UpdateInfo
{
    public string Version { get; set; } = string.Empty;
    public string ReleaseUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public bool IsNewerVersion { get; set; }
    public List<UpdateAsset> Assets { get; set; } = new();
}

public class UpdateAsset
{
    public string Name { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public long Size { get; set; }
}
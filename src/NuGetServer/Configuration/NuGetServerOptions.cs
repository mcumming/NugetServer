namespace NuGetServer.Configuration;

public sealed class NuGetServerOptions
{
    public const string SectionName = "NuGetServer";

    public string PackagesPath { get; set; } = "/packages";
    public string ApiKey { get; set; } = string.Empty;
    public bool AllowOverwrite { get; set; } = false;
    public bool EnableDelisting { get; set; } = true;
    public int MaxPackageSizeMB { get; set; } = 250;
}

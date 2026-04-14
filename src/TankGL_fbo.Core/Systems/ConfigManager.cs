using System.IO;
using System.Text.Json;
using TankGL_fbo.Core.Contracts;

namespace TankGL_fbo.Core.Systems;

public static class ConfigManager
{
    private static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, "config.json");
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static GameConfig Config { get; internal set; } = new();

    public static void Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                string json = File.ReadAllText(ConfigPath);
                Config = JsonSerializer.Deserialize<GameConfig>(json, _jsonOptions) ?? new GameConfig();
            }
            else
            {
                Save(); 
            }
        }
        catch
        {
            Config = new GameConfig();
            Save();
        }
    }

    public static void Save()
    {
        try
        {
            string json = JsonSerializer.Serialize(Config, _jsonOptions);
            File.WriteAllText(ConfigPath, json);
        }
        catch
        {
            
        }
    }
}
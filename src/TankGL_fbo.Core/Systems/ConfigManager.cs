using System;
using System.IO;
using System.Text.Json;
using TankGL_fbo.Core.Contracts;

namespace TankGL_fbo.Core.Systems;

/// <summary>
/// Статический менеджер конфигурации игры.
/// Отвечает за загрузку, сохранение и уведомление об изменении настроек приложения.
/// Данные хранятся в формате JSON в файле config.json рядом с исполняемым файлом.
/// </summary>
public static class ConfigManager
{
    /// <summary>Полный путь к файлу конфигурации.</summary>
    private static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, "config.json");

    /// <summary>Настройки сериализации JSON (отступы, camelCase для имен свойств).</summary>
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>Текущий объект конфигурации игры.</summary>
    public static GameConfig Config { get; internal set; } = new();

    /// <summary>Событие, вызываемое после успешного сохранения конфигурации на диск.</summary>
    public static event Action? ConfigSaved;

    /// <summary>Событие, вызываемое при изменении размера шрифта меню.</summary>
    public static event Action<int>? MenuFontSizeChanged;

    /// <summary>
    /// Принудительно вызывает событие изменения размера шрифта меню.
    /// </summary>
    /// <param name="newSize">Новый размер шрифта.</param>
    public static void NotifyMenuFontSizeChanged(int newSize) => MenuFontSizeChanged?.Invoke(newSize);

    /// <summary>
    /// Загружает конфигурацию из файла. Если файл отсутствует или поврежден,
    /// создает конфигурацию по умолчанию и сохраняет её.
    /// </summary>
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

    /// <summary>
    /// Сохраняет текущую конфигурацию в файл JSON.
    /// При успешном сохранении вызывает событие <see cref="ConfigSaved"/>.
    /// </summary>
    public static void Save()
    {
        try
        {
            string json = JsonSerializer.Serialize(Config, _jsonOptions);
            File.WriteAllText(ConfigPath, json);
            ConfigSaved?.Invoke();
        }
        catch { }
    }
}
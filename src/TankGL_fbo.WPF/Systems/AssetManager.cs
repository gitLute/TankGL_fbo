using System.Diagnostics;
using System.IO;

namespace TankGL_fbo.WPF.Systems;

/// <summary>
/// Менеджер ресурсов приложения.
/// Отвечает за загрузку, кэширование и предоставление доступа к шейдерам и текстурам.
/// Также предоставляет общий экземпляр геометрии квада для отрисовки спрайтов.
/// </summary>
public sealed class AssetManager(string basePath) : IDisposable
{
    /// <summary>Базовый путь к директории ресурсов (Assets).</summary>
    private readonly string _basePath = basePath;
    /// <summary>Кэш загруженных шейдеров по имени.</summary>
    private readonly Dictionary<string, Shader> _shaders = [];
    /// <summary>Кэш загруженных текстур по имени файла.</summary>
    private readonly Dictionary<string, Texture2D> _textures = [];

    /// <summary>Общий экземпляр геометрии квада для отрисовки спрайтов.</summary>
    public UnitQuad Quad { get; private set; } = null!;

    /// <summary>
    /// Инициализирует менеджер: создает квад и загружает базовый шейдер.
    /// </summary>
    public void Init()
    {
        Quad = new UnitQuad();
        LoadShader("default", "Shaders/default_vert.glsl", "Shaders/default_frag.glsl");
    }

    /// <summary>
    /// Загружает и компилирует шейдер из файлов по относительным путям.
    /// </summary>
    /// <param name="name">Имя шейдера для хранения в кэше.</param>
    /// <param name="vertRel">Относительный путь к вершинному шейдеру.</param>
    /// <param name="fragRel">Относительный путь к фрагментному шейдеру.</param>
    private void LoadShader(string name, string vertRel, string fragRel)
    {
        string vPath = Path.Combine(_basePath, vertRel);
        string fPath = Path.Combine(_basePath, fragRel);
        if (File.Exists(vPath) && File.Exists(fPath)) _shaders[name] = Shader.FromFiles(vPath, fPath);
    }

    /// <summary>
    /// Возвращает шейдер по имени из кэша. Если не найден, возвращает первый доступный или выбрасывает исключение.
    /// </summary>
    /// <param name="name">Имя шейдера.</param>
    /// <returns>Экземпляр шейдера.</returns>
    public Shader GetShader(string name)
    {
        if (_shaders.TryGetValue(name, out var s)) return s;
        return _shaders.Values.FirstOrDefault() ?? throw new Exception("No shaders loaded.");
    }

    /// <summary>
    /// Загружает текстуру из файла. Если файл не найден, пытается загрузить текстуру-заглушку (missing.png).
    /// Результат кэшируется для повторного использования.
    /// </summary>
    /// <param name="name">Имя файла текстуры.</param>
    /// <returns>Экземпляр загруженной текстуры.</returns>
    public Texture2D LoadTexture(string name)
    {
        string path = Path.Combine(_basePath, "Textures", name);
        if (!File.Exists(path))
        {
            string missingPath = Path.Combine(_basePath, "Textures", "missing.png");
            if (!File.Exists(missingPath)) throw new FileNotFoundException($"Ни основной файл ({name}), ни резервный (missing.png) не найдены.");
            path = missingPath;
        }
        if (_textures.TryGetValue(name, out var t)) return t;
        if (!File.Exists(path))
        {
            return _textures.Values.FirstOrDefault()!;
        }
        var tex = Texture2D.FromPath(path);
        _textures[name] = tex;
        return tex;
    }

    /// <summary>
    /// Освобождает все загруженные ресурсы (шейдеры, текстуры, квад) и очищает кэши.
    /// </summary>
    public void Dispose()
    {
        Quad?.Dispose();
        foreach (var s in _shaders.Values) s.Dispose();
        foreach (var t in _textures.Values) t.Dispose();
        _shaders.Clear();
        _textures.Clear();
    }
}
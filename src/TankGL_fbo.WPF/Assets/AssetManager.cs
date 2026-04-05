using System.Diagnostics;
using System.IO;

namespace TankGL_fbo.WPF.Assets;

public sealed class AssetManager : IDisposable
{
    private readonly string _basePath;
    private readonly Dictionary<string, Shader> _shaders = [];
    private readonly Dictionary<string, Texture2D> _textures = [];

    public UnitQuad Quad { get; private set; } = null!;

    public AssetManager(string basePath) => _basePath = basePath;

    public void Init()
    {
        Quad = new UnitQuad();
        LoadShader("default", "Shaders/default_vert.glsl", "Shaders/default_frag.glsl");
    }

    private void LoadShader(string name, string vertRel, string fragRel)
    {
        string vPath = Path.Combine(_basePath, vertRel);
        string fPath = Path.Combine(_basePath, fragRel);
        if (File.Exists(vPath) && File.Exists(fPath)) _shaders[name] = Shader.FromFiles(vPath, fPath);
        else Debug.WriteLine($"[AssetManager] Missing shader files for '{name}'");
    }

    public Shader GetShader(string name)
    {
        if (_shaders.TryGetValue(name, out var s)) return s;
        Debug.WriteLine($"[AssetManager] Shader '{name}' not found!");
        return _shaders.Values.FirstOrDefault() ?? throw new Exception("No shaders loaded.");
    }

    public Texture2D LoadTexture(string name)
    {
        if (_textures.TryGetValue(name, out var t)) return t;

        string path = Path.Combine(_basePath, "Textures", name);
        if (!File.Exists(path))
        {
            Debug.WriteLine($"[AssetManager] Missing texture: {path}");
            return _textures.Values.FirstOrDefault()!;
        }

        var tex = Texture2D.FromPath(path);
        _textures[name] = tex;
        return tex;
    }

    public void Dispose()
    {
        Quad?.Dispose();
        foreach (var s in _shaders.Values) s.Dispose();
        foreach (var t in _textures.Values) t.Dispose();
        _shaders.Clear();
        _textures.Clear();
    }
}
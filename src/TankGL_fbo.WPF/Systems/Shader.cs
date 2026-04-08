using System.Diagnostics;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace TankGL_fbo.WPF.Systems;

public sealed class Shader : IDisposable
{
    public int Handle { get; private set; }
    private readonly Dictionary<string, int> _uniformLocations = [];
    private bool _disposed;

    public static Shader FromFiles(string vertPath, string fragPath) => new(File.ReadAllText(vertPath), File.ReadAllText(fragPath));

    private Shader(string vertSource, string fragSource) => Init(vertSource, fragSource);

    private void Init(string vertSource, string fragSource)
    {
        int vs = CompileShader(ShaderType.VertexShader, vertSource);
        int fs = CompileShader(ShaderType.FragmentShader, fragSource);

        Handle = GL.CreateProgram();
        GL.AttachShader(Handle, vs);
        GL.AttachShader(Handle, fs);
        GL.LinkProgram(Handle);
        CheckLog(Handle);

        GL.DetachShader(Handle, vs);
        GL.DetachShader(Handle, fs);
        GL.DeleteShader(vs);
        GL.DeleteShader(fs);
        CacheUniforms();
    }

    private int CompileShader(ShaderType type, string source)
    {
        int id = GL.CreateShader(type);
        GL.ShaderSource(id, source);
        GL.CompileShader(id);
        GL.GetShader(id, ShaderParameter.CompileStatus, out int ok);
        if (ok == 0) Debug.WriteLine($"[Shader] Compile error: {GL.GetShaderInfoLog(id)}");
        return id;
    }

    private void CheckLog(int program)
    {
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int ok);
        if (ok == 0) Debug.WriteLine($"[Shader] Link error: {GL.GetProgramInfoLog(program)}");
    }

    private void CacheUniforms()
    {
        GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out int count);
        for (int i = 0; i < count; i++)
        {
            string name = GL.GetActiveUniform(Handle, i, out _, out _);
            _uniformLocations[name] = GL.GetUniformLocation(Handle, name);
        }
    }

    public void Use() => GL.UseProgram(Handle);

    public void SetMatrix4(string name, Matrix4 matrix)
    {
        if (_uniformLocations.TryGetValue(name, out int loc) && loc != -1) GL.UniformMatrix4(loc, false, ref matrix);
    }

    public void SetInt(string name, int value)
    {
        if (_uniformLocations.TryGetValue(name, out int loc) && loc != -1) GL.Uniform1(loc, value);
    }

    public void SetFloat(string name, float value)
    {
        if (_uniformLocations.TryGetValue(name, out int loc) && loc != -1) GL.Uniform1(loc, value);
    }

    public void Dispose()
    {
        if (_disposed) return;
        GL.DeleteProgram(Handle);
        _disposed = true;
        GC.SuppressFinalize(this);
    }
    ~Shader() => Dispose();
}
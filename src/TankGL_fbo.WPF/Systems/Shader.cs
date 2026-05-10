using System.Diagnostics;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace TankGL_fbo.WPF.Systems;

/// <summary>
/// Управление шейдерной программой OpenGL.
/// Отвечает за компиляцию вершинного и фрагментного шейдеров, линковку программы,
/// кэширование расположений uniform-переменных и их безопасную установку.
/// </summary>
public sealed class Shader : IDisposable
{
    /// <summary>Идентификатор шейдерной программы в OpenGL.</summary>
    public int Handle { get; private set; }

    /// <summary>Кэш расположений uniform-переменных для быстрого доступа по имени.</summary>
    private readonly Dictionary<string, int> _uniformLocations = [];
    private bool _disposed;

    /// <summary>
    /// Создает шейдер из файлов с вершинным и фрагментным кодом.
    /// </summary>
    /// <param name="vertPath">Путь к файлу вершинного шейдера (.glsl).</param>
    /// <param name="fragPath">Путь к файлу фрагментного шейдера (.glsl).</param>
    /// <returns>Экземпляр скомпилированного и слинкованного шейдера.</returns>
    public static Shader FromFiles(string vertPath, string fragPath) => new(File.ReadAllText(vertPath), File.ReadAllText(fragPath));

    private Shader(string vertSource, string fragSource) => Init(vertSource, fragSource);

    /// <summary>
    /// Инициализирует шейдер: компилирует исходники, линкует программу, проверяет ошибки и кэширует uniform-переменные.
    /// </summary>
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

    /// <summary>
    /// Компилирует шейдер указанного типа из исходного кода.
    /// </summary>
    /// <param name="type">Тип шейдера (вершинный, фрагментный и т.д.).</param>
    /// <param name="source">Исходный код шейдера на GLSL.</param>
    /// <returns>Идентификатор скомпилированного шейдера.</returns>
    private int CompileShader(ShaderType type, string source)
    {
        int id = GL.CreateShader(type);
        GL.ShaderSource(id, source);
        GL.CompileShader(id);
        GL.GetShader(id, ShaderParameter.CompileStatus, out int ok);
        return id;
    }

    /// <summary>
    /// Проверяет статус линковки программы. В текущей реализации служит заглушкой для потенциального логирования.
    /// </summary>
    /// <param name="program">Идентификатор программы шейдера.</param>
    private void CheckLog(int program)
    {
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int ok);
    }

    /// <summary>
    /// Кэширует расположения всех активных uniform-переменных для быстрого доступа во время рендеринга.
    /// </summary>
    private void CacheUniforms()
    {
        GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out int count);
        for (int i = 0; i < count; i++)
        {
            string name = GL.GetActiveUniform(Handle, i, out _, out _);
            _uniformLocations[name] = GL.GetUniformLocation(Handle, name);
        }
    }

    /// <summary>
    /// Активирует данную шейдерную программу для последующих вызовов отрисовки.
    /// </summary>
    public void Use() => GL.UseProgram(Handle);

    /// <summary>Устанавливает значение uniform-переменной типа mat4.</summary>
    /// <param name="name">Имя переменной в шейдере.</param>
    /// <param name="matrix">Матрица 4x4.</param>
    public void SetMatrix4(string name, Matrix4 matrix)
    {
        if (_uniformLocations.TryGetValue(name, out int loc) && loc != -1) GL.UniformMatrix4(loc, false, ref matrix);
    }

    /// <summary>Устанавливает значение uniform-переменной типа int.</summary>
    /// <param name="name">Имя переменной в шейдере.</param>
    /// <param name="value">Целочисленное значение.</param>
    public void SetInt(string name, int value)
    {
        if (_uniformLocations.TryGetValue(name, out int loc) && loc != -1) GL.Uniform1(loc, value);
    }

    /// <summary>Устанавливает значение uniform-переменной типа float.</summary>
    /// <param name="name">Имя переменной в шейдере.</param>
    /// <param name="value">Вещественное значение.</param>
    public void SetFloat(string name, float value)
    {
        if (_uniformLocations.TryGetValue(name, out int loc) && loc != -1) GL.Uniform1(loc, value);
    }

    /// <summary>Устанавливает значение uniform-переменной типа vec2.</summary>
    /// <param name="name">Имя переменной в шейдере.</param>
    /// <param name="value">Вектор из двух компонентов.</param>
    public void SetVector2(string name, OpenTK.Mathematics.Vector2 value)
    {
        if (_uniformLocations.TryGetValue(name, out int loc) && loc != -1)
            GL.Uniform2(loc, value.X, value.Y);
    }

    /// <summary>
    /// Освобождает ресурсы OpenGL, связанные с шейдерной программой.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        GL.DeleteProgram(Handle);
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~Shader() => Dispose();
}
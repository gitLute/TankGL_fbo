using OpenTK.Graphics.OpenGL4;

namespace TankGL_fbo.WPF.Systems;

/// <summary>
/// Представляет единичный квад (два треугольника) для отрисовки спрайтов в OpenGL.
/// Управляет созданием, привязкой и удалением VAO, VBO и EBO.
/// </summary>
public sealed class UnitQuad : IDisposable
{
    private int _vao, _vbo, _ebo;
    private bool _disposed;

    /// <summary>
    /// Инициализирует геометрию квада, создает и настраивает буферы вершин и индексов.
    /// Вершины задаются в диапазоне [-0.5, 0.5] для удобного масштабирования через матрицы.
    /// </summary>
    public UnitQuad()
    {
        float[] vertices = {
            -0.5f,  0.5f, 0.0f, 1.0f,
             0.5f,  0.5f, 1.0f, 1.0f,
             0.5f, -0.5f, 1.0f, 0.0f,
            -0.5f, -0.5f, 0.0f, 0.0f
        };
        uint[] indices = { 0, 1, 2, 2, 3, 0 };

        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        _ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
        GL.EnableVertexAttribArray(1);
    }

    /// <summary>
    /// Привязывает VAO квада для подготовки к отрисовке.
    /// </summary>
    public void Bind() => GL.BindVertexArray(_vao);

    /// <summary>
    /// Выполняет отрисовку квада с использованием индексного буфера (6 индексов, 2 треугольника).
    /// </summary>
    public void Draw() => GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

    /// <summary>
    /// Освобождает ресурсы OpenGL (VAO, VBO, EBO).
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        GL.DeleteVertexArray(_vao);
        GL.DeleteBuffer(_vbo);
        GL.DeleteBuffer(_ebo);
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
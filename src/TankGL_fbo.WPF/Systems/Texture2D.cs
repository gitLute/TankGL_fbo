using System.IO;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace TankGL_fbo.WPF.Systems;

/// <summary>
/// Обертка над двумерной текстурой OpenGL.
/// Отвечает за загрузку изображения из файла с помощью StbImageSharp,
/// создание текстуры на GPU и управление её жизненным циклом.
/// </summary>
public sealed class Texture2D : IDisposable
{
    /// <summary>Идентификатор текстуры в OpenGL.</summary>
    public int Handle { get; private set; }
    /// <summary>Ширина загруженного изображения в пикселях.</summary>
    public int Width { get; private set; }
    /// <summary>Высота загруженного изображения в пикселях.</summary>
    public int Height { get; private set; }
    private bool _disposed;

    /// <summary>
    /// Загружает изображение из файла и создает OpenGL-текстуру с настройками фильтрации и повторения.
    /// Автоматически переворачивает изображение по вертикали для соответствия координатам OpenGL.
    /// </summary>
    /// <param name="path">Полный путь к файлу изображения.</param>
    /// <returns>Экземпляр созданной текстуры.</returns>
    public static Texture2D FromPath(string path)
    {
        using var stream = File.OpenRead(path);
        StbImage.stbi_set_flip_vertically_on_load(1);
        ImageResult img = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        int tex = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, tex);
        GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
        GL.TexImage2D(
            TextureTarget.Texture2D,
            0,
            PixelInternalFormat.Rgba8,
            img.Width,
            img.Height,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            img.Data
        );
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        return new Texture2D { Handle = tex, Width = img.Width, Height = img.Height };
    }

    /// <summary>
    /// Привязывает текстуру к текущему активному текстурному блоку для использования в шейдере.
    /// </summary>
    public void Use() => GL.BindTexture(TextureTarget.Texture2D, Handle);

    /// <summary>
    /// Удаляет текстуру из памяти видеокарты.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        GL.DeleteTexture(Handle);
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
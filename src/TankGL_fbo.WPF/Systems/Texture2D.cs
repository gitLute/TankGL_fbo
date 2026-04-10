using System.IO;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace TankGL_fbo.WPF.Systems;

public sealed class Texture2D : IDisposable
{
    public int Handle { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    private bool _disposed;

    public static Texture2D FromPath(string path)
    {
        using var stream = File.OpenRead(path);
        StbImage.stbi_set_flip_vertically_on_load(1);
        ImageResult img = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        int tex = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, tex);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, img.Width, img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, img.Data);
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        return new Texture2D { Handle = tex, Width = img.Width, Height = img.Height };
    }

    public void Use() => GL.BindTexture(TextureTarget.Texture2D, Handle);

    public void Dispose()
    {
        if (_disposed) return;
        GL.DeleteTexture(Handle);
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
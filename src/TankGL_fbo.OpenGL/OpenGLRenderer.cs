using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using TankGL_fbo.Core.Interfaces;
using TankGL_fbo.OpenGL.Assets;

namespace TankGL_fbo.OpenGL;

public sealed class OpenGLRenderer : IDisposable
{
    private readonly AssetManager _assets;
    private readonly FrameConfig _config;
    private readonly Matrix4 _projection;

    private int _fbo;
    private int _fboColorTex;
    private int _fboDepthRb;

    public OpenGLRenderer(AssetManager assets, FrameConfig config)
    {
        _assets = assets;
        _config = config;
        _projection = Matrix4.CreateOrthographicOffCenter(config.Left, config.Right, config.Bottom, config.Top, -1f, 1f);
    }

    public void Init()
    {
        GL.ClearColor(0.12f, 0.12f, 0.18f, 1.0f);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        SetupFBO();
    }

    private void SetupFBO()
    {
        _fboColorTex = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _fboColorTex);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, _config.Width, _config.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.BindTexture(TextureTarget.Texture2D, 0);

        _fboDepthRb = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _fboDepthRb);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, _config.Width, _config.Height);
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

        _fbo = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _fboColorTex, 0);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _fboDepthRb);

        //if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete) throw new InvalidOperationException();

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void Render(IEnumerable<IRenderable> objects)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        GL.Viewport(0, 0, _config.Width, _config.Height);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        var shader = _assets.GetShader("default");
        shader.Use();
        shader.SetMatrix4("uProjection", _projection);
        shader.SetInt("uTexture", 0);

        var quad = _assets.Quad;
        quad.Bind();

        foreach (var obj in objects.OrderBy(o => o.ZIndex))
        {
            var tex = _assets.LoadTexture(obj.TexturePath);
            GL.ActiveTexture(TextureUnit.Texture0);
            tex.Use();

            var scale = Matrix4.CreateScale(tex.Width * obj.Scale, tex.Height * obj.Scale, 1.0f);
            var rotation = Matrix4.CreateRotationZ(obj.Rotation);
            var translation = Matrix4.CreateTranslation(obj.Position.X, obj.Position.Y, 0.0f);

            shader.SetMatrix4("uModel", translation * rotation * scale);
            quad.Draw();
        }

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public byte[] ReadPixelsToBuffer()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        var buffer = new byte[_config.Width * _config.Height * 4];
        GL.ReadPixels(0, 0, _config.Width, _config.Height, PixelFormat.Rgba, PixelType.UnsignedByte, buffer);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        return buffer;
    }

    public void Dispose()
    {
        if (_fbo != 0) GL.DeleteFramebuffer(_fbo);
        if (_fboColorTex != 0) GL.DeleteTexture(_fboColorTex);
        if (_fboDepthRb != 0) GL.DeleteRenderbuffer(_fboDepthRb);
    }
}
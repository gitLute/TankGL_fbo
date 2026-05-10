using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using OpenTK.Graphics.OpenGL;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace TankGL_fbo.WPF.Systems
{
    /// <summary>
    /// Рендерер текста для OpenGL.
    /// Генерирует текстурный атлас шрифта с помощью GDI+ и отрисовывает строки
    /// посредством немедленного режима (GL.Begin/GL.End) для совместимости и простоты.
    /// </summary>
    public class OpenGlTextRenderer : IDisposable
    {
        /// <summary>Идентификатор текстуры атласа шрифта в OpenGL.</summary>
        private int _textureId;
        /// <summary>Размер текстурного атласа в пикселях (квадратный).</summary>
        private readonly int _atlasSize = 512;
        /// <summary>Количество символов в одной строке атласа.</summary>
        private readonly int _charsPerRow = 16;
        /// <summary>Нормализованный шаг UV-координат для одного символа.</summary>
        private readonly float _cellStep;
        /// <summary>Начальный символ в таблице ASCII для генерации атласа (пробел).</summary>
        private const char StartChar = (char)32;

        /// <summary>
        /// Инициализирует рендерер текста и генерирует атлас шрифта.
        /// </summary>
        public OpenGlTextRenderer()
        {
            _cellStep = 1.0f / _charsPerRow;
            _textureId = GenerateFontAtlas();
        }

        /// <summary>
        /// Создает текстуру атласа шрифта, отрисовывая символы на Bitmap с помощью GDI+.
        /// </summary>
        /// <returns>Идентификатор созданной OpenGL-текстуры.</returns>
        private int GenerateFontAtlas()
        {
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);
            using (var bitmap = new Bitmap(_atlasSize, _atlasSize, PixelFormat.Format32bppArgb))
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    g.Clear(Color.Transparent);
                    using (var font = new Font("Lucida console", 18, FontStyle.Regular))
                    {
                        float cellPixelSize = _atlasSize / (float)_charsPerRow;
                        for (int i = 0; i < 224; i++)
                        {
                            char c = (char)(StartChar + i);
                            float x = (i % _charsPerRow) * cellPixelSize;
                            float y = (i / _charsPerRow) * cellPixelSize;
                            g.DrawString(c.ToString(), font, Brushes.White, x, y);
                        }
                    }
                }
                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                    data.Width, data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                bitmap.UnlockBits(data);
            }
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            return id;
        }

        /// <summary>
        /// Отрисовывает указанную строку текста в экранных координатах.
        /// Поддерживает перенос строк и настройку цвета.
        /// </summary>
        /// <param name="text">Текст для отрисовки.</param>
        /// <param name="x">X-координата начала текста (левый верхний угол).</param>
        /// <param name="y">Y-координата начала текста.</param>
        /// <param name="fontSize">Размер шрифта в пикселях.</param>
        /// <param name="screenWidth">Ширина экрана/вьюпорта.</param>
        /// <param name="screenHeight">Высота экрана/вьюпорта.</param>
        /// <param name="textColor">Цвет текста (по умолчанию белый).</param>
        public void DrawText(string text, float x, float y, float fontSize, int screenWidth, int screenHeight, Color? textColor = null)
        {
            if (string.IsNullOrEmpty(text)) return;
            float startX = x;
            float currentX = x;
            float currentY = y;
            float lineHeight = fontSize * 1.1f;
            float charWidth = fontSize * 0.55f;

            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(0, screenWidth, screenHeight, 0, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.BindTexture(TextureTarget.Texture2D, _textureId);
            GL.Color4(textColor ?? Color.White);

            GL.Begin(PrimitiveType.Quads);
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    currentX = startX;
                    currentY += lineHeight;
                    continue;
                }
                int idx = c - StartChar;
                if (idx < 0 || idx >= 224) idx = 0;
                float u = (idx % _charsPerRow) * _cellStep;
                float v = (idx / _charsPerRow) * _cellStep;

                GL.TexCoord2(u, v);
                GL.Vertex2(currentX, currentY);
                GL.TexCoord2(u + _cellStep, v);
                GL.Vertex2(currentX + fontSize, currentY);
                GL.TexCoord2(u + _cellStep, v + _cellStep);
                GL.Vertex2(currentX + fontSize, currentY + fontSize);
                GL.TexCoord2(u, v + _cellStep);
                GL.Vertex2(currentX, currentY + fontSize);
                currentX += charWidth;
            }
            GL.End();

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix();
        }

        /// <summary>
        /// Освобождает текстуру атласа шрифта из памяти видеокарты.
        /// </summary>
        public void Dispose()
        {
            if (_textureId != 0)
            {
                GL.DeleteTexture(_textureId);
                _textureId = 0;
            }
        }
    }
}
// Adapted after https://github.com/verybadcat/CSharpMath/blob/master/CSharpMath.SkiaSharp/MathPainter.cs

using System.Drawing;
using CSharpMath.Rendering.FrontEnd;
using CSharpMath.Rendering.BackEnd;
using CSharpMath.Structures;
using VectSharp;

namespace CSharpMath.VectSharp
{
    public class MathPainter : MathPainter<Page, Colour>
    {
        public bool AntiAlias { get; set; } = true;
        public void Draw(Page canvas, global::VectSharp.Point point) => Draw(canvas, (float)point.X, (float)point.Y);
        public override Colour UnwrapColor(Color color) => color.ToNative();
        public override Color WrapColor(Colour color) => color.FromNative();
        public override ICanvas WrapCanvas(Page canvas) =>
          new VectSharpCanvas(canvas, AntiAlias);
        /// <summary>
        /// Ignores the MathList and LaTeX of the <see cref="MathPainter"/> provided.
        /// Repositions the <paramref name="display"/>.
        /// </summary>
        public static void DrawDisplay(MathPainter settings, Display.IDisplay<Fonts, Glyph> display, Page canvas, PointF position)
        {
            DrawDisplay(settings, display, _ => _.Draw(canvas, position));
        }

        /// <summary>
        /// Ignores the MathList and LaTeX of the <see cref="MathPainter"/> provided.
        /// Repositions the <paramref name="display"/>.
        /// </summary>
        public static void DrawDisplay(MathPainter settings, Display.IDisplay<Fonts, Glyph> display,
          Page canvas, global::VectSharp.Point position)
        {
            DrawDisplay(settings, display, _ => _.Draw(canvas, position));
        }

        /// <summary>
        /// Ignores the MathList and LaTeX of the <see cref="MathPainter"/> provided.
        /// Repositions the <paramref name="display"/>.
        /// </summary>
        public static void DrawDisplay(MathPainter settings, Display.IDisplay<Fonts, Glyph> display, Page canvas, float x, float y)
        {
            DrawDisplay(settings, display, _ => _.Draw(canvas, x, y));
        }

        /// <summary>
        /// Ignores the MathList and LaTeX of the <see cref="MathPainter"/> provided.
        /// Repositions the <paramref name="display"/>.
        /// </summary>
        public static void DrawDisplay(MathPainter settings, Display.IDisplay<Fonts, Glyph> display, Page canvas, TextAlignment textAlignment = TextAlignment.Center, Thickness padding = default, float offsetX = 0, float offsetY = 0)
        {
            DrawDisplay(settings, display, _ => _.Draw(canvas, textAlignment, padding, offsetX, offsetY));
        }

        private static void DrawDisplay(MathPainter settings, Display.IDisplay<Fonts, Glyph> display, System.Action<MathPainter> draw)
        {
            if (display is null) return;
            var original = (settings.Display, settings._displayChanged);
            (settings.Display, settings._displayChanged) = (display, false);
            draw(settings);
            (settings.Display, settings._displayChanged) = original;
        }
    }
}

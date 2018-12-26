using System.Drawing;
using CSharpMath.Rendering;
using CSharpMath.Structures;
using SkiaSharp;

namespace CSharpMath.SkiaSharp {
  public class MathPainter : MathPainter<SKCanvas, SKColor>, ICanvasPainter<SKCanvas, MathSource, SKColor> {
    public const bool DefaultAntiAlias = true;

    public MathPainter(float fontSize = DefaultFontSize, bool antiAlias = DefaultAntiAlias) : base(fontSize) =>
      AntiAlias = antiAlias;
    
    public SKStrokeCap StrokeCap { get; set; }
    public bool AntiAlias { get; set; }

    public void Draw(SKCanvas canvas, SKPoint point) => Draw(canvas, point.X, point.Y);

    protected override bool CoordinatesFromBottomLeftInsteadOfTopLeft => false;

    public override SKColor UnwrapColor(Color color) => color.ToNative();

    public override Color WrapColor(SKColor color) => color.FromNative();

    public override ICanvas WrapCanvas(SKCanvas canvas) =>
      new SkiaCanvas(canvas, StrokeCap, AntiAlias);

    /// <summary>
    /// Ignores the MathList and LaTeX of the <see cref="MathPainter"/> provided.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="display"></param>
    public static void DrawDisplay(MathPainter settings, IDisplay<Fonts, Glyph> display, SKCanvas canvas, PointF position) {
      var original = settings._display;
      var originalChanged = settings._displayChanged;
      settings._display = display;
      settings._displayChanged = false;
      settings.Draw(canvas, position);
      settings._display = original;
      settings._displayChanged = originalChanged;
    }
    /// <summary>
    /// Ignores the MathList and LaTeX of the <see cref="MathPainter"/> provided.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="display"></param>
    public static void DrawDisplay(MathPainter settings, IDisplay<Fonts, Glyph> display, SKCanvas canvas, SKPoint position) {
      var original = settings._display;
      var originalChanged = settings._displayChanged;
      settings._display = display;
      settings._displayChanged = false;
      settings.Draw(canvas, position);
      settings._display = original;
      settings._displayChanged = originalChanged;
    }
    /// <summary>
    /// Ignores the MathList and LaTeX of the <see cref="MathPainter"/> provided.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="display"></param>
    public static void DrawDisplay(MathPainter settings, IDisplay<Fonts, Glyph> display, SKCanvas canvas, float x, float y) {
      var original = settings._display;
      var originalChanged = settings._displayChanged;
      settings._display = display;
      settings._displayChanged = false;
      settings.Draw(canvas, x, y);
      settings._display = original;
      settings._displayChanged = originalChanged;
    }
    /// <summary>
    /// Ignores the MathList and LaTeX of the <see cref="MathPainter"/> provided.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="display"></param>
    public static void DrawDisplay(MathPainter settings, IDisplay<Fonts, Glyph> display, SKCanvas canvas,
      TextAlignment textAlignment = TextAlignment.Center, Thickness padding = default, float offsetX = 0, float offsetY = 0) {
      var original = settings._display;
      var originalChanged = settings._displayChanged;
      settings._display = display;
      settings._displayChanged = false;
      settings.Draw(canvas, textAlignment, padding, offsetX, offsetY);
      settings._display = original;
      settings._displayChanged = originalChanged;
    }
  }
}
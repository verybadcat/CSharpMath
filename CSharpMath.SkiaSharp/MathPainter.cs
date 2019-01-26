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

    static readonly MathSource staticValidSource = new MathSource(Atoms.MathLists.WithAtoms());
  
    /// <summary>
    /// Ignores the MathList and LaTeX of the <see cref="MathPainter"/> provided. Repositions the <paramref name="display"/>.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="display"></param>
    public static void DrawDisplay(MathPainter settings, IDisplay<Fonts, Glyph> display, SKCanvas canvas, PointF position) =>
        DrawDisplay(settings, display, canvas, _ => _.Draw(canvas, position));
    /// <summary>
    /// Ignores the MathList and LaTeX of the <see cref="MathPainter"/> provided. Repositions the <paramref name="display"/>.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="display"></param>
    public static void DrawDisplay(MathPainter settings, IDisplay<Fonts, Glyph> display, SKCanvas canvas, SKPoint position) =>
        DrawDisplay(settings, display, canvas, _ => _.Draw(canvas, position));
    /// <summary>
    /// Ignores the MathList and LaTeX of the <see cref="MathPainter"/> provided. Repositions the <paramref name="display"/>.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="display"></param>
    public static void DrawDisplay(MathPainter settings, IDisplay<Fonts, Glyph> display, SKCanvas canvas, float x, float y) =>
        DrawDisplay(settings, display, canvas, _ => _.Draw(canvas, x, y));
    /// <summary>
    /// Ignores the MathList and LaTeX of the <see cref="MathPainter"/> provided. Repositions the <paramref name="display"/>.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="display"></param>
    public static void DrawDisplay(MathPainter settings, IDisplay<Fonts, Glyph> display, SKCanvas canvas,
      TextAlignment textAlignment = TextAlignment.Center, Thickness padding = default, float offsetX = 0, float offsetY = 0) =>
          DrawDisplay(settings, display, canvas, _ => _.Draw(canvas, textAlignment, padding, offsetX, offsetY));
    
    private static void DrawDisplay(MathPainter settings, IDisplay<Fonts, Glyph> display, SKCanvas canvas, Action<MathPainter> draw) {
      if (display is null) return;
      var original = settings._display;
      var originalChanged = settings._displayChanged;
      var source = settings.Source;
      settings.Source = staticValidSource;
      settings._display = display;
      settings._displayChanged = false;
      draw(settings);
      settings.Source = source;
      settings._display = original;
      settings._displayChanged = originalChanged;
    }
  }
}

using System;
using CSharpMath.Rendering;
using CSharpMath.Structures;
using SkiaSharp;

namespace CSharpMath.SkiaSharp {
  public class EditableMathPainter<TButton> : EditableMathPainter<SKCanvas, SKColor, TButton> where TButton : class, Editor.IButton {
    public const bool DefaultAntiAlias = true;

    public EditableMathPainter(Editor.MathKeyboardView<TButton> keyboard, float fontSize = DefaultFontSize * 3 / 2, bool antiAlias = DefaultAntiAlias)
      : base(keyboard, fontSize) =>
      AntiAlias = antiAlias;

    public SKStrokeCap StrokeCap { get; set; }
    public bool AntiAlias { get; set; }

    public void Draw(SKCanvas canvas, SKPoint point) => Draw(canvas, point.X, point.Y);

    protected override bool CoordinatesFromBottomLeftInsteadOfTopLeft => false;

    public override SKColor UnwrapColor(Color color) => color.ToNative();

    public override Color WrapColor(SKColor color) => color.FromNative();

    public override ICanvas WrapCanvas(SKCanvas canvas) =>
      new SkiaCanvas(canvas, StrokeCap, AntiAlias);
  }
}

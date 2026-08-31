using System.Collections.Generic;
using System.Drawing;
using CSharpMath.Atom;

namespace CSharpMath.Display.Displays {
  using FrontEnd;
  /// <summary>Rendering of a generic over/under stack produced by the typesetter for
  /// Stack atoms (\overrightarrow, \overbrace, \overset, and similar commands). The
  /// base display is positioned at the stack's baseline; over and under are
  /// pre-positioned above and below it by the typesetter.</summary>
  public class StackDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph>
    where TFont : IFont<TGlyph> {
    /// <summary>The base (inner-list) display. Its baseline is the stack's baseline.</summary>
    public ListDisplay<TFont, TGlyph> Base { get; }
    /// <summary>The over-row display, or null if there is no over row.</summary>
    public IDisplay<TFont, TGlyph>? Over { get; }
    /// <summary>The under-row display, or null if there is no under row.</summary>
    public IDisplay<TFont, TGlyph>? Under { get; }
    public Range Range { get; }
    public float Width { get; set; }
    public float Ascent { get; set; }
    public float Descent { get; set; }
    private PointF _position;
    private readonly PointF _baseOffset;
    private readonly PointF? _overOffset;
    private readonly PointF? _underOffset;
    public PointF Position {
      get => _position;
      set {
        _position = value;
        Base.Position = new PointF(value.X + _baseOffset.X, value.Y + _baseOffset.Y);
        if (Over is { } over && _overOffset is { } overOffset)
          over.Position = new PointF(value.X + overOffset.X, value.Y + overOffset.Y);
        if (Under is { } under && _underOffset is { } underOffset)
          under.Position = new PointF(value.X + underOffset.X, value.Y + underOffset.Y);
      }
    }
    public bool HasScript { get; set; }
    public Color? TextColor { get; set; }
    public void SetTextColorRecursive(Color? textColor) {
      TextColor ??= textColor;
      Base.SetTextColorRecursive(textColor);
      Over?.SetTextColorRecursive(textColor);
      Under?.SetTextColorRecursive(textColor);
    }
    public Color? BackColor { get; set; }
    public StackDisplay(ListDisplay<TFont, TGlyph> baseDisplay,
      IDisplay<TFont, TGlyph>? over, IDisplay<TFont, TGlyph>? under, Range range) {
      (Base, Over, Under, Range) = (baseDisplay, over, under, range);
      _baseOffset = baseDisplay.Position;
      _overOffset = over?.Position;
      _underOffset = under?.Position;
    }
    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      this.DrawBackground(context);
      Base.Draw(context);
      Over?.Draw(context);
      Under?.Draw(context);
    }
    public override string ToString() => $@"\stack {Base}";
  }
}

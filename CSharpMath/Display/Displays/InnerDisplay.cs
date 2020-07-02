using System.Drawing;
using CSharpMath.Atom;

namespace CSharpMath.Display.Displays {
  using FrontEnd;
  public class InnerDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph> where TFont : IFont<TGlyph> {
    public InnerDisplay(ListDisplay<TFont, TGlyph> inner, IGlyphDisplay<TFont, TGlyph>? left, IGlyphDisplay<TFont, TGlyph>? right, Range range) {
      Inner = inner;
      Left = left;
      Right = right;
      Range = range;
    }
    ///<summary>A display representing the inner list that can be wrapped in delimiters.
    ///It's position is relative to the parent is not treated as a sub-display.</summary>
    public ListDisplay<TFont, TGlyph> Inner { get; }
    ///<summary>A display representing the left delimiter.
    ///Its position is relative to the parent and is not treated as a sub-display.</summary>
    public IGlyphDisplay<TFont, TGlyph>? Left { get; }
    ///<summary>A display representing the right delimiter.
    ///Its position is relative to the parent and is not treated as a sub-display.</summary>
    public IGlyphDisplay<TFont, TGlyph>? Right { get; }

    public float Ascent => System.Math.Max(Left?.Ascent ?? 0, System.Math.Max(Right?.Ascent ?? 0, Inner.Ascent));
    public float Descent => System.Math.Max(Left?.Descent ?? 0, System.Math.Max(Right?.Descent ?? 0, Inner.Descent));
    public float Width => (Left?.Width ?? 0) + Inner.Width + (Right?.Width ?? 0);

    public Range Range { get; }

    PointF _position;
    public PointF Position {
      get => _position;
      set {
        _position = value;
        if (Left is { } l) {
          l.Position = value;
          Inner.Position = new PointF(value.X + l.Width, value.Y);
        } else Inner.Position = value;
        if (Right is { } r)
          r.Position = new PointF(Inner.Position.X + Inner.Width, value.Y);
      }
    }
    public bool HasScript { get; set; }
    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      this.DrawBackground(context);
      Left?.Draw(context);
      Right?.Draw(context);
      Inner.Draw(context);
    }

    public Color? TextColor { get; set; }
    public void SetTextColorRecursive(Color? textColor) {
      TextColor ??= textColor;
      Left?.SetTextColorRecursive(textColor);
      Right?.SetTextColorRecursive(textColor);
      Inner.SetTextColorRecursive(textColor);
    }
    public Color? BackColor { get; set; }

    public override string ToString() => $@"\inner[{Left}][{Right}]{{{Inner}}}";
  }
}
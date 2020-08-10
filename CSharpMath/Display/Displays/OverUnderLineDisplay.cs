using System.Drawing;
using CSharpMath.Atom;

namespace CSharpMath.Display.Displays {
  using FrontEnd;
  /// <summary>Corresponds to MTLineDisplay in iosMath.</summary> 
  public class OverOrUnderlineDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph>
    where TFont : IFont<TGlyph> {
    public OverOrUnderlineDisplay(IDisplay<TFont, TGlyph> inner, PointF position) {
      Inner = inner;
      Position = position;
    }
    public float LineShiftUp { get; set; }
    public float LineThickness { get; set; }
    /// <summary>A display representing the inner list that is overlined or underlined.
    /// Its position is relative to the parent. </summary>
    public IDisplay<TFont, TGlyph> Inner { get; }
    public float Ascent => System.Math.Max(LineShiftUp + LineThickness / 2, Inner.Ascent);
    public float Descent => System.Math.Max(-LineShiftUp + LineThickness / 2, Inner.Descent);
    public float Width => Inner.Width;
    public Range Range => Inner.Range;
    public PointF Position {
      get => Inner.Position;
      set => Inner.Position = value;
    }
    public bool HasScript { get; set; }
    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      this.DrawBackground(context);
      Inner.Draw(context);
      context.SaveState();
      context.DrawLine(Position.X, Position.Y + LineShiftUp, Position.X + Inner.Width, Position.Y + LineShiftUp, LineThickness, TextColor);
      context.RestoreState();
    }
    public Color? TextColor { get; set; }
    public void SetTextColorRecursive(Color? textColor) {
      TextColor ??= textColor;
      Inner.SetTextColorRecursive(textColor);
    }
    public Color? BackColor { get; set; }
    public override string ToString() => $@"\shiftup{{{LineShiftUp}}}{{{Inner}}}";
  }
}

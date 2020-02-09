using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Atoms;
using CSharpMath.FrontEnd;
using Color = CSharpMath.Structures.Color;

namespace CSharpMath.Displays.Display {
  /// <summary>Corresponds to MTLineDisplay in iosMath.</summary> 
  public class OverOrUnderlineDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph>
    where TFont : IFont<TGlyph> {
    public OverOrUnderlineDisplay(IDisplay<TFont, TGlyph> inner, PointF position) {
      Inner = inner;
      Position = position;
    }
    public float LineShiftUp { get; set; }
    public float LineThickness { get; set; }
    /// <summary>A display representing the inner list that is underlined.
    /// Its position is relative to the parent. </summary>
    public IDisplay<TFont, TGlyph> Inner { get; }
    public RectangleF DisplayBounds => Inner.DisplayBounds;
    public float Ascent => Inner.Ascent;
    public float Descent => Inner.Descent;
    public float Width => Inner.Width;
    public Range Range => Inner.Range;
    public PointF Position {
      get => Inner.Position;
      set => Inner.Position = value;
    }
    public bool HasScript { get; set; }
    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
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
    public override string ToString() => $@"\shiftup{{{LineShiftUp}}}{{{Inner}}}";
  }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Atoms;
using CSharpMath.FrontEnd;
using CSharpMath.Structures;

namespace CSharpMath.Display {
  /// <summary>Corresponds to MTLineDisplay in iosMath.</summary> 
  public class OverOrUnderlineDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph>
    where TFont : MathFont<TGlyph> {

    public OverOrUnderlineDisplay(MathListDisplay<TFont, TGlyph> inner, PointF position) {
      Inner = inner;
      Position = position;
    }

    public float LineShiftUp { get; set; }
    public float LineThickness { get; set; }

    /// <summary>A display representing the inner list that is underlined.
    /// Its position is relative to the parent. </summary>
    public MathListDisplay<TFont, TGlyph> Inner { get; private set; }

    public RectangleF DisplayBounds => ((IDisplay<TFont, TGlyph>)Inner).DisplayBounds;

    public float Ascent => ((IDisplay<TFont, TGlyph>)Inner).Ascent;

    public float Descent => ((IDisplay<TFont, TGlyph>)Inner).Descent;

    public float Width => ((IDisplay<TFont, TGlyph>)Inner).Width;

    public Range Range => ((IDisplay<TFont, TGlyph>)Inner).Range;
    public PointF Position { get; set; }
    public bool HasScript { get; set; }
    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      ((IDisplay<TFont, TGlyph>)Inner).Draw(context);
      context.SaveState();
      context.DrawLine(Position.X, Position.Y + LineShiftUp, Position.X + Inner.Width, Position.X + LineShiftUp, LineThickness, TextColor);
      context.RestoreState();
    }
    public Color? TextColor { get; set; }

    public void SetTextColor(Color? textColor) {
      TextColor = TextColor ?? textColor;
      ((IDisplay<TFont, TGlyph>)Inner).SetTextColor(textColor);
    }

  }
}

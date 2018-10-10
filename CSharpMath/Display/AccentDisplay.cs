using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Atoms;
using CSharpMath.FrontEnd;
using Color = CSharpMath.Structures.Color;

namespace CSharpMath.Display {
  public class AccentDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph>
    where TFont : IFont<TGlyph> {


    public AccentDisplay(GlyphDisplay<TFont, TGlyph> accentGlyphDisplay, IDisplay<TFont, TGlyph> accentee) {
      Accent = accentGlyphDisplay;
      Accentee = accentee;
    }

    // A display representing the inner list that is accented. It's position is relative to the parent is not treated as a sub-display.
    public IDisplay<TFont, TGlyph> Accentee { get; private set; }
    // A display representing the accent. It's position is relative to the current display.
    public GlyphDisplay<TFont, TGlyph> Accent { get; private set; }

    public RectangleF DisplayBounds => Accentee.DisplayBounds;

    public float Ascent => Accentee.Ascent;

    public float Descent => Accentee.Descent;

    public float Width => Accentee.Width;

    public Range Range => Accentee.Range;

    public PointF Position { get => Accentee.Position; set => Accentee.Position = value; }
    
    public bool HasScript { get; set; }

    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      Accentee.Draw(context);
      context.SaveState();
      context.Translate(Position);
      context.SetTextPosition(new PointF());
      Accent.Draw(context);
      context.RestoreState();
    }

    public Color? TextColor { get; set; }

    public void SetTextColorRecursive(Color? textColor) {
      TextColor = TextColor ?? textColor;
      Accentee.SetTextColorRecursive(textColor);
      Accent.SetTextColorRecursive(textColor);
    }
  }
}
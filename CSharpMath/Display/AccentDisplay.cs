using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Atoms;
using CSharpMath.FrontEnd;
using CSharpMath.Structures;

namespace CSharpMath.Display {
  public class AccentDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph>
    where TFont : MathFont<TGlyph> {
    public MathListDisplay<TFont, TGlyph> Accentee { get; private set; }
    public GlyphDisplay<TFont, TGlyph> Accent { get; private set; }

    public RectangleF DisplayBounds => ((IDisplay<TFont, TGlyph>)this.Accentee).DisplayBounds;

    public float Ascent => ((IDisplay<TFont, TGlyph>)this.Accentee).Ascent;

    public float Descent => ((IDisplay<TFont, TGlyph>)this.Accentee).Descent;

    public float Width => ((IDisplay<TFont, TGlyph>)this.Accentee).Width;

    public Range Range => ((IDisplay<TFont, TGlyph>)this.Accentee).Range;

    public PointF Position => ((IDisplay<TFont, TGlyph>)this.Accentee).Position;
    
    public bool HasScript { get; set; }

    public void Draw(IGraphicsContext<TFont, TGlyph> context) => ((IDisplay<TFont, TGlyph>)this.Accentee).Draw(context);

    public Color? TextColor { get; set; }

    public void SetTextColor(Color? textColor) {
      TextColor = textColor;
      ((IDisplay<TFont, TGlyph>)Accentee).SetTextColor(textColor);
      ((IDisplay<TFont, TGlyph>)Accent).SetTextColor(textColor);
    }
  }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Atoms;
using CSharpMath.Display.Text;
using CSharpMath.FrontEnd;
using Color = CSharpMath.Structures.Color;

namespace CSharpMath.Display {
  public class RadicalDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph>
    where TFont : MathFont<TGlyph>
  {
    // A display representing the numerator of the fraction. Its position is relative
    // to the parent and it is not treated as a sub-display.
    public IDisplay<TFont, TGlyph> Radicand { get; private set; }
    // A display representing the numerator of the fraction. Its position is relative
    // to the parent and it is not treated as a sub-display.
    public IDisplay<TFont, TGlyph> Degree { get; private set; }

    public float Width { get; set; }

    public float TopKern { get; set; }

    public float LineThickness { get; set; } // the thickness of the top bar of the radical.

    private float _radicalShift;
    private readonly IDisplay<TFont, TGlyph> _radicalGlyph;

        public RadicalDisplay(IDisplay<TFont, TGlyph> innerDisplay, IDownshiftableDisplay<TFont, TGlyph> glyph, PointF position, Range range)
    {
      Radicand = innerDisplay;
      _radicalGlyph = glyph;
      Position = position;
      Range = range;
    }

    public void SetDegree(IDisplay<TFont, TGlyph> degree, TFont degreeFont, FontMathTable<TFont, TGlyph> degreeFontMathTable)
    {
      var kernBefore = degreeFontMathTable.RadicalKernBeforeDegree(degreeFont);
      var kernAfter = degreeFontMathTable.RadicalKernAfterDegree(degreeFont);
      var raise = degreeFontMathTable.RadicalDegreeBottomRaise(degreeFont) * (this.Ascent - this.Descent);
      Degree = degree;
      _radicalShift = kernBefore + degree.Width + kernAfter;
      if (_radicalShift < 0)
      {
        kernBefore -= _radicalShift;
        _radicalShift = 0;
      }
      
      // Position of degree is relative to parent.
      Degree.Position = new PointF(this.Position.X + kernBefore, this.Position.Y + raise);
      // update the width by the _radicalShift
      Width = _radicalShift + _radicalGlyph.Width + Radicand.Width;
      _UpdateRadicandPosition();
    }

    private void _UpdateRadicandPosition()
    {
      var x = this.Position.X + _radicalShift + _radicalGlyph.Width;
      Radicand.Position = new PointF(x, this.Position.Y);
    }

    public RectangleF DisplayBounds => IDisplayExtensions.ComputeDisplayBounds(this);

    public float Ascent { get; set; }

    public float Descent { get; set; }
    
    public Range Range { get; set; }

    PointF _position;
    public PointF Position { get => _position; set { _position = value; _UpdateRadicandPosition(); } }
    public bool HasScript { get; set; }
    public void Draw(IGraphicsContext<TFont, TGlyph> context)
    {
      Radicand?.Draw(context);
      Degree?.Draw(context);
      context.SaveState();
      var translation = new PointF(Position.X + _radicalShift, Position.Y);
      context.Translate(translation);
      context.SetTextPosition(new PointF());
      _radicalGlyph?.Draw(context);
      // Draw the VBOX
      // for the kern of, we don't need to draw anything.
      float heightFromTop = TopKern;
      // draw the horizontal line with the given thickness
      PointF lineStart = new PointF(_radicalGlyph.Width, Ascent - heightFromTop - LineThickness / 2);
      PointF lineEnd = new PointF(lineStart.X + Radicand.Width, lineStart.Y);
      context.DrawLine(lineStart.X, lineStart.Y, lineEnd.X, lineEnd.Y, LineThickness, TextColor);

      context.RestoreState();
    }
    public Color? TextColor { get; set; }

    public void SetTextColorRecursive(Color? textColor) {
      TextColor = TextColor ?? textColor;
      _radicalGlyph.SetTextColorRecursive(TextColor);
      Radicand?.SetTextColorRecursive(textColor);
      Degree?.SetTextColorRecursive(textColor);
    }
  }
}

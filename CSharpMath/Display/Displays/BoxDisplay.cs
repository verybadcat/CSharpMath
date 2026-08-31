using System.Collections.Generic;
using System.Drawing;
using CSharpMath.Atom;
using CSharpMath.Atom.Atoms;

namespace CSharpMath.Display.Displays {
  using FrontEnd;
  /// <summary>Display for the box family (phantom/smash/lap/cancel). Reports geometry
  /// selected by the keep* flags, either draws or suppresses its measured child, and
  /// draws an optional strike overlay.</summary>
  public class BoxDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph>
    where TFont : IFont<TGlyph> {
    public ListDisplay<TFont, TGlyph> Child { get; }
    public bool KeepWidth { get; }
    public bool DrawChild { get; }
    public BoxHAlign HAlign { get; }
    public StrikeStyle StrikeStyle { get; }
    /// <summary>The stroke thickness for the strike overlay.</summary>
    public float StrikeThickness { get; }
    /// <summary>y-offset above the baseline for the \sout horizontal strike.</summary>
    public float StrikeVerticalOffset { get; }
    public Range Range { get; }

    public BoxDisplay(ListDisplay<TFont, TGlyph> child,
      bool keepWidth, bool keepHeight, bool keepDepth, bool drawChild,
      BoxHAlign hAlign, StrikeStyle strikeStyle,
      float strikeThickness, float strikeVerticalOffset, Range range) {
      Child = child;
      DrawChild = drawChild;
      KeepWidth = keepWidth;
      HAlign = hAlign;
      StrikeStyle = strikeStyle;
      StrikeThickness = strikeThickness;
      StrikeVerticalOffset = strikeVerticalOffset;
      Range = range;
      Width = keepWidth ? child.Width : 0;
      Ascent = keepHeight ? child.Ascent : 0;
      Descent = keepDepth ? child.Descent : 0;
    }

    public float Width { get; set; }
    public float Ascent { get; set; }
    public float Descent { get; set; }
    public PointF Position { get; set; }
    public bool HasScript { get; set; }
    public Color? TextColor { get; set; }
    public void SetTextColorRecursive(Color? textColor) {
      TextColor ??= textColor;
      Child.SetTextColorRecursive(textColor);
    }
    public Color? BackColor { get; set; }

    private void UpdateChildPosition() {
      // Push an absolute position down to the child so draw never has to mutate
      // child state or juggle the coordinate transform.
      float offset = 0;
      if (!KeepWidth) {
        offset = HAlign switch {
          BoxHAlign.Right => -Child.Width,
          BoxHAlign.Center => -Child.Width / 2,
          _ => 0
        };
      }
      Child.Position = new PointF(Position.X + offset, Position.Y);
    }

    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      this.DrawBackground(context);
      if (!DrawChild) return; // phantom: geometry already flowed up at measure time
      UpdateChildPosition();
      Child.Draw(context);
      if (StrikeStyle != StrikeStyle.None) {
        // Overlay stroke in the inherited text color.
        float x = Position.X, y = Position.Y;
        float w = Child.Width, top = y + Ascent, bot = y - Descent;
        switch (StrikeStyle) {
          case StrikeStyle.Forward:
            context.DrawLine(x, bot, x + w, top, StrikeThickness, TextColor);
            break;
          case StrikeStyle.Backward:
            context.DrawLine(x, top, x + w, bot, StrikeThickness, TextColor);
            break;
          case StrikeStyle.Cross:
            context.DrawLine(x, bot, x + w, top, StrikeThickness, TextColor);
            context.DrawLine(x, top, x + w, bot, StrikeThickness, TextColor);
            break;
          case StrikeStyle.Horizontal: {
              float m = y + StrikeVerticalOffset;
              context.DrawLine(x, m, x + w, m, StrikeThickness, TextColor);
              break;
            }
        }
      }
    }
    public override string ToString() => @"\box";
  }
}

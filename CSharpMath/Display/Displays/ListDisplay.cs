using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CSharpMath.Atom;

namespace CSharpMath.Display.Displays {
  using FrontEnd;
  /// <summary>Corresponds to MTMathListDisplay in iosMath.</summary>
  public class ListDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph>
    where TFont : IFont<TGlyph> {
    public IReadOnlyList<IDisplay<TFont, TGlyph>> Displays { get; }
    public LinePosition LinePosition { get; set; }
    public bool HasScript { get; set; }
    public Color? TextColor { get; set; }
    public void SetTextColorRecursive(Color? textColor) {
      TextColor ??= textColor;
      foreach (var display in Displays)
        display.SetTextColorRecursive(textColor);
    }
    public Color? BackColor { get; set; }
    /// <summary>For a subscript or superscript, this is the index in the
    /// parent list. For a regular list, it is int.MinValue.</summary>
    public int IndexInParent { get; set; }
    public ListDisplay(IReadOnlyList<IDisplay<TFont, TGlyph>> displays) {
      Displays = displays;
      LinePosition = LinePosition.Regular;
      IndexInParent = int.MinValue;
    }
    public float Ascent => _ascentOverride ?? Displays.CollectionAscent();
    public float Descent => _descentOverride ?? Displays.CollectionDescent();
    public PointF Position { get; set; }
    private float? _ascentOverride;
    private float? _descentOverride;
    private float? _widthOverride;
    /// <summary>Overrides the metrics computed from the child displays (used by the
    /// \cfrac thinspace wrapper, whose padding is not part of any child).</summary>
    public void SetOverrideMetrics(float ascent, float descent, float width) {
      _ascentOverride = ascent;
      _descentOverride = descent;
      _widthOverride = width;
    }

    public Range Range =>
      _rangeOverride
      ?? Range.Combine(
        Displays
        .Where(d => !(d is ListDisplay<TFont, TGlyph> ld && ld.LinePosition != LinePosition.Regular))
        .Select(d => d.Range));
    private Range? _rangeOverride;
    /// <summary>Overrides the combined range of the child displays (used for ruled rows,
    /// where the row range must exclude script-positioned children).</summary>
    public void SetRangeOverride(Range range) => _rangeOverride = range;
    public float Width => _widthOverride ?? Displays.CollectionWidth();
    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      this.DrawBackground(context);
      context.SaveState();
      context.Translate(this.Position);
      context.SetTextPosition(new PointF());
      foreach (var displayAtom in Displays)
        displayAtom.Draw(context);
      context.RestoreState();
    }
    /// <summary>The string returned is NOT real TeX! It's for debugging purposes only.</summary>
    public override string ToString() => string.Concat(Displays);
  }
}
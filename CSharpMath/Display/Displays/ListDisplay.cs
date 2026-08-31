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
    /// <summary>Internal provenance marker for JoinRel-aware ink normalization.</summary>
    internal bool HasJoinRelDirect { get; set; }
    internal bool HasJoinRelDescendant { get; set; }
    public ListDisplay(IReadOnlyList<IDisplay<TFont, TGlyph>> displays) {
      // Take a snapshot: provenance, width, and drawing must continue to
      // describe the same children even when the caller supplied a mutable
      // IReadOnlyList such as List<T>.
      Displays = System.Array.AsReadOnly(displays.ToArray());
      LinePosition = LinePosition.Regular;
      IndexInParent = int.MinValue;
      // Children are fully constructed before their containing list. Cache
      // provenance here so manually composed lists and table containers are
      // covered without a later Measure/Draw traversal.
      HasJoinRelDescendant = Displays.Any(d => d.HasJoinRel());
      LogicalWidth = displays.CollectionWidth();
    }
    public float Ascent => Displays.CollectionAscent();
    public float Descent => Displays.CollectionDescent();
    public PointF Position { get; set; }
    internal float LogicalWidth { get; set; }
    internal float InkLeft => this.InkBounds().Left;
    internal float InkRight => this.InkBounds().Right;

    public Range Range =>
      Range.Combine(
        Displays
        .Where(d => !(d is ListDisplay<TFont, TGlyph> ld && ld.LinePosition != LinePosition.Regular))
        .Select(d => d.Range));
    public float Width => Displays.CollectionWidth();
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

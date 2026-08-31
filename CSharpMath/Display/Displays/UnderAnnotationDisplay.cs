using System.Drawing;
using CSharpMath.Atom;

namespace CSharpMath.Display.Displays;

using FrontEnd;
/// <summary>Creates underanotation display</summary> 
public class UnderAnnotationDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph>
  where TFont : IFont<TGlyph> {
  public UnderAnnotationDisplay(IDisplay<TFont, TGlyph> inner, IDisplay<TFont, TGlyph>? underList,
   IGlyphDisplay<TFont, TGlyph> annotationGlyph,
   float underListBasedDescent,
   PointF position) {
    Inner = inner;
    UnderList = underList;
    AnnotationGlyph = annotationGlyph;
    Position = position;
  }

  /// <summary>A display representing the inner list of annotation
  /// Its position is relative to the parent. </summary>
  public IDisplay<TFont, TGlyph> Inner { get; }
  public IDisplay<TFont, TGlyph>? UnderList { get; }
  public IGlyphDisplay<TFont, TGlyph> AnnotationGlyph { get; }

  public float Ascent {
    get {
      float max = AnnotationGlyph.Ascent + AnnotationGlyph.Position.Y;
      max = System.Math.Max(max, Inner.Ascent + Inner.Position.Y);
      if (UnderList is not null)
        max = System.Math.Max(max, UnderList.Ascent + UnderList.Position.Y);
      return max;
    }
  }
  public float Descent {
    get {
      float max = AnnotationGlyph.Descent - AnnotationGlyph.Position.Y;
      max = System.Math.Max(max, Inner.Descent - Inner.Position.Y);
      if (UnderList is not null)
        max = System.Math.Max(max, UnderList.Descent - UnderList.Position.Y);
      return max;
    }
  }
  public float Width => System.Math.Max(Inner.Width,
    System.Math.Max(AnnotationGlyph.Width, UnderList?.Width ?? 0));
  public Range Range => Inner.Range;
  public PointF Position {
    get => field;
    set => field = value;
  }
  public bool HasScript { get; set; }
  public void Draw(IGraphicsContext<TFont, TGlyph> context) {
    this.DrawBackground(context);
    Inner.Draw(context);
    UnderList?.Draw(context);
    AnnotationGlyph.Draw(context);
  }
  public Color? TextColor { get; set; }
  public void SetTextColorRecursive(Color? textColor) {
    TextColor ??= textColor;
    Inner.SetTextColorRecursive(textColor);
    AnnotationGlyph?.SetTextColorRecursive(textColor);
  }
  public Color? BackColor { get; set; }
  public override string ToString() => $@"\underannotation{{{Inner}}}";
}

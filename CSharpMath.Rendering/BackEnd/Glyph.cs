namespace CSharpMath.Rendering.BackEnd {

  public readonly struct Glyph {
    //public enum FontStyle {
    //  Normal = 0,
    //  Bold = 1,
    //  Italic = 2,
    //  BoldItalic = 3
    //}
    public Glyph(Typography.OpenFont.Typeface typeface, Typography.OpenFont.Glyph info) {
      Typeface = typeface ?? throw new System.ArgumentNullException(nameof(typeface));
      Info = info ?? throw new System.ArgumentNullException(nameof(info));
    }
    public Typography.OpenFont.Typeface Typeface { get; }
    public Typography.OpenFont.Glyph Info { get; }
    public void Deconstruct
      (out Typography.OpenFont.Typeface typeface, out Typography.OpenFont.Glyph glyph)
      { typeface = Typeface; glyph = Info; }
    public bool IsEmpty => Equals(Empty);
    public static readonly Glyph Empty = new Glyph();
  }
}

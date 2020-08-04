namespace CSharpMath.Rendering.BackEnd {

  public readonly struct Glyph {
    public Glyph(Typography.OpenFont.Typeface typeface, Typography.OpenFont.Glyph info) {
      Typeface = typeface ?? throw new System.ArgumentNullException(nameof(typeface));
      Info = info ?? throw new System.ArgumentNullException(nameof(info));
    }
    public Typography.OpenFont.Typeface Typeface { get; }
    public Typography.OpenFont.Glyph Info { get; }
    public bool IsEmpty => Equals(Empty);
    public static readonly Glyph Empty = new Glyph();
  }
}

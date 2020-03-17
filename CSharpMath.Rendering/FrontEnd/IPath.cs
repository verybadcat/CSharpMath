namespace CSharpMath.Rendering.FrontEnd {
  public interface IPath : Typography.OpenFont.IGlyphTranslator {
    CSharpMath.Structures.Color? Foreground { get; set; }
  }
}
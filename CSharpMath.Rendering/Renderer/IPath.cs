namespace CSharpMath.Rendering.Renderer {
  public interface IPath : Typography.OpenFont.IGlyphTranslator {
    CSharpMath.Structures.Color? Foreground { get; set; }
  }
}
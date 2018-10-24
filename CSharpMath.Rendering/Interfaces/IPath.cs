namespace CSharpMath.Rendering {
  public interface IPath : Typography.OpenFont.IGlyphTranslator {
    Structures.Color? Foreground { set; }
  }
}

namespace CSharpMath.Rendering.Renderer {
  public interface ISource {
    string ErrorMessage { get; }
    bool IsValid { get; }
  }
}
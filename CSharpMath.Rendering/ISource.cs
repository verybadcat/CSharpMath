namespace CSharpMath.Rendering {
  public interface ISource {
    string ErrorMessage { get; }
    bool IsValid { get; }
  }
}
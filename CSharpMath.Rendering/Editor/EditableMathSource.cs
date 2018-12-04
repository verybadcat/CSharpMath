namespace CSharpMath.Rendering {
  public readonly struct EditableMathSource : ISource {
    public EditableMathSource(Atoms.MathList mathList) =>
      (MathList, LaTeX) = (mathList, new System.Lazy<string>(() => Atoms.MathListBuilder.MathListToString(mathList)));
    public string ErrorMessage => null;
    public bool IsValid => true;
    public Atoms.MathList MathList { get; }
    public System.Lazy<string> LaTeX { get; }
  }
}

namespace CSharpMath.Blazor.Example;

/// Small, renderer-independent state holder used by the sample and smoke tests.
public sealed class FormulaEditorState {
  public const string DefaultLatex = @"f(x) = \frac{-b \pm \sqrt{b^2 - 4ac}}{2a}\\
\int_0^\infty e^{-x^2}\,dx = \frac{\sqrt{\pi}}{2}";
  public string Latex { get; private set; } = DefaultLatex;
  public int Revision { get; private set; }
  public void SetLatex(string value) { Latex = value; Revision++; }
  public void Reset() { SetLatex(DefaultLatex); }
}
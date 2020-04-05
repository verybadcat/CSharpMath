using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CSharpMath.Rendering;

namespace CSharpMath.Avalonia.Example.Pages {
  public class MathViewPage : UserControl {
    public const string Taylor = @"\begin{eqnarray} e^x  &=&  \sum_{n=0}^{\infty}\frac{x^n}{n!} \\ \\ \sin(x) &=& \sum_{n=0}^{\infty}(-1)^n\frac{x^{2n+1}}{(2n+1)!}  \\ \\ -\ln(1-x)   &=& \sum_{n=1}^{\infty}\frac{x^n}{n}  \ \ \ \ \ (-1 \leq x < 1) \end{eqnarray}";
    public const string EvalIntegral = @"\int_1^2 x\; dx=\left.\frac{x^2}{2}\right|_1^2=2-\frac{1}{2}=\frac{3}{2}";

    public MathViewPage() {
      Resources.Add("Taylor", Atom.LaTeXParser.MathListFromLaTeX(Taylor).Match(mathList => mathList, error => throw new Structures.InvalidCodePathException(error)));
      Resources.Add("EvalIntegral", Atom.LaTeXParser.MathListFromLaTeX(EvalIntegral).Match(mathList => mathList, error => throw new Structures.InvalidCodePathException(error)));
      InitializeComponent();

    }

    private void InitializeComponent() {
      AvaloniaXamlLoader.Load(this);
    }
  }
}

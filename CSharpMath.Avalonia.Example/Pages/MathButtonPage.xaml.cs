using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CSharpMath.Rendering;

namespace CSharpMath.Avalonia.Example.Pages {
  public class MathButtonPage : UserControl {
    public MathButtonPage() {
      Resources.Add("Taylor", @"\begin{eqnarray} e^x  &=&  \sum_{n=0}^{\infty}\frac{x^n}{n!} \\ \\ \sin(x) &=& \sum_{n=0}^{\infty}(-1)^n\frac{x^{2n+1}}{(2n+1)!}  \\ \\ -\ln(1-x)   &=& \sum_{n=1}^{\infty}\frac{x^n}{n}  \ \ \ \ \ (-1 \leq x < 1) \end{eqnarray}");
      Resources.Add("EvalIntegral", @"\int_1^2 x\; dx=\left.\frac{x^2}{2}\right|_1^2=2-\frac{1}{2}=\frac{3}{2}");
      InitializeComponent();

    }

    private void InitializeComponent() {
      AvaloniaXamlLoader.Load(this);
    }
  }
}
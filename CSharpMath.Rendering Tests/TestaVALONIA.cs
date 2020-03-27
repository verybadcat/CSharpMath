using Avalonia;
using Avalonia.Media;
using Xunit;
using AvaloniaColor = Avalonia.Media.Color;

namespace CSharpMath.Rendering.Tests {
  using Avalonia;
  [Collection(nameof(TestFixture))]
  public class TestAvalonia {
    static void Run<TContent>(string file, string latex, string folder,
      Painter<AvaloniaCanvas, TContent, AvaloniaColor> painter) where TContent : class =>
      Test.Run(file, latex, folder, nameof(Avalonia), painter, (painter, target) => painter.DrawAsPng(target));
    [Theory, ClassData(typeof(MathData))]
    public void Display(string file, string latex) =>
      Run(file, latex, nameof(Display), new MathPainter { LineStyle = Atom.LineStyle.Display });
    [Theory, ClassData(typeof(MathData))]
    public void Inline(string file, string latex) =>
      Run(file, latex, nameof(Inline), new MathPainter { LineStyle = Atom.LineStyle.Text });
    [Theory, ClassData(typeof(TextData))]
    public void Text(string file, string latex) =>
      Run(file, latex, nameof(Text), new TextPainter());
  }
}

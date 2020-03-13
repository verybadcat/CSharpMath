using System;
using Stream = System.IO.Stream;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms.Example {
  [AcceptEmptyServiceProvider]
  public class MathInputExtension : IMarkupExtension<Command> {
    public Editor.MathKeyboardInput Input { get; set; }
    public Rendering.MathKeyboard Keyboard { get; set; }
    public Command ProvideValue(IServiceProvider _) =>
      Input is 0 ? throw new ArgumentNullException(nameof(Input)) :
      Keyboard is null ? throw new ArgumentNullException(nameof(Keyboard)) :
      new Command(() => Keyboard.KeyPress(Input));
    object IMarkupExtension.ProvideValue(IServiceProvider _) => ProvideValue(_);
  }
  [AcceptEmptyServiceProvider]
  public class SwitchToTabExtension : IMarkupExtension<Command> {
    public MathKeyboard.Tab Target { get; set; }
    public MathKeyboard Self { get; set; }
    public Command ProvideValue(IServiceProvider _) =>
      Target is 0 ? throw new ArgumentNullException(nameof(Target)) :
      Self is null ? throw new ArgumentNullException(nameof(Self)) :
      new Command(() => Self.CurrentTab = Target);
    object IMarkupExtension.ProvideValue(IServiceProvider _) => ProvideValue(_);
  }
  [AcceptEmptyServiceProvider, ContentProperty(nameof(Path))]
  public class CSharpMathSVGExtension : IMarkupExtension<Stream> {
    public string Path { get; set; }
    public Stream ProvideValue(IServiceProvider _) =>
      Path is null ? throw new ArgumentNullException(nameof(Path)) :
      GetType().Assembly.GetManifestResourceStream(
        $"{nameof(CSharpMath)}.{nameof(Forms)}.{nameof(Example)}.SVGs.{Path}.svg");
    object IMarkupExtension.ProvideValue(IServiceProvider _) => ProvideValue(_);
  }
}
using System;
using SkiaSharp.Views.Forms;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms.MathKeyboardMarkupExtensions {
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

  [AcceptEmptyServiceProvider, ContentProperty(nameof(Target))]
  public class ToggleVisibilityExtension : IMarkupExtension<Command> {
    public VisualElement Target { get; set; }

    public Command ProvideValue(IServiceProvider _) =>
      Target is null ? throw new ArgumentNullException(nameof(Target)) :
      new Command(() => Target.IsVisible ^= true);
    object IMarkupExtension.ProvideValue(IServiceProvider _) => ProvideValue(_);
  }

  [AcceptEmptyServiceProvider, ContentProperty(nameof(Base))]
  public class MultiplyExtension : IMarkupExtension<double> {
    public double Base { get; set; }
    public double By { get; set; }

    public double ProvideValue(IServiceProvider _) => Base * By;
    object IMarkupExtension.ProvideValue(IServiceProvider _) => ProvideValue(_);
  }

  [AcceptEmptyServiceProvider, ContentProperty(nameof(Path))]
  public class EmbeddedSVGExtension : IMarkupExtension<ImageSource> {
    private static readonly SKSvg svg = new SKSvg();
    public string Path { get; set; }
    public ImageSource ProvideValue(IServiceProvider _) {
      svg.Load(GetType().Assembly.GetManifestResourceStream("CSharpMath.Forms.SVGs." + Path + ".svg"));
      return new SKPictureImageSource {
        Picture = svg.Picture,
        Dimensions = svg.CanvasSize.ToSizeI()
      };
    }
    object IMarkupExtension.ProvideValue(IServiceProvider _) => ProvideValue(_);
  }
}

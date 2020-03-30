namespace CSharpMath.Forms {
  using Rendering.Text;
  using SkiaSharp;
  using Xamarin.Forms;
  using Xamarin.Forms.Xaml;
  [ContentProperty(nameof(LaTeX)), XamlCompilation(XamlCompilationOptions.Compile)]
  public class TextView : BaseView<TextPainter, TextAtom> {
    public float AdditionalLineSpacing { get => (float)GetValue(AdditionalLineSpacingProperty); set => SetValue(AdditionalLineSpacingProperty, value); }
    public static readonly BindableProperty AdditionalLineSpacingProperty = CreateProperty(nameof(AdditionalLineSpacing), p => p.AdditionalLineSpacing, (p, v) => p.AdditionalLineSpacing = v, typeof(TextView));
  }
}
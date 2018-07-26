namespace CSharpMath.Forms
{
  using Rendering;
  using SkiaSharp;
  using Xamarin.Forms;
  using Xamarin.Forms.Xaml;

  [ContentProperty(nameof(Text)), XamlCompilation(XamlCompilationOptions.Compile)]
	public class TextView : BaseView<TextPainter, TextSource>
	{
    public TextAtom Atom { get => Source.Atom; set => Source = new TextSource(value); }
    public string Text { get => Source.Text; set => Source = new TextSource(value); }

    public float? LineWidth { get => (float?)GetValue(LineWidthProperty); set => SetValue(LineWidthProperty, value); }
    public static readonly BindableProperty LineWidthProperty = BindableProperty.Create(nameof(LineWidth), typeof(float?), typeof(TextView));

    protected override void OnPaintSurface(global::SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e) {
      Painter.LineWidth = LineWidth ?? e.Info.Width;
      base.OnPaintSurface(e);
    }
  }
}
namespace CSharpMath.Forms {
  using Rendering.Renderer;
  using Rendering.Text;
  using SkiaSharp;
  using Xamarin.Forms;
  using Xamarin.Forms.Xaml;
  [ContentProperty(nameof(LaTeX)), XamlCompilation(XamlCompilationOptions.Compile)]
  public class TextView : BaseView<TextPainter, TextSource, TextView.PainterSupplier> {
    public struct PainterSupplier : IPainterAndSourceSupplier<TextPainter, TextSource> {
      public TextPainter Default => new TextPainter();
      public string DefaultLaTeX(TextPainter painter) => painter.LaTeX;
      public string LaTeXFromSource(TextSource source) => source.LaTeX;
      public TextSource SourceFromLaTeX(string latex) => TextSource.FromLaTeX(latex);
    }
    public TextView() : base(default(PainterSupplier).Default) { }
    public TextAtom Atom { get => Source.Atom; set => Source = new TextSource(value); }
#if false
    public float? LineWidth {
      get => (float?)GetValue(LineWidthProperty);
      set => SetValue(LineWidthProperty, value);
    }
    public static readonly BindableProperty LineWidthProperty =
      BindableProperty.Create(nameof(LineWidth), typeof(float?), typeof(TextView));
#endif
  }
}
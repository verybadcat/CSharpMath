namespace CSharpMath.Forms
{
  using Rendering;
  using SkiaSharp;
  using Xamarin.Forms;
  using Xamarin.Forms.Xaml;

  [ContentProperty(nameof(LaTeX)), XamlCompilation(XamlCompilationOptions.Compile)]
	public class TextView : BaseView<TextPainter, TextSource, TextView.PainterSupplier> {
    public struct PainterSupplier : IPainterSupplier<TextPainter> {
      public TextPainter Default => new TextPainter();
    }
    public TextView() : base(default(PainterSupplier).Default) { }
    public TextAtom Atom { get => Source.Atom; set => Source = new TextSource(value); }
    public string LaTeX { get => (string)GetValue(LaTeXProperty); set => SetValue(LaTeXProperty, value); }


    public static BindableProperty LaTeXProperty = BindableProperty.Create(nameof(LaTeX), typeof(string), typeof(TextView), "");

    //Same hack as MathView
    protected override void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null) {
      base.OnPropertyChanged(propertyName);
      if (propertyName == LaTeXProperty.PropertyName) {
        if (LaTeX is null) {
          if (Source.IsValid) Source = new TextSource();
        } else {
          var desiredTarget = new TextSource(LaTeX);
          if (!Source.Equals(desiredTarget))
            Source = desiredTarget;
        }
      } else if (propertyName == SourceProperty.PropertyName) {
        if (Source.IsValid) {
          var desiredTarget = Source.LaTeX;
          if (!LaTeX.Equals(desiredTarget))
            LaTeX = desiredTarget;
        } else if (!string.IsNullOrEmpty(LaTeX)) LaTeX = "";
      }
    }
    //public float? LineWidth { get => (float?)GetValue(LineWidthProperty); set => SetValue(LineWidthProperty, value); }
    //public static readonly BindableProperty LineWidthProperty = BindableProperty.Create(nameof(LineWidth), typeof(float?), typeof(TextView));
  }
}
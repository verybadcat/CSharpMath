using CSharpMath.Atom;
using CSharpMath.Rendering.FrontEnd;
using TextAlignment = CSharpMath.Rendering.FrontEnd.TextAlignment;

namespace CSharpMath.Uno.Example {
  public partial class ExamplesPage : Page {
    public ExamplesPage() {
      InitializeComponent();

      // Collect all MathView controls from the StackPanel (defined in XAML)
      var mathViews = Stack.Children.OfType<MathView>().ToList();

      // Set ErrorFontSize for all views
      foreach (var view in mathViews) {
        view.ErrorFontSize = view.FontSize * 0.8f;
      }

      var values = typeof(TextAlignment).GetEnumValues();
      Array.Reverse(values);
      alignment.ItemsSource = values;
      alignment.SelectionChanged += (sender, e) => {
        foreach (var view in mathViews) view.TextAlignment = (TextAlignment)alignment.SelectedItem;
      };
      alignment.SelectedItem = TextAlignment.Top;

      paintStyle.ItemsSource = typeof(PaintStyle).GetEnumValues();
      paintStyle.SelectionChanged += (sender, e) => {
        foreach (var view in mathViews) view.PaintStyle = (PaintStyle)paintStyle.SelectedItem;
      };
      paintStyle.SelectedItem = PaintStyle.Fill;

      lineStyle.ItemsSource = typeof(LineStyle).GetEnumValues();
      lineStyle.SelectionChanged += (sender, e) => {
        foreach (var view in mathViews) view.LineStyle = (LineStyle)lineStyle.SelectedItem;
      };
      lineStyle.SelectedItem = LineStyle.Display;

      drawGlyphBoxes.Toggled += (sender, e) => {
        foreach (var view in mathViews)
          view.GlyphBoxColor = drawGlyphBoxes.IsOn ? (Microsoft.UI.Colors.Red, Microsoft.UI.Colors.Blue) : null;
      };
      highlight.Toggled += (sender, e) => {
        foreach (var view in mathViews)
          view.HighlightColor = highlight.IsOn ? Microsoft.UI.Colors.Yellow : Microsoft.UI.Colors.Transparent;
      };
    }
  }
}
using CSharpMath.Atom;
using TextAlignment = CSharpMath.Rendering.FrontEnd.TextAlignment;

namespace CSharpMath.Maui.Example {
  public partial class TextPage : ContentPage {
    public TextPage() {
      InitializeComponent();
      View.LaTeX = Text.Text;
      Size.SelectedItem = View.FontSize;
    }

    private void Size_SelectedIndexChanged(object sender, EventArgs e) {
      if (View is { }) { // Size is initialized before View, so View is null at first
        View.FontSize = (float)Size.SelectedItem;
        Scroll.InvalidateMeasure(); // Seems to be required on Windows to update the scroll size
      }
    }

    private void Text_TextChanged(object sender, TextChangedEventArgs e) {
      if (View is { }) { // Text is initialized before View, so View is null at first
        View.LaTeX = e.NewTextValue;
        Scroll.InvalidateMeasure(); // Seems to be required on Windows to update the scroll size
      }
    }
  }
}
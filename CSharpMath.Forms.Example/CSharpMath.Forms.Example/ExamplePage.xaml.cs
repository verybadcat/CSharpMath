using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms.Example {
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class ExamplePage : ContentPage {
    public ExamplePage() {
      InitializeComponent();
      App.AllViews.Add(View);
      Size.ItemsSource = CustomExamplePage.FontSizes;
      Size.SelectedIndexChanged += (sender, e) => {
        View.FontSize = (float)Size.SelectedItem;
        View.InvalidateSurface();
      };
      Size.SelectedItem = 96f;
      Picker.ItemsSource = SkiaSharp.MathData.AllConstants.Keys.ToList();
      Picker.SelectedIndexChanged += (sender, e) => {
        View.LaTeX = Label.Text = SkiaSharp.MathData.AllConstants[(string)Picker.SelectedItem];
        View.InvalidateSurface();
      };
      Picker.SelectedItem = nameof(ExamplesPage.ShortIntegral);
    }
    protected override void OnDisappearing() {
      //App.AllViews.Remove(View);
      base.OnDisappearing();
    }
  }
}
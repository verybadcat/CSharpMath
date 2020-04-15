using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms.Example {
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class SelectPage : ContentPage {
    public SelectPage() {
      InitializeComponent();
      App.AllViews.Add(View);
      Size.ItemsSource = TryPage.FontSizes;
      Size.SelectedIndexChanged += (sender, e) =>
        View.FontSize = (float)Size.SelectedItem;
      Size.SelectedItem = 96f;
      Picker.ItemsSource = Rendering.Tests.TestRenderingMathData.AllConstants.Keys.ToList();
      Picker.SelectedIndexChanged += (sender, e) =>
        View.LaTeX = Label.Text = Rendering.Tests.TestRenderingMathData.AllConstants[(string)Picker.SelectedItem];
      Picker.SelectedItem = nameof(Rendering.Tests.TestRenderingMathData.ShortIntegral);
    }
    protected override void OnDisappearing() {
      //App.AllViews.Remove(View);
      base.OnDisappearing();
    }
    private void Button_Clicked(object sender, System.EventArgs e) {
      View.DisplacementX = View.DisplacementY = 0;
    }
  }
}
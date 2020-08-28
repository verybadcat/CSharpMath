using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms.Example {
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class ExamplesPage : ContentPage {
    public ExamplesPage() {
      InitializeComponent();
      App.AllMathViews.Add(View);
      View.FontSize = 30;
      View.LaTeX = Rendering.Tests.TestRenderingMathData.AllConstantValues;
    }
    private void Button_Clicked(object sender, EventArgs e) =>
      View.DisplacementX = View.DisplacementY = 0;
  }
}
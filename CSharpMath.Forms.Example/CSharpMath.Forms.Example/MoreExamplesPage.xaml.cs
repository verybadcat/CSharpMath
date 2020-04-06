using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms.Example {
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class MoreExamplesPage : ContentPage {
    public MoreExamplesPage() {
      InitializeComponent();
      foreach (var view in MoreExamples.Views) {
        view.ErrorFontSize = view.FontSize * 0.8f;
        Stack.Children.Add(view);
      }
    }
  }
}
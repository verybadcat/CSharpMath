using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms.Example {
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class MoreExamplesPage : ContentPage {
    public MoreExamplesPage() {
      InitializeComponent();
      foreach (var view in MoreExamples.Views) Stack.Children.Add(view);
    }
  }
}
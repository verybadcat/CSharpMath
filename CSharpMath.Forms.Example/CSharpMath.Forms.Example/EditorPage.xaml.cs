using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms.Example {
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class EditorPage : ContentPage {
    public EditorPage() {
      InitializeComponent();
#warning WIPWIPWIPWIPWIPWIWPIWPIP
      //var (view, keyboard) = MathKeyboard.Default;
      //Content = new StackLayout { Children = { view, keyboard, new BoxView{HeightRequest = 50, WidthRequest = 50, Color = Color.Black} } };
    }
  }
}
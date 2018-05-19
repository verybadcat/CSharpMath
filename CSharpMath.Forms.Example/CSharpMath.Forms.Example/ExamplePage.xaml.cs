using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms.Example
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ExamplePage : ContentPage
	{
    Dictionary<string, string> dict = AllExamplesPage.AllConstants.ToDictionary(info => info.Name, info => (string)info.GetRawConstantValue());
    public ExamplePage() {
      InitializeComponent();
      Picker.ItemsSource = dict.Keys.ToList();
      Picker.SelectedIndexChanged += (sender, e) => Device.BeginInvokeOnMainThread(() => View.LaTeX = Label.Text = dict[(string)Picker.SelectedItem]);
      View.BackgroundColor = Color.Gray;
    }
	}
}
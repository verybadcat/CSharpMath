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
      View.HorizontalTextAlignment = TextAlignment.Center;
      Size.ItemsSource = new float[] { 8, 12, 16, 24, 36, 48, 60, 72, 96, 144, 192, 288, 384, 576, 768, 864 };
      Size.SelectedIndexChanged += (sender, e) => {
        View.FontSize = (float)Size.SelectedItem;
        View.InvalidateSurface();
      };
      Picker.ItemsSource = dict.Keys.ToList();
      Picker.SelectedIndexChanged += (sender, e) => View.LaTeX = Label.Text = dict[(string)Picker.SelectedItem];
    }
	}
}
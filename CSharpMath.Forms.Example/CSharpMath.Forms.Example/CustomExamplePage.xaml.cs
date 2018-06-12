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
	public partial class CustomExamplePage : ContentPage
	{
    Dictionary<string, string> dict = AllExamplesPage.AllConstants.ToDictionary(info => info.Name, info => (string)info.GetRawConstantValue());
    public CustomExamplePage() {
      InitializeComponent();
      App.AllViews.Add(View);
      Size.ItemsSource = new float[] { 8, 12, 16, 24, 36, 48, 60, 72, 96, 144, 192, 288, 384, 480, 576, 666 /*(insert trollface here)*/, 768, 864, 960 };
      Size.SelectedIndexChanged += (sender, e) => {
        View.FontSize = (float)Size.SelectedItem;
        View.InvalidateSurface();
      };
      Entry.TextChanged += (sender, e) => { View.LaTeX = Entry.Text; View.InvalidateSurface(); };
    }
    protected override void OnDisappearing() {
      //App.AllViews.Remove(View);
      base.OnDisappearing();
    }

    private async Task In_Clicked(object sender, EventArgs e) {
      var file = await Plugin.FilePicker.CrossFilePicker.Current.PickFile();
      if (file == null) return; // user canceled file picking
      var bytes = Encoding.UTF8.GetBytes(Entry.Text);
      file.GetStream().Write(bytes, 0, bytes.Length);
      await Plugin.FilePicker.CrossFilePicker.Current.SaveFile(file);
    }

    private async Task Out_Clicked(object sender, EventArgs e) {
      var file = await Plugin.FilePicker.CrossFilePicker.Current.PickFile();
      if (file == null) return; // user canceled file picking
      Entry.Text = Encoding.UTF8.GetString(file.DataArray);
      await Plugin.FilePicker.CrossFilePicker.Current.SaveFile(file);
    }
  }
}
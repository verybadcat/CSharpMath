using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms.Example
{
	[XamlCompilation(XamlCompilationOptions.Compile), Android.Runtime.Preserve(AllMembers = true), Foundation.Preserve(AllMembers = true)]
	public partial class CustomExamplePage : ContentPage
	{
    public static float[] FontSizes = new float[] { 8, 12, 16, 24, 36, 48, 60, 72, 96, 144, 192, 288, 384, 480, 576, 666 /*(insert trollface here)*/, 768, 864, 960 };
    Dictionary<string, string> dict = ExamplesPage.AllConstants.ToDictionary(info => info.Name, info => (string)info.GetRawConstantValue());
    public CustomExamplePage() {
      InitializeComponent();
      App.AllViews.Add(View);
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

    static readonly string RefFile = Path.Combine(Environment.CurrentDirectory, "SavedLaTeX.txt");

    private async void In_Clicked(object sender, EventArgs e) {
      using (var file = File.Open(RefFile, FileMode.OpenOrCreate))
      using (var reader = new StreamReader(file))
        Entry.Text = await reader.ReadToEndAsync();
    }

    private async void Out_Clicked(object sender, EventArgs e) {
      using (var file = File.Open(RefFile, FileMode.Create))
      using (var writer = new StreamWriter(file))
        await writer.WriteAsync(Entry.Text);
    }
  }
}
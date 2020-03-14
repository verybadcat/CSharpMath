using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms.Example {
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class ExamplesPage : ContentPage {
    public ExamplesPage() {
      InitializeComponent();
      App.AllViews.Add(View);
      View.FontSize = 30;
      View.LaTeX = latex;
      View.InvalidateSurface();
    }
    static readonly string latex =
      string.Join(@"\\", SkiaSharp.MathData.AllConstants.Select(info => $@"{info.Key}: {info.Value}"));
    private void Button_Clicked(object sender, EventArgs e) {
      View.DisplacementX = View.DisplacementY = 0;
      View.InvalidateSurface();
    }
  }
}
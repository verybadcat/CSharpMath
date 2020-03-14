using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms.Example {
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class ExamplesPage : ContentPage {
    static readonly string latex =
      string.Join(@"\\", SkiaSharp.MathData.AllConstants.Select(info => $@"{info.Key}: {info.Value}"));

    protected override void OnAppearing() {
      base.OnAppearing();
      App.AllViews.Add(View);
      View.FontSize = 50;
      View.LaTeX = latex;
      View.InvalidateSurface();
    }
    protected override void OnDisappearing() {
      //App.AllViews.Remove(View);
      base.OnDisappearing();
    }
  }
}
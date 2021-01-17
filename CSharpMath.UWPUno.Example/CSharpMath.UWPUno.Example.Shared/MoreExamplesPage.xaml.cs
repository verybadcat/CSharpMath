using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace CSharpMath.UWPUno.Example {
  public sealed partial class MoreExamplesPage : Page {
    public MoreExamplesPage() {
      this.InitializeComponent();
      foreach (var view in MoreExamples.Views) {
        view.ErrorFontSize = view.FontSize * 0.8f;
        view.TextColor = Windows.UI.Colors.White;
        Stack.Children.Add(view);
      }
    }
  }
}
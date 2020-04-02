using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using SkiaSharp.Views.Forms;

namespace CSharpMath.Xaml.Tests {
  using Forms;
  class TestXamarinForms
    : Test<Color, BindingMode, BindableProperty, SKCanvasView, MathView, TextView> {
    protected override BindingMode Default => BindingMode.Default;
    protected override BindingMode TwoWay => BindingMode.TwoWay;
    protected override void SetBinding(SKCanvasView view, BindableProperty property, string viewModelProperty, BindingMode bindingMode) =>
      view.SetBinding(property, viewModelProperty, bindingMode);
    protected override void SetBindingContext(SKCanvasView view, object viewModel) =>
      view.BindingContext = viewModel;
  }
}

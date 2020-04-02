using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;

namespace CSharpMath.Xaml.Tests {
  using Avalonia;
  class TestAvalonia
    : Test<Color, BindingMode, AvaloniaProperty, Control, MathView, TextView> {
    protected override BindingMode Default => BindingMode.Default;
    protected override BindingMode TwoWay => BindingMode.TwoWay;
    protected override void SetBinding(Control view, AvaloniaProperty property, string viewModelProperty, BindingMode bindingMode) =>
      view.Bind(property, new Binding(viewModelProperty, bindingMode));
    protected override void SetBindingContext(Control view, object viewModel) =>
      view.DataContext = viewModel;
  }
}

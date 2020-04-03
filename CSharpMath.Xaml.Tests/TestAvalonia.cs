using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Markup.Xaml;

namespace CSharpMath.Xaml.Tests {
  using Avalonia;
  public class TestAvalonia
    : Test<Color, BindingMode, AvaloniaProperty, Control, MathView, TextView> {
    protected override string FrontEndNamespace => nameof(Avalonia);
    protected override BindingMode Default => BindingMode.Default;
    protected override BindingMode OneWayToSource => BindingMode.OneWayToSource;
    protected override BindingMode TwoWay => BindingMode.TwoWay;
    protected override TView ParseFromXaml<TView>(string xaml) => AvaloniaXamlLoader.Parse<TView>(xaml);
    protected override IDisposable SetBinding(Control view, AvaloniaProperty property, string viewModelProperty, BindingMode bindingMode) =>
      view.Bind(property, new Binding(viewModelProperty, bindingMode));
    protected override void SetBindingContext(Control view, object viewModel) =>
      view.DataContext = viewModel;
  }
}

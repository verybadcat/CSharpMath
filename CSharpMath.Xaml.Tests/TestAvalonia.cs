using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace CSharpMath.Xaml.Tests {
  using Avalonia;
  public class TestAvalonia
    : Test<Color, BindingMode, AvaloniaProperty, Control, MathView, TextView> {
    protected override Display.IDisplay<Rendering.BackEnd.Fonts, Rendering.BackEnd.Glyph> GetDisplay(Control control) {
      switch (control) {
        case MathView { Painter: var p }:
          p.Measure();
          return p.Display ?? throw new InvalidOperationException("Invalid content");
        case TextView { Painter: var p }:
          p.Measure(float.PositiveInfinity);
          return p.Display ?? throw new InvalidOperationException("Invalid content");
        default: throw new NotImplementedException();
      }
      ;
    }
    protected override string FrontEndNamespace => nameof(Avalonia);
    protected override BindingMode Default => BindingMode.Default;
    protected override BindingMode OneWayToSource => BindingMode.OneWayToSource;
    protected override BindingMode TwoWay => BindingMode.TwoWay;
    protected override TView ParseFromXaml<TView>(string xaml) => AvaloniaRuntimeXamlLoader.Parse<TView>(xaml);
    protected override IDisposable SetBinding(Control view, AvaloniaProperty property, string viewModelProperty, BindingMode bindingMode) =>
      view.Bind(property, new Binding(viewModelProperty, bindingMode));
    protected override void SetBindingContext(Control view, object viewModel) =>
      view.DataContext = viewModel;
  }
}
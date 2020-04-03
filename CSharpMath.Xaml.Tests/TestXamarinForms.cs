using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using SkiaSharp.Views.Forms;

namespace CSharpMath.Xaml.Tests {
  using Forms;
  using Xamarin.Forms.Xaml;

  public class TestXamarinForms
    : Test<Color, BindingMode, BindableProperty, SKCanvasView, MathView, TextView> {
    protected override string FrontEndNamespace => nameof(Forms);
    protected override BindingMode Default => BindingMode.Default;
    protected override BindingMode OneWayToSource => BindingMode.OneWayToSource;
    protected override BindingMode TwoWay => BindingMode.TwoWay;
    protected override TView ParseFromXaml<TView>(string xaml) {
      var view = new TView();
      view.LoadFromXaml(xaml);
      return view;
    }
    class DisposeAction : IDisposable {
      public DisposeAction(Action action) => this.action = action;
      readonly Action action;
      void IDisposable.Dispose() => action();
    }
    protected override IDisposable SetBinding(SKCanvasView view, BindableProperty property, string viewModelProperty, BindingMode bindingMode) {
      view.SetBinding(property, viewModelProperty, bindingMode);
      return new DisposeAction(() => view.RemoveBinding(property));
    }
    protected override void SetBindingContext(SKCanvasView view, object viewModel) =>
      view.BindingContext = viewModel;
  }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace CSharpMath.Xaml.Tests {
  using Maui;
  using Microsoft.Maui.Controls.Xaml;

  public class TestMaui
    : Test<Color, BindingMode, BindableProperty, GraphicsView, MathView, TextView> {

    static TestMaui() { // Ensure MAUI application context is available before any tests run
      Microsoft.Maui.Dispatching.DispatcherProvider.SetCurrent(new TestDispatcherProvider()); // Set up a minimal dispatcher
    }

    // Minimal dispatcher provider implementation for unit tests
    class TestDispatcherProvider : Microsoft.Maui.Dispatching.IDispatcherProvider {
      private readonly TestDispatcher _dispatcher = new();

      public Microsoft.Maui.Dispatching.IDispatcher GetForCurrentThread() => _dispatcher;
    }

    // Minimal dispatcher implementation for unit tests
    class TestDispatcher : Microsoft.Maui.Dispatching.IDispatcher {
      public bool IsDispatchRequired => false;

      public Microsoft.Maui.Dispatching.IDispatcherTimer CreateTimer() =>
        throw new NotImplementedException("Timer not needed for these tests");

      public bool Dispatch(Action action) {
        action();
        return true;
      }

      public bool DispatchDelayed(TimeSpan delay, Action action) {
        action();
        return true;
      }
    }

    protected override Display.IDisplay<Rendering.BackEnd.Fonts, Rendering.BackEnd.Glyph> GetDisplay(GraphicsView view) {
      switch (view) {
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
    protected override string FrontEndNamespace => nameof(Maui);
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
    protected override IDisposable SetBinding(GraphicsView view, BindableProperty property, string viewModelProperty, BindingMode bindingMode) {
      view.SetBinding(property, viewModelProperty, bindingMode);
      return new DisposeAction(() => view.RemoveBinding(property));
    }
    protected override void SetBindingContext(GraphicsView view, object viewModel) =>
      view.BindingContext = viewModel;
  }
}
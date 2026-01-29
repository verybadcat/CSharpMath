using Microsoft.Extensions.DependencyInjection;

namespace CSharpMath.Maui.Example {
  public partial class App : Application {
    public App() {
      InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState) {
      return new Window(new AppShell());
    }
  }
}
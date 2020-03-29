namespace CSharpMath.Playground.NuGet.UWP {
  public sealed partial class MainPage {
    public MainPage() {
      InitializeComponent();
      LoadApplication(new NuGet.App());
    }
  }
}
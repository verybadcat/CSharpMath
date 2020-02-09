namespace CSharpMath.Forms.Example.WPF {
  /// <summary>Interaction logic for MainWindow.xaml</summary>
  public partial class MainWindow : Xamarin.Forms.Platform.WPF.FormsApplicationPage {
    public MainWindow() {
      InitializeComponent();
      Xamarin.Forms.Forms.Init();
      LoadApplication(new Example.App());
    }
  }
}
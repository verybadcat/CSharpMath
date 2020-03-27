using Android.App;
using Android.Content.PM;
using Android.OS;

namespace CSharpMath.Forms.Example.Android {
  [Activity(
    Label = "CSharpMath.Forms.Example",
    Icon = "@mipmap/icon",
    Theme = "@style/MainTheme",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
  public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity {
    protected override void OnCreate(Bundle bundle) {
      TabLayoutResource = Resource.Layout.Tabbar;
      ToolbarResource = Resource.Layout.Toolbar;
      base.OnCreate(bundle);
      Xamarin.Forms.Forms.Init(this, bundle);
      LoadApplication(new App());
    }
  }
}
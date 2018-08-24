using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.PlatformConfiguration;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace CSharpMath.Forms.Example
{
	public partial class App : Application
	{
    public static ObservableCollection<MathView> AllViews = new ObservableCollection<MathView>();
    public App() {
      InitializeComponent();
    }

    int index = -1;
    void Handle_ChildAdded(object sender, ElementEventArgs e) {
      index++;
      if (Device.RuntimePlatform == Device.iOS && e.Element is Page p && !(p is ExamplesPage)) {
        p.Padding = new Thickness(0, index > 3 ? 90 : 30, 0, 0); //Pages after 4th page have an extra thicc tab bar on iOS
      }
    }

    protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}

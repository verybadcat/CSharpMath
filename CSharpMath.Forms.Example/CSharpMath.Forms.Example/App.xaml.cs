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
    public static ObservableCollection<FormsLatexView> AllViews = new ObservableCollection<FormsLatexView>();
    public App() {
      InitializeComponent();
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

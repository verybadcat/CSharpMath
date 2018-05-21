using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace CSharpMath.Forms.Example
{
	public partial class App : Application
	{
    public static List<FormsLatexView> AllViews = new List<FormsLatexView>();
    public App() {
      InitializeComponent();
      MainPage = new TabbedPage {
        Children = {
          new ExamplePage(), new AllExamplesPage(), new CustomExamplePage(), new SettingsPage()
        }
      };
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

using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace CSharpMath.Forms.Example
{
  static class PageExtensions {
    public static Page ApplyiOSPadding(this Page page) {
      page.Padding = new Thickness(0, Device.RuntimePlatform == Device.iOS ? 30 : 0, 0, 0); //Accomodate the iOS status bar
      return page;
    }
    public static MultiPage<T> SetCurrentPageIndex<T>(this MultiPage<T> page, int index) where T : Page 
      { page.CurrentPage = page.Children[index]; return page; }
  }
	public partial class App : Application
	{
    public static ObservableCollection<FormsLatexView> AllViews = new ObservableCollection<FormsLatexView>();
    public App() {
      InitializeComponent();
      MainPage = new TabbedPage {
        Children = {
          new ExamplePage().ApplyiOSPadding(), new AllExamplesPage().ApplyiOSPadding(), new CustomExamplePage().ApplyiOSPadding(), new SettingsPage().ApplyiOSPadding()
        }
      }.SetCurrentPageIndex(2);
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

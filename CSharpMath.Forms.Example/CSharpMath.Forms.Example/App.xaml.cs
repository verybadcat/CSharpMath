using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace CSharpMath.Forms.Example
{
  static class SetCurrentPageIndexExtension {
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
          new ExamplePage(), new AllExamplesPage(), new CustomExamplePage(), new SettingsPage()
        }
      }.SetCurrentPageIndex(0);
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

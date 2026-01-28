namespace CSharpMath.Maui.Example {
  public partial class AppShell : Shell {
    public AppShell() {
      InitializeComponent();
    }
    private void ThemeChange(object sender, EventArgs e) {
      Application.Current!.UserAppTheme = Application.Current.RequestedTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
    }
  }
}
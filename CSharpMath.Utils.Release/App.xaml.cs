using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Windows;

namespace CSharpMath.Utils.Release {
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application {
    /// <summary>
    /// The path of the global CSharpMath folder
    /// </summary>
    public static readonly string Global = ((Func<string>)(() => {
      var L = typeof(App).Assembly.Location;
      while (Path.GetFileName(L) != nameof(CSharpMath)) L = Path.GetDirectoryName(L);
      return L;
    }))();

    public static readonly string ReleaseData =
      Path.Combine(Path.Combine(Global, $"{nameof(CSharpMath)}.{nameof(Utils)}.{nameof(Release)}"), "ReleaseData.xml");

    protected override void OnStartup(StartupEventArgs e) {
      base.OnStartup(e);
      var e1 = new Editor { Left = 50 };
      e1.Show();
      new Editor2() { Left = e1.Left + e1.Width, Top = e1.Top }.Show();
    }
  }
}

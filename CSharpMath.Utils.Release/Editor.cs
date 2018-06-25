using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharpMath.Utils.Release {
  public partial class Editor : Form {
    /// <summary>
    /// The path of the global CSharpMath folder
    /// </summary>
    public static readonly string Global = ((Func<string>)(() => {
      var L = typeof(Editor).Assembly.Location;
      while (Path.GetFileName(L) != nameof(CSharpMath)) L = Path.GetDirectoryName(L);
      return L;
    }))();

    public Editor() {
      InitializeComponent();
    }
  }
}

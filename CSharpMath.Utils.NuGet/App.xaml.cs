using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Xml;

namespace CSharpMath.Utils.NuGet {
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
      Path.Combine(Path.Combine(Global,
        $"{nameof(CSharpMath)}.{nameof(Utils)}.{nameof(NuGet)}"), "ReleaseData.xml");

    protected override void OnStartup(StartupEventArgs e) {
      base.OnStartup(e);
      var e1 = new Editor { Left = 50 };
      e1.Show();
      new Editor2() { Left = e1.Left + e1.Width, Top = e1.Top }.Show();
    }

    public static void UpdateProject(string project) {
      var d = new XmlDocument();
      var csproj = new XmlDocument();
      d.Load(ReleaseData);
      var tags = string.Empty;
      var files = new List<string>(Directory.GetFiles
        (Global, project + ".csproj", SearchOption.AllDirectories));
      files.AddRange(Directory.GetFiles(
        Global,
        project.Insert(project.LastIndexOf('.') + 1, "?")
          + "?.csproj", //The ~ character in project names!
        SearchOption.AllDirectories));
      //Avoid "Cannot switch to Unicode" errors => use 'true'
      using (var reader = new StreamReader(files[0], true)) {
        csproj.Load(reader);
      }
      var propGroup = (XmlElement)csproj.LastChild.FirstChild;
      if (propGroup.Name != "PropertyGroup") {
        Debugger.Break(); //Something fishy
        propGroup = (XmlElement)
          ((XmlElement)csproj.LastChild).GetElementsByTagName("PropertyGroup")[0];
      }
      XmlElement GetProjectProperty(string node) {
        var list = propGroup.GetElementsByTagName(node);
        if (list.Count == 0) {
          var newnode = csproj.CreateElement(node);
          propGroup.AppendChild(newnode);
          return newnode;
        } else return (XmlElement)list[0];
      }
      foreach (XmlElement e in d.LastChild["_Global-"]) {
        if (e.Name == "PackageTags") tags += e.InnerText + ";";
        else {
          GetProjectProperty(e.Name).InnerText = e.InnerText;
          for (int i = 0; i < e.Attributes.Count; i++) {
            GetProjectProperty(e.Name)
              .SetAttributeNode((XmlAttribute)e.Attributes[i].CloneNode(true));
          }
        }
      }
      foreach (XmlElement e in d.LastChild[project]) {
        if (e.Name == "PackageTags") tags += e.InnerText + ";";
        else {
          GetProjectProperty(e.Name).InnerText = e.InnerText;
          for (int i = 0; i < e.Attributes.Count; i++) {
            GetProjectProperty(e.Name)
              .SetAttributeNode((XmlAttribute)e.Attributes[i].CloneNode(true));
          }
        }
      }
      GetProjectProperty("PackageTags").InnerText = tags.TrimEnd(';');
      ;
      using (var writer = new StreamWriter(files[0])) {
        csproj.Save(XmlWriter.Create(writer, new XmlWriterSettings {
          NewLineChars = Environment.NewLine,
          OmitXmlDeclaration = true,
          Encoding = Encoding.UTF8,
          IndentChars = "  ",
          Indent = true,
          CloseOutput = true,
        }));
      }
    }


    public static void UpdateAllProjects() {
      foreach (var project in ViewModel2.ProjectNames) {
        UpdateProject(project);
      }
    }
  }
}

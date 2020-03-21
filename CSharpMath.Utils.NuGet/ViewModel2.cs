using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Xml;
using System.Windows.Input;

namespace CSharpMath.Utils.NuGet {
  public class ViewModel2 : INotifyPropertyChanged {
    public static string[] ProjectNames { get; } =
      new[] {
        //Add projects here
        "CSharpMath",
        "CSharpMath.Editor",
        "CSharpMath.Rendering",
        "CSharpMath.Ios",
        "CSharpMath.Avalonia",
        "CSharpMath.SkiaSharp",
        "CSharpMath.Forms"
        //NOTE: When a new project is added, first save project spec, NOT global spec
      };
    public event PropertyChangedEventHandler PropertyChanged;
    private void Set<T>(T _, [CallerMemberName] string propertyName = null) =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    public ViewModel2() => Initialize();
    public void Initialize() {
      var d = new XmlDocument();
      using (var s = new FileStream(App.ReleaseData, FileMode.Open))
        d.Load(s);
      var root = (XmlElement)d.LastChild;
      XmlElement GetElement(string node) {
        var list = root.GetElementsByTagName(node);
        if (list.Count == 0) {
          var newnode = d.CreateElement(node);
          newnode.InnerXml = root.GetElementsByTagName("_Template-")[0].InnerXml;
          root.AppendChild(newnode);
          return newnode;
        } else return (XmlElement)list[0];
      }
      foreach (var info in Projects) {
        var e = GetElement(info.Project);
        string GetValue(string name) =>
          e.GetElementsByTagName(name)[0].InnerText;
        info.PackageId = GetValue(nameof(info.PackageId));
        info.Title = GetValue(nameof(info.Title));
        info.Description = GetValue(nameof(info.Description));
        info.PackageTags = GetValue(nameof(info.PackageTags));
      }
    }
    
    public ReadOnlyCollection<ProjectInfo> Projects { get; } =
      ProjectNames.Select(p => new ProjectInfo(p)).ToList().AsReadOnly();
    ProjectInfo _currentInfo;
    public ProjectInfo CurrentInfo {
      get => _currentInfo;
      set {
        Set(_currentInfo = value);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PackageId)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PackageTags)));
      }
    }
    public string PackageId
      { get => CurrentInfo.PackageId; set => Set(CurrentInfo.PackageId = value); }
    public string Title
      { get => CurrentInfo.Title; set => Set(CurrentInfo.Title = value); }
    public string Description
      { get => CurrentInfo.Description; set => Set(CurrentInfo.Description = value); }
    public string PackageTags
      { get => CurrentInfo.PackageTags; set => Set(CurrentInfo.PackageTags = value); }
    public class ProjectInfo {
      public ProjectInfo(string project) => Project = project;
      public string Project { get; }
      public string PackageId { get; set; }
      public string Title { get; set; }
      public string Description { get; set; }
      public string PackageTags { get; set; }
      public override string ToString() => Project;
    }
    public ICommand Reload => new Command(Initialize);
    public void SaveFile() {
      using (var file = new FileStream(App.ReleaseData, FileMode.Open)) {
        var d = new XmlDocument();
        d.Load(file);
        var root = (XmlElement)d.LastChild;
        XmlElement GetElement(string node) {
          var list = root.GetElementsByTagName(node);
          if (list.Count == 0) {
            var newnode = d.CreateElement(node);
            newnode.InnerXml = root.GetElementsByTagName("_Template-")[0].InnerXml;
            root.AppendChild(newnode);
            return newnode;
          } else return (XmlElement)list[0];
        }
        foreach (var info in Projects) {
          var e = GetElement(info.Project);
          void SetValue(string name, string value) =>
            e.GetElementsByTagName(name)[0].InnerText = value;
          SetValue(nameof(info.PackageId), info.PackageId);
          SetValue(nameof(info.Title), info.Title);
          SetValue(nameof(info.Description), info.Description);
          SetValue(nameof(info.PackageTags), info.PackageTags);
        }
        //Prevents XML from appending to the end of the file
        file.Seek(0, SeekOrigin.Begin);
        d.Save(file); //Save to the file
        file.SetLength(file.Position); //Gets rid of residue after this position
      }
      App.UpdateProject(CurrentInfo.Project);
      //MVVM broken here
      new Avalonia.Controls.Window {
        Content = new Avalonia.Controls.TextBlock {
          Text = $"Project-wise spec for {CurrentInfo.Project} was saved successfully."
        }
      }.Show();
    }
    public ICommand Save => new Command(SaveFile);
  }
}

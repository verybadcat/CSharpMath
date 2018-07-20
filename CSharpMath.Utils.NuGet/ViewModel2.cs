using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Input;

namespace CSharpMath.Utils.NuGet {
  public class ViewModel2 : INotifyPropertyChanged {
    public event PropertyChangedEventHandler PropertyChanged;
    private void Set<T>(T assignment, [CallerMemberName] string propertyName = null) =>
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

    public static string[] ProjectNames { get; } =
      new[] {
        //Add projects here
        "CSharpMath",
        "CSharpMath.Rendering",
        "CSharpMath.Ios",
        "CSharpMath.SkiaSharp",
        "CSharpMath.Forms"
        //NOTE: When a new project is added, first save project spec, NOT global spec
      };
    
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

    public string PackageId { get => CurrentInfo.PackageId; set => Set(CurrentInfo.PackageId = value); }

    public string Title { get => CurrentInfo.Title; set => Set(CurrentInfo.Title = value); }

    public string Description { get => CurrentInfo.Description; set => Set(CurrentInfo.Description = value); }

    public string PackageTags { get => CurrentInfo.PackageTags; set => Set(CurrentInfo.PackageTags = value); }

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
      using (var file = new FileStream(App.ReleaseData, FileMode.Open))
      using (var memory = new MemoryStream()) {
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
        d.Save(memory); //Not directly to file so that we can know its actual length
        file.SetLength(memory.Length); //Gets rid of residue when memory.Length < file.Length
        file.Seek(0, SeekOrigin.Begin); //Prevents XML from appending to the end of the file
        memory.WriteTo(file); //Actually save the file
      }
      App.UpdateProject(CurrentInfo.Project);
      System.Windows.MessageBox.Show($"Project-wise spec for {CurrentInfo.Project} was saved successfully."); //MVVM broken here
    }

    public ICommand Save => new Command(SaveFile);
  }
}

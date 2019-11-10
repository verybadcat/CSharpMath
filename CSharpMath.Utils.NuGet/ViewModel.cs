using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using System.Xml;
using System.Xml.Serialization;

namespace CSharpMath.Utils.NuGet {
  public class ViewModel : INotifyPropertyChanged {
    public event PropertyChangedEventHandler PropertyChanged;
    private void Set<T>(T assignment, [CallerMemberName] string propertyName = null) =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public ViewModel() => Initialize();

    public void Initialize() {
      using (var s = new FileStream(App.ReleaseData, FileMode.Open)) {
        var r = new XmlSerializer(typeof(XmlElement));
        var e = (XmlElement)r.Deserialize(s);
        var global = (XmlElement)e.GetElementsByTagName("_Global-")[0];
        string GetValue(string name) =>
          ((XmlElement)global.GetElementsByTagName(name)[0]).InnerText;
        GeneratePackageOnBuild = bool.Parse(GetValue(nameof(GeneratePackageOnBuild)));
        PackageVersion = GetValue(nameof(PackageVersion));
        Authors = GetValue(nameof(Authors));
        PackageReleaseNotes = GetValue(nameof(PackageReleaseNotes));
        RepositoryType = GetValue(nameof(RepositoryType));
        RepositoryUrl = GetValue(nameof(RepositoryUrl));
        RepositoryBranch = GetValue(nameof(RepositoryBranch));
        RepositoryCommit = GetValue(nameof(RepositoryCommit));
        PackageProjectUrl = GetValue(nameof(PackageProjectUrl));
        PackageRequireLicenseAcceptance = bool.Parse(GetValue(nameof(PackageRequireLicenseAcceptance)));
        PackageLicenseExpression = GetValue(nameof(PackageLicenseExpression));
        PackageIcon = GetValue(nameof(PackageIcon));
        Copyright = GetValue(nameof(Copyright));
        PackageTags = GetValue(nameof(PackageTags));
      }
    }

    bool _packOnBuild_Release;
    public bool GeneratePackageOnBuild { get => _packOnBuild_Release; set => Set(_packOnBuild_Release = value); }

    string _version;
    public string PackageVersion { get => _version; set => Set(_version = value); }

    string _authors;
    public string Authors { get => _authors; set => Set(_authors = value); }

    string _releaseNotes;
    public string PackageReleaseNotes { get => _releaseNotes; set => Set(_releaseNotes = value); }

    string _repoType;
    public string RepositoryType { get => _repoType; set => Set(_repoType = value); }

    string _repoURL;
    public string RepositoryUrl { get => _repoURL; set => Set(_repoURL = value); }

    string _repoBranch;
    public string RepositoryBranch { get => _repoBranch; set => Set(_repoBranch = value); }

    string _repoCommit;
    public string RepositoryCommit { get => _repoCommit; set => Set(_repoCommit = value); }

    string _packageURL;
    public string PackageProjectUrl { get => _packageURL; set => Set(_packageURL = value); }

    bool _licenseRequired;
    public bool PackageRequireLicenseAcceptance { get => _licenseRequired; set => Set(_licenseRequired = value); }

    string _packageLicenseExpression;
    public string PackageLicenseExpression { get => _packageLicenseExpression; set => Set(_packageLicenseExpression = value); }

    string _packageIcon;
    public string PackageIcon { get => _packageIcon; set => Set(_packageIcon = value); }

    string _copyright;
    public string Copyright { get => _copyright; set => Set(_copyright = value); }

    string _tags;
    public string PackageTags { get => _tags; set => Set(_tags = value); }

    public ICommand Reload => new Command(Initialize);

    public void SaveFile() {
      using (var file = new FileStream(App.ReleaseData, FileMode.Open)) {
        var r = new XmlSerializer(typeof(XmlElement));
        var e = (XmlElement)r.Deserialize(file);
        var global = (XmlElement)e.GetElementsByTagName("_Global-")[0];
        void SetValue(string name, string value) =>
          ((XmlElement)global.GetElementsByTagName(name)[0]).InnerText = value;
        SetValue(nameof(GeneratePackageOnBuild), GeneratePackageOnBuild.ToString().ToLowerInvariant());
        ((XmlElement)global.GetElementsByTagName(nameof(GeneratePackageOnBuild))[0]).SetAttribute("Condition", "'$(Configuration)' == 'Release'");
        SetValue(nameof(PackageVersion), PackageVersion);
        SetValue(nameof(Authors), Authors);
        SetValue(nameof(PackageReleaseNotes), PackageReleaseNotes);
        SetValue(nameof(RepositoryType), RepositoryType);
        SetValue(nameof(RepositoryUrl), RepositoryUrl);
        SetValue(nameof(RepositoryBranch), RepositoryBranch);
        SetValue(nameof(RepositoryCommit), RepositoryCommit);
        SetValue(nameof(PackageProjectUrl), PackageProjectUrl);
        SetValue(nameof(PackageRequireLicenseAcceptance), PackageRequireLicenseAcceptance.ToString().ToLowerInvariant());
        SetValue(nameof(PackageLicenseExpression), PackageLicenseExpression);
        SetValue(nameof(PackageIcon), PackageIcon);
        SetValue(nameof(Copyright), Copyright);
        SetValue(nameof(PackageTags), PackageTags);
        file.Seek(0, SeekOrigin.Begin); //Prevents XML from appending to the end of the file
        r.Serialize(file, e); //Save to the file
        file.SetLength(file.Position); //Gets rid of residue after this position
      }
      App.UpdateAllProjects();
      System.Windows.MessageBox.Show("Successfully saved the global spec."); //MVVM broken here
    }

    public ICommand Save => new Command(SaveFile);
  }
}

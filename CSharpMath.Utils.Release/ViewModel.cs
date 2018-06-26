using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Xml;

namespace CSharpMath.Utils.Release {
  public class ViewModel : INotifyPropertyChanged {
    public event PropertyChangedEventHandler PropertyChanged;
    private void Set<T>(T assignment, [CallerMemberName] string propertyName = null) =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public ViewModel() {
      var r = XmlReader.Create(App.ReleaseData);
      
    }

    bool _packOnBuild;
    public bool PackOnBuild { get => _packOnBuild; set => Set(_packOnBuild = value); }

    string _version;
    public string Version { get => _version; set => Set(_version = value); }

    string _authors;
    public string Authors { get => _authors; set => Set(_authors = value); }

    string _releaseNotes;
    public string Prop { get => _releaseNotes; set => Set(_releaseNotes = value); }

    string _repoType;
    public string RepoType { get => _repoType; set => Set(_repoType = value); }

    string _repoURL;
    public string RepoURL { get => _repoURL; set => Set(_repoURL = value); }

    string _repoBranch;
    public string RepoBranch { get => _repoBranch; set => Set(_repoBranch = value); }

    string _repoCommit;
    public string RepoCommit { get => _repoCommit; set => Set(_repoCommit = value); }

    string _packageURL;
    public string PackageURL { get => _packageURL; set => Set(_packageURL = value); }

    string _packageLicenseURL;
    public string PackageLicenseURL { get => _packageLicenseURL; set => Set(_packageLicenseURL = value); }

    string _packageIconURL;
    public string PackageIconURL { get => _packageIconURL; set => Set(_packageIconURL = value); }

    string _copyright;
    public string Copyright { get => _copyright; set => Set(_copyright = value); }

    string _globalPackageTags;
    public string GlobalPackageTags { get => _globalPackageTags; set => Set(_globalPackageTags = value); }
  }
}

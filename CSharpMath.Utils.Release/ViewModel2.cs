using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;

namespace CSharpMath.Utils.Release {
  public class ViewModel2 : INotifyPropertyChanged {
    public event PropertyChangedEventHandler PropertyChanged;
    private void Set<T>(T assignment, [CallerMemberName] string propertyName = null) =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    
    string _currentProject;
    public string CurrentProject { get => _currentProject; set => Set(_currentProject = value); }

    string _packageId;
    public string PackageId { get => _packageId; set => Set(_packageId = value); }

    string _title;
    public string Title { get => _title; set => Set(_title = value); }

    string _description;
    public string Description { get => _description; set => Set(_description = value); }

    string _packageTags;
    public string PackageTags { get => _packageTags; set => Set(_packageTags = value); }
  }
}

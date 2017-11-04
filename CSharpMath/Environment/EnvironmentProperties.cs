using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Environment {
  class EnvironmentProperties {
    public string Name { get; set; }
    public bool Ended { get; set; }
    public int NRows { get; set; }

    public EnvironmentProperties(string name) {
      Name = name;
    }
  }
}

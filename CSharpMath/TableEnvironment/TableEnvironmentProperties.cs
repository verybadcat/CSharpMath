using System.Collections.Generic;
using System.Text;

namespace CSharpMath.TableEnvironment {
  class TableEnvironmentProperties {
    public string Name { get; set; }
    public bool Ended { get; set; }
    public int NRows { get; set; }

    public TableEnvironmentProperties(string name) {
      Name = name;
    }
  }
}

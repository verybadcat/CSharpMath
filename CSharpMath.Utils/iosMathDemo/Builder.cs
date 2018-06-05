using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CSharpMath.DevUtils.iosMathDemo {
  static class Builder {
    public static void Build(string inFile, string outFile) {
      var text = File.ReadAllText(inFile);
      int i = 0;
      for (; !lines[i].Contains("// Demo formulae"); i++) ;
      //i is at "// Demo formulae"

      var regex = new Regex(@"self.demoLabels\[(\d+)\] = \[self createMathLabel:@""(.+)"" withHeight:(\d+)\];");

      for (; !lines[i].Contains("- (void)didReceiveMemoryWarning"); i++) {

      }
    }
  }
}
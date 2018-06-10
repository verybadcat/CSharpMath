using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CSharpMath.DevUtils.iosMathDemo {
  static class Builder {
    public static void Build(string inFile, string outFile) {
      var text = File.ReadAllText(inFile);
      var regex = new Regex(@"self.((?:demoLabels|labels)\[(\d+)\] = )\[self createMathLabel:@("".+"") withHeight:(\d+)\];", RegexOptions.Compiled);
      var m = regex.Matches(text);
      var sb = new StringBuilder("var demoLabels = new List<View>();\nvar labels = new List<View>();\n");
      for (int i = 0; i < m.Count; i++) {
        sb.Append(m[i].Captures[0].Value).Append("new FormsMathView { ");
      }
#error Unfinished!
    }
  }
}
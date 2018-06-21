using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CSharpMath.DevUtils.CSharpMathExamples {
  static class MirrorFromIos {
    //unused helper function
    static bool ReadUntil(this Stream s, string str) {
      byte[] chars = Encoding.UTF8.GetBytes(str);
      byte b;
      matchFirstByte: do {
        int res = s.ReadByte();
        if (res == -1) return false;
        b = (byte)res;
      } while (b != chars[0]);
      //matched first byte
      for (int i = 1; i < chars.Length; i++) if (s.ReadByte() != chars[i]) goto matchFirstByte;
      return true;
    }

    /// <summary>
    /// Mirrors examples from CSharpMath.Ios.Example (IosMathViewController.cs) to CSharpMath.Forms.Example (ExamplesPage.xaml.cs)
    /// </summary>
    public static void Do() {
      var lines = File.ReadLines(Paths.IosMathViewController);
      var output = new List<string>();

      bool lineNotExample = true;
      foreach (var line in lines) {
        if (line.Contains("private const string")) lineNotExample = false;
        else if (line.Contains("public override void ViewDidLoad()")) break;
        if (lineNotExample) continue;
        output.Add(line.Replace(" private ", " public "));
      }

      var writeFile = new List<string>(File.ReadAllLines(Paths.FormsMathExamplesFile));
      int i = 0;
      for (; i < writeFile.Count; i++) {
        if (writeFile[i].Contains("//START mirror CSharpMath.Ios.Example")) break;
      }
      if (i == writeFile.Count) throw new KeyNotFoundException("START not found.");
      int j = i;
      for (; j < writeFile.Count; j++) {
        if (writeFile[j].Contains("//END mirror CSharpMath.Ios.Example")) break;
      }
      if (j == writeFile.Count) throw new KeyNotFoundException("END not found.");
      writeFile.RemoveRange(i + 1, j - i - 1);
      writeFile.InsertRange(i + 1, output);
      File.WriteAllLines(Paths.FormsMathExamplesFile, writeFile);
    }
  }
}
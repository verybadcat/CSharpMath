using System.Collections.Generic;
using System.IO;

namespace CSharpMath.DevUtils.CSharpMathExamples {
  static class MirrorFromIos {
    /// <summary>
    /// Mirrors examples from CSharpMath.Ios.Example (IosMathViewController.cs)
    /// to CSharpMath.Forms.Example (ExamplesPage.xaml.cs)
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
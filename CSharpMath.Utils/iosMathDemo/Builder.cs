using System;
using System.CodeDom.Compiler;
using static System.Linq.Enumerable;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace CSharpMath.DevUtils.iosMathDemo {
  /// <summary>
  /// Grabs the latest iosMath demo examples straight from GitHub and translates them into C# then stores them in
  /// MoreExamples.cs located at CSharpMath\CSharpMath.Forms.Example\CSharpMath.Forms.Example\MoreExamples.cs"
  /// </summary>
  static class Builder {
    static IndentedTextWriter Append(this IndentedTextWriter w, string text) { w.Write(text); return w; }
    static IndentedTextWriter AppendLine(this IndentedTextWriter w) { w.WriteLine(); return w; }
    static IndentedTextWriter AppendLine(this IndentedTextWriter w, string text) { w.WriteLine(text); return w; }
    static IndentedTextWriter Expand(this IndentedTextWriter w) { w.Indent++; return w; }
    static IndentedTextWriter Collapse(this IndentedTextWriter w) { w.Indent--; return w; }
    public static void Build() {
      var sb = new StringBuilder();
      string text;
      using (var client = new WebClient())
        text = client.DownloadString(@"https://raw.githubusercontent.com/kostub/iosMath/master/iosMathExample/example/ViewController.m");
      text = text.Substring(text.IndexOf("[self setHeight:4280 forView:contentView];"));
      text = text.Remove(text.IndexOf("- (void)didReceiveMemoryWarning"));
      var regex = new Regex(@"self.(?<label>(?:demoLabels|labels)\[\d+\] = )\[self createMathLabel:@(?<latex>(?:""[^""]*?""\s+)+?)withHeight:(?<height>\d+)\];|" +

                            @"self.(?<colorTarget>(?:demoLabels|labels)\[\d+\]).backgroundColor = \[UIColor colorWithHue:(?<hue>\d+(?:\.\d+)?) " +
                            @"saturation:(?<saturation>\d+(?:\.\d+)?) brightness:(?<brightness>\d+(?:\.\d+)?) alpha:(?<alpha>\d+(?:\.\d+)?)\];|" +

                            @"self.(?<sizeTarget>(?:demoLabels|labels)\[\d+\]).fontSize = (?<size>\d+(?:\.\d+)?);|" +

                            @"self.(?<alignTarget>(?:demoLabels|labels)\[\d+\]).textAlignment = kMTTextAlignment(?<align>\w+);|" +

                            @"self.(?<insetsTarget>(?:demoLabels|labels)\[\d+\]).contentInsets = UIEdgeInsetsMake\(?<insets>([\d\s,]+)\);|" +

                            @"self.(?<styleTarget>(?:demoLabels|labels)\[\d+\]).labelMode = kMTMathUILabelMode(?<style>\w+);|" + 
                            
                            @"\n\s+//(?<comment>.+)", RegexOptions.Compiled);
      var writer = new IndentedTextWriter(new StringWriter(sb), "  ") { Indent = 0, NewLine = Environment.NewLine };
      writer.AppendLine("//Do not modify this file directly. Instead, modify this at")
            .AppendLine("//CSharpMath\\CSharpMath.Utils\\iosMathDemo\\Builder.cs and re-generate")
            .AppendLine("//this file by executing the method in that file in the CSharpMath.Utils project.")
            .AppendLine()
            .AppendLine("using CSharpMath.Enumerations;")
            .AppendLine("using CSharpMath.Rendering;")
            .AppendLine("using System.Collections.Generic;")
            .AppendLine("using System.Collections.ObjectModel;")
            .AppendLine("using System.Linq;")
            .AppendLine("using Color = Xamarin.Forms.Color;")
            .AppendLine()
            .AppendLine("namespace CSharpMath.Forms.Example {")
            .Expand()
            .AppendLine("[System.Diagnostics.DebuggerNonUserCode, System.Runtime.CompilerServices.CompilerGenerated]")
            .AppendLine("public static class MoreExamples {")
            .Expand()
            .AppendLine("public static ReadOnlyCollection<FormsMathView> Views { get; }")
            .AppendLine("static MoreExamples() {")
            .Expand()
            .AppendLine("var demoLabels = new Dictionary<byte, FormsMathView>();")
            .AppendLine("var labels = new Dictionary<byte, FormsMathView>();");
      foreach (var m in regex.Matches(text).AsEnumerable()) {
        if (!string.IsNullOrEmpty(m.Groups["label"].Value))
          writer.Append(m.Groups["label"].Value)
                .AppendLine("new FormsMathView {")
                .Expand()
                .Append("LaTeX = @\"")
                .Append(m.Groups["latex"].Value.Replace("\n ", Environment.NewLine + ' ').Replace("\\\n", Environment.NewLine).Replace(@"\\", @"\").TrimEnd().Replace("\"", ""))
                .AppendLine("\",")
                .Append("HeightRequest = ")
                .Append(m.Groups["height"].Value)
                .AppendLine(",")
                .AppendLine("FontSize = 15")
                .Collapse()
                .AppendLine("};");
        else if (!string.IsNullOrEmpty(m.Groups["colorTarget"].Value))
          writer.Append(m.Groups["colorTarget"].Value)
                .Append(".BackgroundColor = Color.FromHsla(")
                .Append(m.Groups["hue"].Value)
                .Append(", ")
                .Append(m.Groups["saturation"].Value)
                .Append(", ")
                .Append(m.Groups["brightness"].Value)
                .Append(", ")
                .Append(m.Groups["alpha"].Value)
                .AppendLine(");");
        else if (!string.IsNullOrEmpty(m.Groups["sizeTarget"].Value))
          writer.Append(m.Groups["sizeTarget"].Value)
                .Append(".FontSize = ")
                .Append(m.Groups["size"].Value)
                .AppendLine(";");
        else if (!string.IsNullOrEmpty(m.Groups["alignTarget"].Value))
          writer.Append(m.Groups["alignTarget"].Value)
                .Append(".TextAlignment = TextAlignment.")
                .Append(m.Groups["align"].Value)
                .AppendLine(";");
        else if (!string.IsNullOrEmpty(m.Groups["insetsTarget"].Value))
          writer.Append(m.Groups["insetsTarget"].Value)
                .Append(".Padding = new Thickness(")
                .Append(m.Groups["insets"].Value)
                .AppendLine(");");
        else if (!string.IsNullOrEmpty(m.Groups["styleTarget"].Value))
          writer.Append(m.Groups["styleTarget"].Value)
                .Append(".LineStyle = LineStyle.")
                .Append(m.Groups["style"].Value)
                .AppendLine(";");
        else if (!string.IsNullOrEmpty(m.Groups["comment"].Value))
          writer.AppendLine()
                .Append(@"// ")
                .AppendLine(m.Groups["comment"].Value);
      }
      writer.AppendLine()
            .AppendLine("Views = demoLabels.Concat(labels).Select(p => p.Value).ToList().AsReadOnly();")
            .Collapse()
            .AppendLine("}")
            .Collapse()
            .AppendLine("}")
            .Collapse()
            .AppendLine("}")
            .Collapse();
      if (writer.Indent != 0)
        throw new InvalidOperationException("Indents are not balanced.");
      File.WriteAllText(Paths.iosMathExamplesFile, sb.ToString());
    }
  }
}
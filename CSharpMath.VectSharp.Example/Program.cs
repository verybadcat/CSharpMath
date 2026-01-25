using System.IO;
using VectSharp;
using VectSharp.PDF;
using VectSharp.SVG;
using CSharpMath.VectSharp;

var painter = new TextPainter {
  LaTeX = @"Let's render some math to a PDF and an SVG file!$$x = {-b \pm \color{red}\sqrt{b^2-4ac} \over 2a}$$"
};
var page = painter.DrawToPage(400f); // adjust width here
var doc = new Document { Pages = { page } };

// PDF
var tempFileName = Path.Combine(Path.GetTempPath(), "CSharpMath.pdf");
doc.SaveAsPDF(tempFileName);
System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
  FileName = tempFileName,
  UseShellExecute = true
});

// SVG
tempFileName = Path.Combine(Path.GetTempPath(), "CSharpMath.svg");
page.SaveAsSVG(tempFileName);
System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
  FileName = tempFileName,
  UseShellExecute = true
});
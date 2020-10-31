using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace CSharpMath.Forms.Tests {
  public static class ButtonTestsHelper {
    public static bool ImageSourceEquals(this ImageButton imageButton, string expectedImageRelativePath) =>
      ImagesAreEqual(new FileInfo(expectedImageRelativePath), ((StreamImageSource)imageButton.Source).Stream(System.Threading.CancellationToken.None).Result);
    static bool ImagesAreEqual(FileInfo f, Stream s) {
      using (FileStream fs = f.OpenRead()) {
        int b;
        while ((b = fs.ReadByte()) != -1) {
          if (s.ReadByte() != b) {
            return false;
          }
        }
        return fs.ReadByte() == -1;
      }
    }
    /// <summary>
    /// For creating unit tests.
    /// </summary>
    public static void ButtonImageSourceToFile(this ImageButton imageButton, string relativeDestinationFilePath) {
      using (var fs = new FileInfo(@"..\..\..\" + relativeDestinationFilePath).Create())
        ((StreamImageSource)imageButton.Source).Stream(System.Threading.CancellationToken.None).Result.CopyTo(fs);
    }
  }
}

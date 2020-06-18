using System;
using System.Linq;
using System.Collections.Generic;

using Foundation;
using UIKit;

namespace CSharpMath.Ios.Tests {
  public class Application {
    // This is the main entry point of the application.
    static void Main(string[] args) {
      // Write Debug.WriteLine to StdErr: https://github.com/dotnet/runtime/blob/1cfa461bbf071fbc71ceb5e105e1d39d0c077f25/src/libraries/System.Private.CoreLib/src/System/Diagnostics/DebugProvider.Unix.cs#L9
      Environment.SetEnvironmentVariable("COMPlus_DebugWriteToStdErr", "1");

      // if you want to use a different Application Delegate class from "AppDelegate"
      // you can specify it here.
      UIApplication.Main(args, null, "AppDelegate");
    }
  }
}

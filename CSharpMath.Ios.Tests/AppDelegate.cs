using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Foundation;
using UIKit;

using Xunit.Runner;
using Xunit.Sdk;


namespace CSharpMath.Ios.Tests {
  // The UIApplicationDelegate for the application. This class is responsible for launching the 
  // User Interface of the application, as well as listening (and optionally responding) to 
  // application events from iOS.
  [Register("AppDelegate")]
  public partial class AppDelegate : RunnerAppDelegate {

    //
    // This method is invoked when the application has loaded and is ready to run. In this 
    // method you should instantiate the window, load the UI into it and then make the window
    // visible.
    //
    // You have 17 seconds to return from this method, or iOS will terminate your application.
    //
    public override bool FinishedLaunching(UIApplication app, NSDictionary options) {
      // Write Debug.WriteLine to StdErr: https://github.com/dotnet/runtime/blob/1cfa461bbf071fbc71ceb5e105e1d39d0c077f25/src/libraries/System.Private.CoreLib/src/System/Diagnostics/DebugProvider.Unix.cs#L9
      Environment.SetEnvironmentVariable("COMPlus_DebugWriteToStdErr", "1");

      // We need this to ensure the execution assembly is part of the app bundle
      AddExecutionAssembly(typeof(ExtensibilityPointFactory).Assembly);
      
      
      // tests can be inside the main assembly
      AddTestAssembly(Assembly.GetExecutingAssembly());
      // otherwise you need to ensure that the test assemblies will 
      // become part of the app bundle
      // AddTestAssembly(typeof(PortableTests).Assembly);
      
      // start running the test suites as soon as the application is loaded
      AutoStart = true;
      // crash the application (to ensure it's ended) and return to springboard
#if CI
      TerminateAfterExecution = true;
#endif
      return base.FinishedLaunching(app, options);
    }
  }
}
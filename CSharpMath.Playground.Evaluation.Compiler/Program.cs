using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Net.Http;
using Jurassic;
using Jurassic.Compiler;
using Jurassic.Library;
using System.Diagnostics;
using Microsoft.ClearScript.V8;
using System.Threading.Tasks;

namespace CSharpMath.Playground.Evaluation.Compiler {
  class Program {
    class StreamScriptSource : ScriptSource, IDisposable {
      public StreamScriptSource(Stream stream) => this.stream = stream;
      readonly Stream stream;
      public override string? Path => null;
      public override TextReader GetReader() => new StreamReader(stream);
      public void Dispose() => stream.Dispose();
    }
    static string ThisDirectory([System.Runtime.CompilerServices.CallerFilePath] string? path = null) =>
        Path.GetDirectoryName(path ?? throw new ArgumentNullException(nameof(path)));
    static async Task Main() {
      Console.WriteLine("1 out of 6: Entered Main");

      static string ReadNeradmerFile(string file) =>
        File.ReadAllText(Path.Combine(ThisDirectory(), "..", "nerdamer", file));
      using var http = new HttpClient();
      using var clearScript = new V8ScriptEngine();
      clearScript.Execute(await http.GetStringAsync("https://unpkg.com/@babel/standalone@7.9.4/babel.min.js"));
      Console.WriteLine("2 out of 6: Loaded Babel");

      clearScript.AddHostObject("nerdamer", new {
        Core = ReadNeradmerFile("nerdamer.core.js"),
        Algebra = ReadNeradmerFile("Algebra.js"),
        Calculus = ReadNeradmerFile("Calculus.js"),
        Solve = ReadNeradmerFile("Solve.js"),
        Extra = ReadNeradmerFile("Extra.js"),
      });
      dynamic transformed = clearScript.Evaluate(@"({
        Core: Babel.transform(nerdamer.Core, { presets: ['env'] }).code,
        Algebra: Babel.transform(nerdamer.Algebra, { presets: ['env'] }).code,
        Calculus: Babel.transform(nerdamer.Calculus, { presets: ['env'] }).code,
        Solve: Babel.transform(nerdamer.Solve, { presets: ['env'] }).code,
        Extra: Babel.transform(nerdamer.Extra, { presets: ['env'] }).code
      })");
      Console.WriteLine("3 out of 6: Transformed Nerdamer");

      var compiler = new ScriptCompiler();
      compiler.IncludeInput(transformed.Core);
      compiler.IncludeInput(transformed.Algebra);
      compiler.IncludeInput(transformed.Calculus);
      compiler.IncludeInput(transformed.Solve);
      compiler.IncludeInput(transformed.Extra);
      compiler.Save(Path.Combine(ThisDirectory(), "nerdamer.dll"));
      Console.WriteLine("6 out of 6: Done!");
    }
  }

  ///<summary>https://github.com/paulbartrum/jurassic/issues/15#issuecomment-206990807</summary>
  public class ScriptCompiler {
    private readonly List<string> codesInputs = new List<string>();

    public void IncludeInput(string code) {
      codesInputs.Add(code);
    }

    public void Save(string dllPath) {
      var fullpath = Path.GetFullPath(dllPath);

      var assemblyName = Path.GetFileNameWithoutExtension(fullpath);
      var assemblyBuilder =
          AppDomain.CurrentDomain.DefineDynamicAssembly(
              new AssemblyName(assemblyName),
              AssemblyBuilderAccess.RunAndSave,
              Path.GetDirectoryName(fullpath));

      var module = assemblyBuilder.DefineDynamicModule("Module", Path.GetFileName(fullpath), true);

      var engine = new ScriptEngine {
        EnableDebugging = true
      };
      var info = Activator.CreateInstance(typeof(ScriptEngine).Assembly.GetType("Jurassic.Compiler.MethodGenerator+ReflectionEmitModuleInfo"));
      info.GetType().GetField("AssemblyBuilder").SetValue(info, assemblyBuilder);
      info.GetType().GetField("ModuleBuilder").SetValue(info, module);
      typeof(ScriptEngine).Assembly.GetType("Jurassic.Compiler.MethodGenerator").GetField("ReflectionEmitInfo", BindingFlags.NonPublic | BindingFlags.Static).SetValue(engine, info);

      Console.WriteLine("4 out of 6: Including code");
      int codei = 0;
      foreach (var code in codesInputs) {
        engine.Execute(code);
        Console.WriteLine("5 out of 6: Included code #" + codei++);
      }

      var generatedTypes = module.GetTypes();
      var userType = module.DefineType(assemblyName, TypeAttributes.Public);
      var restoreMethod = userType.DefineMethod("RestoreScriptEngine", MethodAttributes.Public | MethodAttributes.Static, typeof(ScriptEngine), new Type[] { });
      var restoreMethodBody = restoreMethod.GetILGenerator();

      var generator = typeof(ScriptEngine).Assembly
        .GetType("Jurassic.Compiler.ReflectionEmitILGenerator")
        .GetConstructor(new[] { restoreMethodBody.GetType(), typeof(bool) })
        .Invoke(new object[] { restoreMethodBody, true });

      // Local : engine = new ScriptEngine()
      var loadedEngine = restoreMethodBody.DeclareLocal(typeof(ScriptEngine));
      restoreMethodBody.EmitCall(OpCodes.Call, typeof(Assembly).GetMethod(nameof(Assembly.GetExecutingAssembly)), new Type[] { });
      restoreMethodBody.EmitCall(OpCodes.Call, typeof(ScriptCompiler).GetMethod("Load"), new Type[] { });
      restoreMethodBody.Emit(OpCodes.Ret);
      userType.CreateType();

      assemblyBuilder.Save(Path.GetFileName(dllPath));
    }

    public static void AddToFunctionCache(long id, FunctionDelegate functionDelegate, GeneratedMethod[] dependencies) {
      GMethod.AddToMethodCache(id, new GeneratedMethod(functionDelegate, dependencies));
    }

    public static ScriptEngine Load(Assembly assembly) {
      var engine = new ScriptEngine();
      var globalScope = /*engine.CreateGlobalScope();*/
      ObjectScope.CreateRuntimeScope(null, engine.Global, false, false);

      var sw = new Stopwatch();

      sw.Start();

      var methods = assembly
          .GetTypes()
          .Where(t => t.GetMethods().Any(IsJavaScriptFunction))
          .Select(t => {
            return
                new {
                  Id = (long)t.GetMethod("GetFunctionId").Invoke(null, new object[] { }),
                  Function = t.GetMethods().SingleOrDefault(IsJavaScriptFunction),
                  Dependencies = (long[])t.GetMethod("GetDependencyIds").Invoke(null, new object[] { })
                };
          }
          ).ToList();


      GMethod.GeneratedMethodID = 0;
      GMethod.GeneratedMethodCache = new Dictionary<long, WeakReference>();
      int i = 0;

      while (methods.Count != 0) {
        bool foundDependency = true;
        var func = methods[i];

        var functionDelegate = (FunctionDelegate)Delegate.CreateDelegate(typeof(FunctionDelegate), func.Function);

        List<GeneratedMethod> dependencies = new List<GeneratedMethod>();
        foreach (var m in func.Dependencies) {
          foundDependency = GMethod.GeneratedMethodCache.TryGetValue(m, out var refer);

          if (!foundDependency) {
            break;
          }

          dependencies.Add((GeneratedMethod)refer.Target);
        }

        if (foundDependency) {
          AddToFunctionCache(func.Id, functionDelegate, dependencies.ToArray());
          GMethod.GeneratedMethodID++;
          methods.Remove(func);
          i = 0;
        } else {
          i = i < methods.Count - 1 ? i + 1 : 0;
        }
      }

      Console.WriteLine("Functions" + sw.ElapsedMilliseconds / 1000.0);



      var globals = assembly
          .GetTypes()
          .SelectMany(t => t.GetMethods())
          .Where(m => m.Name == "global_")
          .Select(m => (Func<ScriptEngine, Scope, object, object>)Delegate.CreateDelegate(typeof(Func<ScriptEngine, Scope, object, object>), m))
          .ToList();

      sw.Reset();
      sw.Start();
      object obj = engine.Global;
      foreach (var global in globals) {
        global(engine, globalScope, obj);
      }

      Console.WriteLine("globals: " + sw.ElapsedMilliseconds / 1000.0);

      return engine;
    }

    public static bool IsJavaScriptFunction(MethodInfo info) {
      if (info == null) {
        return false;
      }

      if (info.ReturnParameter?.ParameterType != typeof(object)) {
        return false;
      }

      var parameters = info.GetParameters();

      if (parameters.Length != 5) {
        return false;
      }

      var result = true;
      result |= parameters[0].ParameterType == typeof(ScriptEngine);
      result |= parameters[1].ParameterType == typeof(Scope);
      result |= parameters[1].ParameterType == typeof(object);
      result |= parameters[1].ParameterType == typeof(FunctionInstance);
      result |= parameters[1].ParameterType == typeof(object[]);

      return result;
    }
  }
  class GMethod {
    public static long GeneratedMethodID { get; set; }
    public static Dictionary<long, WeakReference> GeneratedMethodCache { get; set; } = new Dictionary<long, WeakReference>();
    public static Dictionary<GeneratedMethod, long> InverseCache { get; set; } = new Dictionary<GeneratedMethod, long>();
    public static long[] IdCache { get; set; } = { };
    public static void AddToMethodCache(long id, GeneratedMethod generatedMethod) {
      InverseCache.Add(generatedMethod, id);
      var weakReference = new WeakReference(generatedMethod);
      GeneratedMethodCache.Add(id, weakReference);

      if (generatedMethod.Dependencies == null) {
        return;
      }
      var dependancyList = new List<long>();
      foreach (var method in generatedMethod.Dependencies) {
        dependancyList.Add(InverseCache[generatedMethod]);
      }
      IdCache = dependancyList.ToArray();
    }
  }
}
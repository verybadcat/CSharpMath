using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Jurassic;
using Jurassic.Compiler;
using Jurassic.Library;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace CSharpMath.Utils.EvaluationExperiment {
  class Program {
    class StreamScriptSource : ScriptSource, IDisposable {
      public StreamScriptSource(Stream stream) => this.stream = stream;
      Stream stream;
      public override string? Path => null;
      public override TextReader GetReader() => new StreamReader(stream);
      public void Dispose() => stream.Dispose();
    }
    static string ThisDirectory([System.Runtime.CompilerServices.CallerFilePath] string? path = null) =>
        Path.GetDirectoryName(path ?? throw new ArgumentNullException(nameof(path)));
    static async Task Main() {
      static string GetNeradmerFile(string file) =>
        Path.Combine(ThisDirectory(), "..", "nerdamer", file);
      var babelEval = new ScriptEngine();
      babelEval.Execute("var self = {};");
      var babel = babelEval.Evaluate(new FileScriptSource(Path.Combine(ThisDirectory(), "babel7.9.4-passed-through-babel.js"), Encoding.UTF8));
      ;
      //var nerdamercore = babel.ev(GetNeradmerFile("nerdamer.core.js"));
      //var algebra = babel.TransformFile(GetNeradmerFile("Algebra.js"));
      //var calculus = babel.TransformFile(GetNeradmerFile("Calculus.js"));
      //var solve = babel.TransformFile(GetNeradmerFile("Solve.js"));
      //var extra = babel.TransformFile(GetNeradmerFile("Extra.js"));
      //var compiler = new ScriptCompiler();
      //compiler.IncludeInput(nerdamercore);
      //compiler.IncludeInput(algebra);
      //compiler.IncludeInput(calculus);
      //compiler.IncludeInput(solve);
      //compiler.IncludeInput(extra);
      //compiler.Save(Path.Combine(ThisDirectory(), "nerdamer.dll"));
    }
  }

  public class ScriptCompiler {
    private List<string> codesInputs = new List<string>();

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


      foreach (var code in codesInputs) {
        engine.Execute(code);
      }

      var generatedTypes = module.GetTypes();
      var userType = module.DefineType(assemblyName, TypeAttributes.Public);
      var restoreMethod = userType.DefineMethod("RestoreScriptEngine", MethodAttributes.Public | MethodAttributes.Static, typeof(ScriptEngine), new Type[] { });
      var restoreMethodBody = restoreMethod.GetILGenerator();

      var generator = typeof(ScriptEngine).Assembly
        .GetType("Jurassic.Compiler.ReflectionEmitILGenerator")
        .GetConstructor(new[] { restoreMethod.GetType() })
        .Invoke(new[] { restoreMethodBody });

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
      Jurassic.Compiler.ObjectScope.CreateRuntimeScope(null, engine.Global, false, false);

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


      GMethod.generatedMethodID = 0;
      GMethod.generatedMethodCache = new Dictionary<long, WeakReference>();
      int i = 0;

      while (methods.Count != 0) {
        bool foundDependency = true;
        var func = methods[i];

        var functionDelegate = (FunctionDelegate)Delegate.CreateDelegate(typeof(FunctionDelegate), func.Function);

        List<GeneratedMethod> dependencies = new List<GeneratedMethod>();
        foreach (var m in func.Dependencies) {
          WeakReference refer;
          foundDependency = GMethod.generatedMethodCache.TryGetValue(m, out refer);

          if (!foundDependency) {
            break;
          }

          dependencies.Add((GeneratedMethod)refer.Target);
        }

        if (foundDependency) {
          AddToFunctionCache(func.Id, functionDelegate, dependencies.ToArray());
          GMethod.generatedMethodID++;
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
    public static long generatedMethodID { get; set; }
    public static Dictionary<long, WeakReference> generatedMethodCache { get; set; } = new Dictionary<long, WeakReference>();
    public static Dictionary<GeneratedMethod, long> inverseCache { get; set; } = new Dictionary<GeneratedMethod, long>();
    public static long[] idCache { get; set; } = { };
    public static void AddToMethodCache(long id, GeneratedMethod generatedMethod) {
      inverseCache.Add(generatedMethod, id);
      var weakReference = new WeakReference(generatedMethod);
      generatedMethodCache.Add(id, weakReference);

      if (generatedMethod.Dependencies == null) {
        return;
      }
      var dependancyList = new List<long>();
      foreach (var method in generatedMethod.Dependencies) {
        dependancyList.Add(inverseCache[generatedMethod]);
      }
      idCache = dependancyList.ToArray();
    }
  }
}
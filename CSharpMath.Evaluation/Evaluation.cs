using System;
using System.Collections.Generic;
using System.Linq;
using AngouriMath;

namespace CSharpMath {
  using Atom;
  using Atoms = Atom.Atoms;
  public static partial class Evaluation {
    enum Precedence {
      // Order matters
      Lowest,
      Bracket,
      AddSubtract,
      MultiplyDivide,
      FunctionApplication,
      UnaryPlusMinus,
      PercentDegree
    }
    public abstract class MathItem : AngouriMath.Core.Sys.Interfaces.ILatexiseable {
      private protected MathItem() { }
      public abstract string Latexise();
      public static implicit operator MathItem(AngouriMath.Entity content) => new Entity(content);
      public static explicit operator AngouriMath.Entity(MathItem item) => ((Entity)item).Content;
      public static implicit operator MathItem(AngouriMath.Core.SetNode content) => new Set(content);
      public static explicit operator AngouriMath.Core.SetNode(MathItem item) => ((Set)item).Content;
      /// <summary>An real number, complex number, variable or function call</summary>
      public sealed class Entity : MathItem {
        public Entity(AngouriMath.Entity content) => Content = content;
        public AngouriMath.Entity Content { get; }
        public override string Latexise() => Content.Latexise();
      }
      /// <summary>A set or collection of set operations</summary>
      public sealed class Set : MathItem {
        public Set(AngouriMath.Core.SetNode content) => Content = content;
        public AngouriMath.Core.SetNode Content { get; }
        public override string Latexise() => Content.Latexise();
      }
    }
    public static MathList MathListFromEntity(MathItem entity) =>
      LaTeXParser.MathListFromLaTeX(entity.Latexise())
      // CSharpMath must handle all LaTeX coming from MathS or a bug is present!
      .Match(list => list, e => throw new Structures.InvalidCodePathException(e));
    public static Structures.Result<MathItem> MathListToEntity(MathList mathList) {
      MathS.pi.ToString(); // Call into MathS's static initializer to ensure Entity methods work
      return Transform(mathList.Clone(true))
      .Bind(result =>
        result is { } r
        ? Structures.Result.Ok(r)
        : Structures.Result.Err("There is nothing to evaluate"));
    }
    static Structures.Result<MathItem?> Transform(MathList mathList) {
      int i = 0;
      return Transform(mathList, ref i, Precedence.Lowest);
    }
    static Structures.Result<Entity?> ExpectEntityOrNull(this Structures.Result<MathItem?> result, string itemName) =>
      result.Bind(item => item switch {
        null => Structures.Result.Ok((Entity?)null),
        MathItem.Entity entity => Structures.Result.Ok((Entity?)entity.Content),
        var notEntity => Structures.Result.Err(item.GetType().Name + " cannot be " + itemName)
      });
    static Structures.Result<Entity> ExpectEntity(this Structures.Result<MathItem?> result, string itemName) =>
      result.ExpectEntityOrNull(itemName).Bind(item => item switch {
        null => Structures.Result.Err("Missing " + itemName),
        { } entity => Structures.Result.Ok(entity)
      });
    static Structures.Result<MathItem> ExpectNotNull(this Structures.Result<MathItem?> result, string itemName) =>
      result.Bind(item => item switch {
        null => Structures.Result.Err("Missing " + itemName),
        { } notnull => Structures.Result.Ok(notnull)
      });
    static Structures.Result<MathItem?> Transform(MathList mathList, ref int i, Precedence prec) {
      MathItem? prevEntity = null;
      Entity? nextEntity;
      string? error;
      Precedence handlePrecendence;
      Func<Entity, Entity> handleUnary, handleFunction, handleFunctionInverse;
      Func<Entity, Entity, Entity> handleBinary;
      for (; i < mathList.Count; i++) {
        var atom = mathList[i];
        MathItem? thisEntity;
        switch (atom) {
          case Atoms.Placeholder _:
            return "Placeholders should be filled";
          case Atoms.Number n:
            if (AngouriMath.Core.Number.TryParse(n.Nucleus, out var number)) {
              thisEntity = new NumberEntity(number);
              goto setEntity;
            } else return "Invalid number: " + n.Nucleus;
          case Atoms.Variable v:
            thisEntity = v.Nucleus switch
            {
              "e" => MathS.e,
              "π" => MathS.pi,
              "i" => new NumberEntity(MathS.i),
              _ when LaTeXSettings.CommandForAtom(atom) is string s => MathS.Var(s),
              var name => new VariableEntity(name)
            };
            goto setEntity;
          case Atoms.Fraction f:
            Entity numerator, denominator;
            (numerator, error) = Transform(f.Numerator).ExpectEntity(nameof(numerator));
            if (error != null) return error;
            (denominator, error) = Transform(f.Denominator).ExpectEntity(nameof(denominator));
            if (error != null) return error;
            thisEntity = numerator / denominator;
            goto setEntity;
          case Atoms.Radical r:
            Entity degree, radicand;
            (degree, error) = Transform(r.Degree).ExpectEntityOrNull(nameof(degree))
              .Bind(degree => degree is null ? 0.5 : 1 / degree);
            if (error != null) return error;
            (radicand, error) = Transform(r.Radicand).ExpectEntity(nameof(radicand));
            if (error != null) return error;
            thisEntity = MathS.Pow(radicand, degree);
            goto setEntity;
          case Atoms.Open { Nucleus: "(" }:
            i++;
            (thisEntity, error) = Transform(mathList, ref i, Precedence.Bracket);
            if (error != null) return error;
            if (thisEntity == null)
              return "Missing )";
            goto setEntity;
          case Atoms.Close { Nucleus: ")", Superscript: var super }:
            if (prevEntity == null)
              return "Missing math before )";
            switch (prec) {
              case Precedence.Lowest:
                return "Missing (";
              case Precedence.Bracket:
                if (super.IsNonEmpty()) {
                  (degree, error) = Transform(super).ExpectEntity(nameof(degree));
                  if (error != null) return error;
                  return
                    Structures.Result.Ok((MathItem?)prevEntity).ExpectEntity("base")
                    .Bind(@base => (MathItem?)MathS.Pow(@base, degree));
                }
                return prevEntity;
              default:
                i--;
                return prevEntity;
            }
          case Atoms.Inner { LeftBoundary: { Nucleus: "(" }, InnerList: var inner, RightBoundary: { Nucleus: ")" } }:
            (thisEntity, error) = Transform(inner);
            if (error != null) return error;
            if (thisEntity == null) return "Missing math between ()";
            goto setEntity;
          case Atoms.UnaryOperator { Nucleus: "+" }:
            handlePrecendence = Precedence.UnaryPlusMinus;
            handleUnary = e => +e;
            goto handleUnary;
          case Atoms.UnaryOperator { Nucleus: "\u2212" }:
            handlePrecendence = Precedence.UnaryPlusMinus;
            handleUnary = e => -e;
            goto handleUnary;
          case Atoms.LargeOperator { Nucleus: "sin" }:
            handleFunction = MathS.Sin;
            handleFunctionInverse = MathS.Arcsin;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "cos" }:
            handleFunction = MathS.Cos;
            handleFunctionInverse = MathS.Arccos;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "tan" }:
            handleFunction = MathS.Tan;
            handleFunctionInverse = MathS.Arctan;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "cot" }:
            handleFunction = MathS.Cotan;
            handleFunctionInverse = MathS.Arccotan;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "sec" }:
            handleFunction = MathS.Sec;
            handleFunctionInverse = MathS.Arcsec;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "csc" }:
            handleFunction = MathS.Cosec;
            handleFunctionInverse = MathS.Arccosec;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "arcsin" }:
            handleFunction = MathS.Arcsin;
            handleFunctionInverse = MathS.Sin;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "arccos" }:
            handleFunction = MathS.Arccos;
            handleFunctionInverse = MathS.Cos;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "arctan" }:
            handleFunction = MathS.Arctan;
            handleFunctionInverse = MathS.Tan;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "arccot" }:
            handleFunction = MathS.Arccotan;
            handleFunctionInverse = MathS.Cotan;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "arcsec" }:
            handleFunction = MathS.Arcsec;
            handleFunctionInverse = MathS.Sec;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "arccsc" }:
            handleFunction = MathS.Arccosec;
            handleFunctionInverse = MathS.Cosec;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "log", Subscript: var @base }:
            Entity? logBase;
            (logBase, error) = Transform(@base).ExpectEntityOrNull(nameof(logBase));
            if (error != null) return error;
            logBase ??= new NumberEntity(10);
            handleFunction = arg => MathS.Log(arg, logBase);
            handleFunctionInverse = arg => MathS.Pow(logBase, arg);
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "ln" }:
            handleFunction = MathS.Ln;
            handleFunctionInverse = arg => MathS.Pow(MathS.e, arg);
            goto handleFunction;
          case Atoms.BinaryOperator { Nucleus: "+" }:
            handlePrecendence = Precedence.AddSubtract;
            handleBinary = (a, b) => a + b;
            goto handleBinary;
          case Atoms.BinaryOperator { Nucleus: "\u2212" }:
            handlePrecendence = Precedence.AddSubtract;
            handleBinary = (a, b) => a - b;
            goto handleBinary;
          case Atoms.BinaryOperator { Nucleus: "*" }:
          case Atoms.BinaryOperator { Nucleus: "×" }:
          case Atoms.BinaryOperator { Nucleus: "·" }:
            handlePrecendence = Precedence.MultiplyDivide;
            handleBinary = (a, b) => a * b;
            goto handleBinary;
          case Atoms.BinaryOperator { Nucleus: "÷" }:
          case Atoms.Ordinary { Nucleus: "/" }:
            handlePrecendence = Precedence.MultiplyDivide;
            handleBinary = (a, b) => a / b;
            goto handleBinary;
          case Atoms.Ordinary { Nucleus: "%" }:
            handlePrecendence = Precedence.PercentDegree;
            handleUnary = x => x / 100;
            goto handleUnaryPostfix;
          case Atoms.Ordinary { Nucleus: "°" }:
            handlePrecendence = Precedence.PercentDegree;
            handleUnary = x => x * MathS.pi / 180;
            goto handleUnaryPostfix;
          case Atoms.Space _:
          case Atoms.Ordinary { Nucleus: var nucleus } when string.IsNullOrWhiteSpace(nucleus):
            continue;
          default:
            return $"Unsupported {atom.TypeName} {atom.Nucleus}";
            handleFunction:
            if (atom.Superscript.EqualsList(new MathList(new Atoms.UnaryOperator("\u2212"), new Atoms.Number("1")))) {
              atom.Superscript.Clear();
              handleFunction = handleFunctionInverse;
            }
            i++;
            MathList? bracketArgument = null;
            int open = -1;
            // Steal the exponent of the following argument!
            // e.g. sin(x)^2 -> sin^2(x) and sin^2(x)^3 -> sin^(2*3)(x)
            // but sin x^2 remains as-is
            for (int levelsDeep = 0; i < mathList.Count; i++)
              switch (mathList[i]) {
                case Atoms.Space _:
                case Atoms.Ordinary { Nucleus: var nucleus } when string.IsNullOrWhiteSpace(nucleus):
                  break;
                case Atoms.Inner inner:
                  var superscript = inner.Superscript;
                  bracketArgument = inner.InnerList;
                  goto stealExponent;
                case Atoms.Open _:
                  if (levelsDeep == 0) open = i;
                  levelsDeep++;
                  break;
                case Atoms.Close { HasCorrespondingOpen: true } close:
                  levelsDeep--;
                  if (levelsDeep == 0) {
                    if (open == -1) return "Missing argument for " + atom.Nucleus;
                    else bracketArgument = mathList.Slice(open + 1, i - open - 1);
                    superscript = close.Superscript;
                    goto stealExponent;
                  }
                  break;
                default:
                  if (levelsDeep == 0)
                    goto exitFor;
                  break;
                  stealExponent:
                  _ = bracketArgument; // Ensure assignment
                  if (levelsDeep > 0)
                    break;
                  if (atom.Superscript.IsNonEmpty() && superscript.IsNonEmpty()) {
                    var originalSuperscript = new Atoms.Inner(new Boundary("("), new MathList(), new Boundary(")"));
                    originalSuperscript.InnerList.Append(atom.Superscript);
                    var newSuperscript = new Atoms.Inner(new Boundary("("), new MathList(), new Boundary(")"));
                    newSuperscript.InnerList.Append(superscript);

                    atom.Superscript.Clear();
                    superscript.Clear();
                    atom.Superscript.Add(originalSuperscript);
                    atom.Superscript.Add(LaTeXSettings.Times);
                    atom.Superscript.Add(newSuperscript);
                  } else {
                    atom.Superscript.Append(superscript);
                    superscript.Clear();
                  }
                  goto exitFor;
              }
            exitFor:
            (nextEntity, error) =
             (bracketArgument == null
              ? Transform(mathList, ref i, Precedence.FunctionApplication)
              : Transform(bracketArgument))
              .ExpectEntity("argument for " + atom.Nucleus);
            if (error != null) return error;
            thisEntity = handleFunction(nextEntity);
            goto setEntity;

            handleUnary:
            i++;
            (nextEntity, error) = Transform(mathList, ref i, handlePrecendence)
              .ExpectEntity("right operand for " + atom.Nucleus);
            if (error != null) return error;
            thisEntity = handleUnary(nextEntity);
            goto setEntity;

            handleBinary:
            if (prevEntity is null) {
              // No previous entity, treat as unary operator (happens for 1---2)
              if (atom is Atoms.BinaryOperator b) {
                mathList[i] = b.ToUnaryOperator();
              } else {
                mathList[i] = new Atoms.UnaryOperator(atom.Nucleus);
                mathList[i].Superscript.Append(atom.Superscript);
                mathList[i].Subscript.Append(atom.Subscript);
              }
              i--;
              continue;
            }
            if (prec < handlePrecendence) {
              i++;
              (nextEntity, error) = Transform(mathList, ref i, handlePrecendence)
                .ExpectEntity("right operand for " + atom.Nucleus);
              if (error != null) return error;
              (thisEntity, error) =
                Structures.Result.Ok((MathItem?)prevEntity)
                .ExpectEntity("left operand for " + atom.Nucleus)
                .Bind(e => (MathItem?)handleBinary(e, nextEntity));
              if (error != null) return error;
              prevEntity = null; // We used up the previous entity, don't keep it
              goto setEntity;
            } else {
              i--;
              return prevEntity;
            }

            handleUnaryPostfix:
            if (prevEntity == null) return "Missing left operand for " + atom.Nucleus;
            if (prec < handlePrecendence) {
              (thisEntity, error) =
                Structures.Result.Ok((MathItem?)prevEntity)
                .ExpectEntity("left operand for " + atom.Nucleus)
                .Bind(e => (MathItem?)handleUnary(e));
              if (error != null) return error;
              prevEntity = null; // We used up prevEntity
              goto setEntity;
            } else {
              i--;
              return prevEntity;
            }

            setEntity:
            Entity? exponent;
            (exponent, error) = Transform(atom.Superscript).ExpectEntityOrNull(nameof(exponent));
            if (error != null) return error;
            if (exponent != null) {
              (thisEntity, error) =
                Structures.Result.Ok(thisEntity).ExpectEntity("base")
                .Bind(@base => (MathItem?)MathS.Pow(@base, exponent));
              if (error != null) return error;
            }
            Entity? prev, @this;
            (prev, error) =
              Structures.Result.Ok(prevEntity).ExpectEntityOrNull("left operand of implicit multiplication");
            if (error != null) return error;
            if (prev is null) { prevEntity = thisEntity; break; }

            (@this, error) =
              Structures.Result.Ok(thisEntity).ExpectEntity("right operand of implicit multiplication");
            if (error != null) return error;
            prevEntity = prev * @this;
            break;
        }
      }
      if (prec == Precedence.Bracket) return "Missing )";
      return prevEntity;
    }
  }
}
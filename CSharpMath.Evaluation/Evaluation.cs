using System;
using System.Collections.Generic;
using System.Linq;
using AngouriMath;
using AngouriMath.Core;

namespace CSharpMath {
  using System.Collections;
  using Atom;
  using Atoms = Atom.Atoms;
  using Structures;

  public static partial class Evaluation {
    enum Precedence {
      DefaultContext,
      BraceContext,
      BracketContext,
      ParenthesisContext,
      // Lowest
      Comma,
      SetOperation,
      AddSubtract,
      MultiplyDivide,
      FunctionApplication,
      UnaryPlusMinus,
      PercentDegree
      // Highest
    }
    public abstract class MathItem : AngouriMath.Core.Sys.Interfaces.ILatexiseable {
      private protected MathItem() { }
      public abstract string Latexise();
      public static implicit operator MathItem(AngouriMath.Entity content) => new Entity(content);
      public static explicit operator AngouriMath.Entity(MathItem item) => ((Entity)item).Content;
      public static implicit operator MathItem(SetNode content) => new Set(content);
      public static explicit operator SetNode(MathItem item) => ((Set)item).Content;
      /// <summary>A real number, complex number, variable, function call, vector, matrix or higher-dimensional tensor</summary>
      public sealed class Entity : MathItem {
        public Entity(AngouriMath.Entity content) => Content = content;
        public AngouriMath.Entity Content { get; }
        public override string Latexise() => Content.Latexise();
      }
      /// <summary>A linked list of comma-delimited items</summary>
      public sealed class Comma : MathItem, IEnumerable<MathItem> {
        public Comma(MathItem prev, MathItem? next) {
          Content = prev;
          Next = next switch { null => null, Comma c => c, _ => new Comma(next, null) };
        }
        public MathItem Content { get; }
        public Comma? Next { get; set; }
        public override string Latexise() => string.Join(",", this.Select(item => item.Latexise()));
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<MathItem> GetEnumerator() {
          Comma? current = this;
          while (current != null) {
            yield return current.Content;
            current = current.Next;
          }
        }
      }
      /// <summary>A set or a combination of set operations</summary>
      public sealed class Set : MathItem {
        public Set(SetNode content) => Content = content;
        public SetNode Content { get; }
        public override string Latexise() => Content.Latexise();
      }
    }
    public static MathList Visualize(MathItem entity) =>
      LaTeXParser.MathListFromLaTeX(entity.Latexise())
      // CSharpMath must handle all LaTeX coming from AngouriMath or a bug is present!
      .Match(list => list, e => throw new InvalidCodePathException(e));
    public static Result<MathItem> Evaluate(MathList mathList) {
      MathS.pi.ToString(); // Call into MathS's static initializer to ensure Entity methods work
      return Transform(mathList.Clone(true))
      .Bind(result =>
        result is { } r
        ? Result.Ok(r)
        : Result.Err("There is nothing to evaluate"));
    }
    static Result<MathItem?> Transform(MathList mathList) {
      int i = 0;
      return Transform(mathList, ref i, Precedence.DefaultContext);
    }
    static Result<Entity[]> ExpectEntities(this Result<MathItem?> result, string itemName) =>
      result.Bind(item => item switch {
        null => Array.Empty<Entity>(),
        MathItem.Entity { Content: var e } => new[] { e },
        MathItem.Comma c =>
          c.Aggregate((Result: Result.Ok(new Entity[c.Count()]), Index: 0), (acc, item) =>
            (acc.Result.Bind(s => item.AsEntity(itemName).Bind(i => { s[acc.Index] = i; return s; })), acc.Index + 1),
            acc => acc.Result),
        var notEntity => Result.Err(item.GetType().Name + " cannot be " + itemName)
      });
    static Result<Entity[]> AsEntities(this MathItem? item, string itemName) =>
      Result.Ok(item).ExpectEntities(itemName);
    static Result<Entity?> ExpectEntityOrNull(this Result<MathItem?> result, string itemName) =>
      result.Bind(item => item switch {
        null => Result.Ok((Entity?)null),
        MathItem.Entity entity => Result.Ok((Entity?)entity.Content),
        var notEntity => Result.Err(item.GetType().Name + " cannot be " + itemName)
      });
    static Result<Entity> ExpectEntity(this Result<MathItem?> result, string itemName) =>
      result.ExpectEntityOrNull(itemName).Bind(item => item switch {
        null => Result.Err("Missing " + itemName),
        { } entity => Result.Ok(entity)
      });
    static Result<Entity> AsEntity(this MathItem? item, string itemName) =>
      Result.Ok(item).ExpectEntity(itemName);
    static Result<SetNode?> ExpectSetOrNull(this Result<MathItem?> result, string itemName) =>
      result.Bind(item => item switch {
        null => Result.Ok((SetNode?)null),
        MathItem.Set entity => Result.Ok((SetNode?)entity.Content),
        var notEntity => Result.Err(item.GetType().Name + " cannot be " + itemName)
      });
    static Result<SetNode> ExpectSet(this Result<MathItem?> result, string itemName) =>
      result.ExpectSetOrNull(itemName).Bind(item => item switch {
        null => Result.Err("Missing " + itemName),
        { } entity => Result.Ok(entity)
      });
    static Result<SetNode> AsSet(this MathItem? item, string itemName) =>
      Result.Ok(item).ExpectSet(itemName);
    static Result<MathItem> ExpectNotNull(this Result<MathItem?> result, string itemName) =>
      result.Bind(item => item switch {
        null => Result.Err("Missing " + itemName),
        { } notnull => Result.Ok(notnull)
      });
    static Result<MathItem> TryMakeSet(MathItem.Comma c, bool leftClosed, bool rightClosed) =>
      c switch {
        { Content: var l, Next: { Content: var r, Next: null } } =>
          l.AsEntity("left interval boundary")
          .Bind(left => r.AsEntity("right interval boundary")
          .Bind(right =>
            (MathItem)(
              left == right // MathS.Sets.Interval throws when both edges are equal
              ? leftClosed && rightClosed
                ? new Set(MathS.Sets.Element(left))
                : MathS.Sets.Empty()
              : new Set(MathS.Sets.Interval(left, right).SetLeftClosed(leftClosed).SetRightClosed(rightClosed))
            )
          )),
        _ => "Unrecognized comma-delimited collection of " + c.Count() + " items"
      };
    static readonly Dictionary<Precedence, (string KnownOpening, string InferredClosing)> ContextInfo =
      new Dictionary<Precedence, (string, string)> {
        { Precedence.ParenthesisContext, ("(", ")") },
        { Precedence.BracketContext, ("[", "]") },
        { Precedence.BraceContext, ("{", "}") },
      };
    static readonly Dictionary<string, (string InferredClosing, Precedence KnownPrecedence)> OpenBracketInfo =
      new Dictionary<string, (string, Precedence)> {
        { "(", (")", Precedence.ParenthesisContext) },
        { "[", ("]", Precedence.BracketContext) },
        { "{", ("}", Precedence.BraceContext) },
      };
    static readonly Dictionary<(string? left, string? right), Func<MathItem?, Result<MathItem>>> BracketHandlers =
      new Dictionary<(string? left, string? right), Func<MathItem?, Result<MathItem>>> {
        { ("(", ")"), item => item switch {
          null => "Missing math inside ( )",
          MathItem.Comma c => TryMakeSet(c, false, false),
          _ => item
        } },
        { ("[", ")"), item => item switch {
          MathItem.Comma c => TryMakeSet(c, true, false),
          _ => "Unrecognized bracket pair [ )"
        } },
        { ("(", "]"), item => item switch {
          MathItem.Comma c => TryMakeSet(c, false, true),
          _ => "Unrecognized bracket pair ( ]"
        } },
        { ("[", "]"), item => item switch {
          null => "Missing math inside [ ]",
          MathItem.Comma c => TryMakeSet(c, true, true),
          _ => item
        } },
        { ("{", "}"), item => item.AsEntities("set element").Bind(entities => (MathItem)MathS.Sets.Finite(entities)) }
      };
    static Result<MathItem?> Transform(MathList mathList, ref int i, Precedence prec) {
      MathItem? prev = null;
      MathItem? next;
      string? error;
      Precedence handlePrecendence;
      Func<Entity, Entity> handlePrefix, handlePostfix, handleFunction, handleFunctionInverse;
      Func<Entity[], Entity> handleFunctionN, handleFunctionInverseN;
      Func<Entity, Entity, Entity> handleBinary;
      Func<SetNode, SetNode> handlePrefixSet, handlePostfixSet, handleFunctionSet, handleFunctionInverseSet;
      Func<SetNode, SetNode, SetNode> handleBinarySet;
      Func<string, MathItem?, Result<MathItem>> handlePrefixInner, handlePostfixInner, handleFunctionInner, handleFunctionInverseInner;
      Func<string, MathItem?, string, MathItem?, Result<MathItem>> handleBinaryInner;
      for (; i < mathList.Count; i++) {
        var atom = mathList[i];
        MathItem? @this;
        Result HandleSuperscript(ref MathItem? @this, MathList superscript) {
          switch(superscript) {
            case { Count: 1 } when superscript[0] is Atoms.Ordinary { Nucleus: "∁" }:
              (@this, error) =
                @this.AsSet("target of set inversion").Bind(target => (MathItem?)!target);
              if (error != null) return error;
              break;
            default:
              Entity? exponent;
              (exponent, error) = Transform(superscript).ExpectEntityOrNull(nameof(exponent));
              if (error != null) return error;
              if (exponent != null) {
                (@this, error) =
                  @this.AsEntity("base of exponentiation").Bind(@base => (MathItem?)MathS.Pow(@base, exponent));
                if (error != null) return error;
              }
              break;
          }
          return Result.Ok();
        }
        switch (atom) {
          case Atoms.Placeholder _:
            return "Placeholders should be filled";
          case Atoms.Number n:
            if (AngouriMath.Core.Numerix.ComplexNumber.TryParse(n.Nucleus, out var number)) {
              @this = new NumberEntity(number);
              goto handleThis;
            } else return "Invalid number: " + n.Nucleus;
          case Atoms.Variable v:
            var subscript = new System.Text.StringBuilder("_");
            foreach (var subAtom in v.Subscript)
              switch (subAtom) {
                case Atoms.Placeholder _:
                  return "Placeholders should be filled";
                case { Superscript: { Count: var count } } when count > 0:
                  return "Unsupported exponentiation in subscript";
                case { Subscript: { Count: var count } } when count > 0:
                  return "Unsupported subscript in subscript";
                case Atoms.Number { Nucleus: var nucleus }:
                  subscript.Append(nucleus);
                  break;
                case Atoms.Variable { Nucleus: var nucleus }:
                  subscript.Append(nucleus);
                  break;
                default:
                  return $"Unsupported {subAtom.TypeName} {subAtom.Nucleus} in subscript";
              }
            @this = (v.Nucleus, v.Subscript.Count) switch
            {
              ("R", 0) when v.FontStyle == FontStyle.Blackboard => MathS.Sets.R(),
              ("C", 0) when v.FontStyle == FontStyle.Blackboard => MathS.Sets.C(),
              ("e", 0) => MathS.e,
              ("π", 0) => MathS.pi,
              ("i", 0) => new NumberEntity(MathS.i),
              // Convert θ to theta
              _ when LaTeXSettings.CommandForAtom(atom) is string s => MathS.Var(s + subscript.ToString()),
              (var name, _) => MathS.Var(name + subscript.ToString())
            };
            v.Subscript.Clear();
            goto handleThis;
          case Atoms.Ordinary { Nucleus: "∞" }:
            @this = new NumberEntity(AngouriMath.Core.Numerix.RealNumber.PositiveInfinity);
            goto handleThis;
          case Atoms.Ordinary { Nucleus: "∅" }:
            @this = MathS.Sets.Empty();
            goto handleThis;
          case Atoms.Fraction f:
            Entity numerator, denominator;
            (numerator, error) = Transform(f.Numerator).ExpectEntity(nameof(numerator));
            if (error != null) return error;
            (denominator, error) = Transform(f.Denominator).ExpectEntity(nameof(denominator));
            if (error != null) return error;
            @this = numerator / denominator;
            goto handleThis;
          case Atoms.Radical r:
            Entity degree, radicand;
            (degree, error) = Transform(r.Degree).ExpectEntityOrNull(nameof(degree))
              .Bind(degree => degree is null ? 0.5 : 1 / degree);
            if (error != null) return error;
            (radicand, error) = Transform(r.Radicand).ExpectEntity(nameof(radicand));
            if (error != null) return error;
            @this = MathS.Pow(radicand, degree);
            goto handleThis;
          case Atoms.UnaryOperator { Nucleus: "+" }:
            handlePrecendence = Precedence.UnaryPlusMinus;
            handlePrefix = e => +e;
            goto handlePrefix;
          case Atoms.UnaryOperator { Nucleus: "\u2212" }:
            handlePrecendence = Precedence.UnaryPlusMinus;
            handlePrefix = e => -e;
            goto handlePrefix;
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
            @base.Clear();
            logBase ??= new NumberEntity(10);
            handleFunction = arg => MathS.Log(logBase, arg);
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
            handlePostfix = x => x / 100;
            goto handlePostfix;
          case Atoms.Ordinary { Nucleus: "°" }:
            handlePrecendence = Precedence.PercentDegree;
            handlePostfix = x => x * MathS.pi / 180;
            goto handlePostfix;
          case Atoms.Punctuation { Nucleus: "," }:
            if (prec <= Precedence.Comma) {
              if (prev is null) return "Missing left operand for comma";
              i++;
              (next, error) = Transform(mathList, ref i, Precedence.Comma);
              if (error != null) return error;
              if (next is null) return "Missing right operand for comma";
              @this = new MathItem.Comma(prev, next);
              prev = null;
              goto handleThis;
            } else {
              i--;
              return prev;
            }
          case Atoms.BinaryOperator { Nucleus: "∩" }:
            handlePrecendence = Precedence.SetOperation;
            handleBinarySet = (l, r) => l & r;
            goto handleBinarySet;
          case Atoms.BinaryOperator { Nucleus: "∪" }:
            handlePrecendence = Precedence.SetOperation;
            handleBinarySet = (l, r) => l | r;
            goto handleBinarySet;
          case Atoms.BinaryOperator { Nucleus: "∖" }:
            handlePrecendence = Precedence.SetOperation;
            handleBinarySet = (l, r) => l - r;
            goto handleBinarySet;
          case Atoms.Table { Environment: "matrix" } matrix:
            var (rows, cols, cells) = (matrix.NRows, matrix.NColumns, matrix.Cells);
            var matrixElements = new Entity[rows * cols];
            for (var row = 0; row < rows; row++)
              for (var col = 0; col < cols; col++) {
                if (cells[row].Count <= col)
                  return $"There are empty slots in the {rows}×{cols} matrix";
                (matrixElements[row * cols + col], error) = Transform(cells[row][col]).ExpectEntity("matrix element");
                if (error != null) return error;
              }
            @this = MathS.Matrices.Matrix(rows, cols, matrixElements);
            goto handleThis;
          case Atoms.Open { Nucleus: var opening }:
            if (!OpenBracketInfo.TryGetValue(opening, out var bracketInfo))
              return "Unsupported opening bracket " + opening;
            i++;
            (@this, error) = Transform(mathList, ref i, bracketInfo.KnownPrecedence);
            if (error != null) return error;
            if (@this == null) return "Missing " + bracketInfo.InferredClosing;
            goto handleThis;
          case Atoms.Close { Nucleus: var rightBracket, Superscript: var super, Subscript: var sub }:
            if (sub.Count > 0) return "Subscripts are unsupported for Close " + rightBracket;
            if (!ContextInfo.TryGetValue(prec, out var contextInfo))
              switch (prec) {
                case Precedence.DefaultContext:
                  string leftBracket;
                  switch (rightBracket) {
                    case ")":
                      leftBracket = "(";
                      break;
                    case "]":
                      leftBracket = "[";
                      break;
                    case "}":
                      leftBracket = "{";
                      break;
                    default:
                      return "Unsupported closing bracket " + rightBracket;
                  }
                  return "Missing " + leftBracket;
                default:
                  i--;
                  return prev;
              }
            return
              BracketHandlers.TryGetValue((contextInfo.KnownOpening, rightBracket), out var handler)
              ? handler(prev).Bind(handled => {
                MathItem? nullable = handled;
                if (HandleSuperscript(ref nullable, super).Error is { } error)
                  return Result.Err(error);
                return Result.Ok(nullable);
              })
              : $"Unrecognized bracket pair {contextInfo.KnownOpening} {rightBracket}";
          case Atoms.Inner { LeftBoundary: { Nucleus: var left }, InnerList: var inner, RightBoundary: { Nucleus: var right } }:
            (@this, error) = Transform(inner);
            if (error != null) return error;
            (@this, error) =
              BracketHandlers.TryGetValue((left, right), out handler)
              ? handler(@this)
              : $"Unrecognized bracket pair {left ?? "(empty)"} {right ?? "(empty)"}";
            if (error != null) return error;
            goto handleThis;
          case Atoms.Space _:
          case Atoms.Style _:
          case Atoms.Comment _:
          case Atoms.Ordinary { Nucleus: var nucleus } when string.IsNullOrWhiteSpace(nucleus):
            if (atom.Superscript.Count > 0)
              return $"Exponentiation is unsupported for {atom.TypeName}";
            if (atom.Subscript.Count > 0)
              return $"Subscripts are unsupported for {atom.TypeName}";
            continue;
          case Atoms.Table table:
            return $"Unsupported table environment {table.Environment}";
          default:
            return $"Unsupported {atom.TypeName} {atom.Nucleus}";
#pragma warning disable CS0162 // Unreachable code detected
#pragma warning disable CS0164 // This label has not been referenced
            handleFunction:
            handleFunctionInner = (itemName, item) =>
              item.AsEntity(itemName).Bind(e => (MathItem)handleFunction(e));
            handleFunctionInverseInner = (itemName, item) =>
              item.AsEntity(itemName).Bind(e => (MathItem)handleFunctionInverse(e));
            goto handleFunctionInner;
            handleFunctionN:
            handleFunctionInner = (itemName, item) =>
              item.AsEntities(itemName).Bind(e => (MathItem)handleFunctionN(e));
            handleFunctionInverseInner = (itemName, item) =>
              item.AsEntities(itemName).Bind(e => (MathItem)handleFunctionInverseN(e));
            goto handleFunctionInner;
            handleFunctionSet:
            handleFunctionInner = (itemName, item) =>
              item.AsSet(itemName).Bind(set => (MathItem)handleFunctionSet(set));
            handleFunctionInverseInner = (itemName, item) =>
              item.AsSet(itemName).Bind(set => (MathItem)handleFunctionInverseSet(set));
            goto handleFunctionInner;
            handleFunctionInner:
            if (atom.Superscript.EqualsList(new MathList(new Atoms.UnaryOperator("\u2212"), new Atoms.Number("1")))) {
              atom.Superscript.Clear();
              handleFunctionInner = handleFunctionInverseInner;
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
                case Atoms.Close close:
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
            (next, error) =
              bracketArgument == null
              ? Transform(mathList, ref i, Precedence.FunctionApplication)
              : Transform(bracketArgument);
            if (error != null) return error;
            (@this, error) = handleFunctionInner("argument for " + atom.Nucleus, next);
            if (error != null) return error;
            goto handleThis;

            handlePrefix:
            handlePrefixInner = (itemName, item) => item.AsEntity(itemName).Bind(e => (MathItem)handlePrefix(e));
            goto handlePrefixInner;
            handlePrefixSet:
            handlePrefixInner = (itemName, item) => item.AsSet(itemName).Bind(set => (MathItem)handlePrefixSet(set));
            goto handlePrefixInner;
            handlePrefixInner:
            i++;
            (next, error) = Transform(mathList, ref i, handlePrecendence);
            if (error != null) return error;
            (@this, error) = handlePrefixInner("right operand for " + atom.Nucleus, next);
            if (error != null) return error;
            goto handleThis;

            handleBinary:
            handleBinaryInner = (leftName, left, rightName, right) => {
              Entity l, r;
              (l, error) = left.AsEntity(leftName);
              if (error != null) return error;
              (r, error) = right.AsEntity(rightName);
              if (error != null) return error;
              return (MathItem)handleBinary(l, r);
            };
            goto handleBinaryInner;
            handleBinarySet:
            handleBinaryInner = (leftName, left, rightName, right) => {
              SetNode l, r;
              (l, error) = left.AsSet(leftName);
              if (error != null) return error;
              (r, error) = right.AsSet(rightName);
              if (error != null) return error;
              return (MathItem)handleBinarySet(l, r);
            };
            goto handleBinaryInner;
            handleBinaryInner:
            if (prev is null) {
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
              (next, error) = Transform(mathList, ref i, handlePrecendence);
              if (error != null) return error;
              (@this, error) =
                handleBinaryInner("left operand for " + atom.Nucleus, prev,
                  "right operand for " + atom.Nucleus, next);
              if (error != null) return error;
              prev = null; // We used up prev, don't keep it
              goto handleThis;
            } else {
              i--;
              return prev;
            }

            handlePostfix:
            handlePostfixInner = (itemName, item) => item.AsEntity(itemName).Bind(e => (MathItem)handlePostfix(e));
            goto handlePostfixInner;
            handlePostfixSet:
            handlePostfixInner = (itemName, item) => item.AsSet(itemName).Bind(set => (MathItem)handlePostfixSet(set));
            goto handlePostfixInner;
            handlePostfixInner:
            if (prev == null) return "Missing left operand for " + atom.Nucleus;
            if (prec < handlePrecendence) {
              (@this, error) =
                handlePostfixInner("left operand for " + atom.Nucleus, prev);
              if (error != null) return error;
              prev = null; // We used up prev, don't keep it
              goto handleThis;
            } else {
              i--;
              return prev;
            }
#pragma warning restore CS0162 // Unreachable code detected
#pragma warning restore CS0164 // This label has not been referenced

            handleThis:
            if (atom.Subscript.Count > 0)
              return $"Subscripts are unsupported for {atom.TypeName} {atom.Nucleus}";
            error = HandleSuperscript(ref @this, atom.Superscript).Error;
            if (error != null) return error;
            Entity? prevEntity, thisEntity;
            (prevEntity, error) =
              Result.Ok(prev).ExpectEntityOrNull("left operand of implicit multiplication");
            if (error != null) return error;
            if (prevEntity is null) { prev = @this; break; }
            (thisEntity, error) =
              Result.Ok(@this).ExpectEntity("right operand of implicit multiplication");
            if (error != null) return error;
            prev = prevEntity * thisEntity;
            break;
        }
      }
      if (ContextInfo.TryGetValue(prec, out var info))
        return "Missing " + info.InferredClosing;
      return prev;
    }
  }
}
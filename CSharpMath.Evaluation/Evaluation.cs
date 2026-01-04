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
  using System.Numerics;

  public static partial class Evaluation {
    enum Precedence {
      DefaultContext,
      BraceContext,
      BracketContext,
      ParenthesisContext,
      // Lowest
      Comma,
      Equivalence,
      Implication,
      Disjunction,
      Conjunction,
      Negation,
      Relation,
      SetOperation,
      AddSubtract,
      MultiplyDivide,
      FunctionApplication,
      UnaryPlusMinus,
      Postfix
      // Highest
    }
    public abstract class MathItem : AngouriMath.Core.ILatexiseable {
      private protected MathItem() { }
      public abstract string Latexise();
      public static implicit operator MathItem(AngouriMath.Entity content) => new Entity(content);
      public static explicit operator AngouriMath.Entity(MathItem item) => ((Entity)item).Content;
      /// <summary>A real number, complex number, variable, function call, vector, matrix, higher-dimensional tensor, or set</summary>
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
    }
    public static MathList Visualize(MathItem entity) =>
      LaTeXParser.MathListFromLaTeX(entity.Latexise())
      // CSharpMath must handle all LaTeX coming from AngouriMath or a bug is present!
      .Match(list => list, e => throw new InvalidCodePathException(e));
    public static Result<MathItem> Evaluate(MathList mathList) {
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
                ? MathS.Sets.Finite(left)
                : MathS.Sets.Empty
              : MathS.Sets.Interval(left, leftClosed, right, rightClosed))
            )
          ),
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
      Func<Entity, Entity, Entity> handleBinary;
      for (; i < mathList.Count; i++) {
        var atom = mathList[i];
        MathItem? @this;
        bool subscriptAllowed = false;
        Result HandleSuperscript(ref MathItem? @this, MathList superscript) {
          switch(superscript) {
            case { Count: 1 } when superscript[0] is Atoms.Ordinary { Nucleus: "∁" }:
              (@this, error) =
                @this.AsEntity("target of set inversion").Bind(target => (MathItem?)MathS.SetSubtraction(MathS.Sets.C, target)); // we don't support domains yet
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
          case Atoms.Number { Subscript: [Atoms.Number numericBase] } n:
            if (int.TryParse(numericBase.Nucleus, out var @base)) {
              try { @this = MathS.FromBaseN(atom.Nucleus, @base); } catch (Exception e) { return e.Message; }
              subscriptAllowed = true;
              goto handleThis;
            } else return "Invalid numeric base: " + numericBase.Nucleus;
          case Atoms.Number n:
            if (Entity.Number.Complex.TryParse(n.Nucleus, out var number)) {
              @this = number;
              goto handleThis;
            } else return "Invalid number: " + n.Nucleus;
          case Atoms.Variable v:
            var name = new System.Text.StringBuilder(v.Nucleus);
            if (v.FontStyle is FontStyle.Roman) // handle multi-character roman variables
              while (i + 1 < mathList.Count) {
                if (mathList[i + 1] is Atoms.Variable { FontStyle: FontStyle.Roman } v2) {
                  name.Append(v2.Nucleus);
                  v = v2;
                  i++;
                  if (v2.Superscript.Count > 0 || v2.Subscript.Count > 0) break;
                } else break;
              }
            var subscript = new System.Text.StringBuilder();
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
            var underscore = subscript.Length > 0 ? "_" : "";
            @this = (name.ToString(), v.Subscript.Count, v.FontStyle) switch
            {
              ("C", 0, FontStyle.Blackboard) => MathS.Sets.C,
              ("R", 0, FontStyle.Blackboard) => MathS.Sets.R,
              ("Q", 0, FontStyle.Blackboard) => MathS.Sets.Q,
              ("Z", 0, FontStyle.Blackboard) => MathS.Sets.Z,
              ("e", 0, FontStyle.Roman or FontStyle.Default or FontStyle.Italic) => MathS.e,
              ("π", 0, FontStyle.Roman or FontStyle.Default or FontStyle.Italic) => MathS.pi,
              ("i", 0, FontStyle.Roman or FontStyle.Default or FontStyle.Italic) => MathS.i,
              // Convert θ to theta
              (_, _, FontStyle.Default or FontStyle.Italic) when LaTeXSettings.CommandForAtom(atom) is string s => MathS.Var(string.Concat(s.TrimStart('\\'), underscore, subscript)),
              _ => MathS.Var(name + underscore + subscript.ToString())
            };
            subscriptAllowed = true;
            goto handleThis;
          case Atoms.Ordinary { Nucleus: "∞" }:
            @this = Entity.Number.Real.PositiveInfinity;
            goto handleThis;
          case Atoms.Ordinary { Nucleus: "∅" }:
            @this = MathS.Sets.Empty;
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
          case Atoms.LargeOperator { Nucleus: "log", Subscript: var logBaseList }:
            Entity? logBase;
            (logBase, error) = Transform(logBaseList).ExpectEntityOrNull(nameof(logBase));
            if (error != null) return error;
            logBase ??= 10;
            handleFunction = arg => MathS.Log(logBase, arg);
            handleFunctionInverse = arg => MathS.Pow(logBase, arg);
            subscriptAllowed = true;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "ln" }:
            handleFunction = MathS.Ln;
            handleFunctionInverse = arg => MathS.Pow(MathS.e, arg);
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "sgn" }:
            handleFunction = MathS.Signum;
            handleFunctionInverse = arg => MathS.NaN;
            goto handleFunction;
          case Atoms.BinaryOperator { Nucleus: "+" }:
            handlePrecendence = Precedence.AddSubtract;
            handleBinary = (a, b) => a + b;
            goto handleBinary;
          case Atoms.BinaryOperator { Nucleus: "\u2212" }:
            handlePrecendence = Precedence.AddSubtract;
            handleBinary = (a, b) => a - b;
            goto handleBinary;
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
            handlePostfix = x => x / 100;
            goto handlePostfix;
          case Atoms.Ordinary { Nucleus: "°" }:
            handlePostfix = x => x * MathS.pi / 180;
            goto handlePostfix;
          case Atoms.Punctuation { Nucleus: "!" }:
            if (i + 1 < mathList.Count && mathList[i + 1] is Atoms.Punctuation { Nucleus: "!" }) {
              i++;
              // z!! = 2^(z/2) (2/π)^((1-cos(πz))/4) Γ(z/2+1)
              handlePostfix = z => MathS.Pow(2, z / 2) *
                MathS.Pow(2 / MathS.pi, (1 - MathS.Cos(MathS.pi * z)) / 4) *
                MathS.Factorial(z / 2);
            } else
              handlePostfix = MathS.Factorial;
            goto handlePostfix;
          case Atoms.Punctuation { Nucleus: "," }:
          case Atoms.Punctuation { Nucleus: ";" }: // ; is interpreted as an alias of ,
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
            handleBinary = MathS.Intersection;
            goto handleBinary;
          case Atoms.BinaryOperator { Nucleus: "∪" }:
            handlePrecendence = Precedence.SetOperation;
            handleBinary = MathS.Union;
            goto handleBinary;
          case Atoms.BinaryOperator { Nucleus: "∖" }:
            handlePrecendence = Precedence.SetOperation;
            handleBinary = MathS.SetSubtraction;
            goto handleBinary;
          case Atoms.Ordinary { Nucleus: "⊤" }:
            @this = MathS.Boolean.Create(true);
            goto handleThis;
          case Atoms.Ordinary { Nucleus: "⊥" }:
            @this = MathS.Boolean.Create(false);
            goto handleThis;
          case Atoms.Ordinary { Nucleus: "¬" }:
            handlePrecendence = Precedence.Negation;
            handlePrefix = MathS.Negation;
            goto handlePrefix;
          case Atoms.BinaryOperator { Nucleus: "∧" }:
            handlePrecendence = Precedence.Conjunction;
            handleBinary = MathS.Conjunction;
            goto handleBinary;
          case Atoms.BinaryOperator { Nucleus: "⌅" }:
            handlePrecendence = Precedence.Conjunction;
            handleBinary = (x, y) => MathS.Negation(MathS.Conjunction(x, y));
            goto handleBinary;
          case Atoms.BinaryOperator { Nucleus: "∨" }:
            handlePrecendence = Precedence.Disjunction;
            handleBinary = MathS.Disjunction;
            goto handleBinary;
          case Atoms.BinaryOperator { Nucleus: "⊻" }:
          case Atoms.Relation { Nucleus: "↮" }:
            handlePrecendence = Precedence.Disjunction; // XOR has same precedence as OR
            handleBinary = MathS.ExclusiveDisjunction;
            goto handleBinary;
          case Atoms.Relation { Nucleus: "↔" }:
            handlePrecendence = Precedence.Equivalence;
            handleBinary = (x, y) => MathS.Negation(MathS.ExclusiveDisjunction(x, y)); // XNOR = equivalence
            goto handleBinary;
          case Atoms.Relation { Nucleus: "→" }:
            handlePrecendence = Precedence.Implication;
            handleBinary = MathS.Implication;
            goto handleBinary;
          case Atoms.Relation { Nucleus: "↛" }:
            handlePrecendence = Precedence.Implication;
            handleBinary = (x, y) => MathS.Negation(MathS.Implication(x, y));
            goto handleBinary;
          case Atoms.Relation { Nucleus: "←" }:
            handlePrecendence = Precedence.Implication;
            handleBinary = (x, y) => MathS.Implication(y, x);
            goto handleBinary;
          case Atoms.Relation { Nucleus: "↚" }:
            handlePrecendence = Precedence.Implication;
            handleBinary = (x, y) => MathS.Negation(MathS.Implication(y, x));
            goto handleBinary;
          case Atoms.Relation { Nucleus: "∈" }:
            handlePrecendence = Precedence.Relation;
            handleBinary = MathS.Sets.ElementInSet;
            goto handleBinary;
          case Atoms.Relation { Nucleus: "∉" }:
            handlePrecendence = Precedence.Relation;
            handleBinary = (element, set) => MathS.Negation(MathS.Sets.ElementInSet(element, set));
            goto handleBinary;
          case Atoms.Relation { Nucleus: "∋" }:
            handlePrecendence = Precedence.Relation;
            handleBinary = (set, element) => MathS.Sets.ElementInSet(element, set);
            goto handleBinary;
          case Atoms.Relation { Nucleus: "=" }:
            handlePrecendence = Precedence.Relation;
            handleBinary = MathS.Equality;
            goto handleBinary;
          case Atoms.Relation { Nucleus: "≠" }:
            handlePrecendence = Precedence.Relation;
            handleBinary = (element, set) => MathS.Negation(MathS.Equality(element, set));
            goto handleBinary;
          case Atoms.Relation { Nucleus: "<" }:
            handlePrecendence = Precedence.Relation;
            handleBinary = MathS.LessThan;
            goto handleBinary;
          case Atoms.Relation { Nucleus: "≤" or "⩽" }:
            handlePrecendence = Precedence.Relation;
            handleBinary = MathS.LessOrEqualThan;
            goto handleBinary;
          case Atoms.Relation { Nucleus: ">" }:
            handlePrecendence = Precedence.Relation;
            handleBinary = MathS.GreaterThan;
            goto handleBinary;
          case Atoms.Relation { Nucleus: "≥" or "⩾" }:
            handlePrecendence = Precedence.Relation;
            handleBinary = MathS.GreaterOrEqualThan;
            goto handleBinary;
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
            (@this, error) = next.AsEntity("argument for " + atom.Nucleus).Bind(e => (MathItem)handleFunction(e));
            if (error != null) return error;
            goto handleThis;

            handlePrefix:
            i++;
            (next, error) = Transform(mathList, ref i, handlePrecendence);
            if (error != null) return error;
            (@this, error) = next.AsEntity("right operand for " + atom.Nucleus).Bind(e => (MathItem)handlePrefix(e));
            if (error != null) return error;
            goto handleThis;

            handleBinary:
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
              (var l, error) = prev.AsEntity("left operand for " + atom.Nucleus);
              if (error != null) return error;
              (var r, error) = next.AsEntity("right operand for " + atom.Nucleus);
              if (error != null) return error;
              @this = (MathItem)handleBinary(l, r);
              if (error != null) return error;
              prev = null; // We used up prev, don't keep it
              goto handleThis;
            } else {
              i--;
              return prev;
            }

            handlePostfix:
            (@this, error) =
              prev.AsEntity("left operand for " + atom.Nucleus).Bind(e => (MathItem)handlePostfix(e));
            if (error != null) return error;
            prev = null; // We used up prev, don't keep it
            goto handleThis;

            handleThis:
            if (!subscriptAllowed && atom.Subscript.Count > 0)
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
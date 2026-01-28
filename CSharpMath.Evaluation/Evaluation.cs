using System;
using System.Collections.Generic;
using System.Linq;
using AngouriMath;
using AngouriMath.Core;

namespace CSharpMath {
  using System.Collections;
  using System.Data.SqlTypes;
  using System.Numerics;
  using Atom;
  using Atoms = Atom.Atoms;

  public static partial class Evaluation {
    enum Precedence {
      DefaultContext,
      CasePredicateContext,
      LimitSubscriptContext,
      IntegralBodyContext,
      BraceContext,
      BracketContext,
      ParenthesisContext,
      _, // used during recursive right associative processing of comma
      // Lowest
      Comma, // right associative
      Provided, // right associative
      Equivalence,
      Implication, // right associative
      Disjunction,
      ExclusiveDisjunction,
      Conjunction,
      Negation,
      Comparison,
      SetMembership,
      SetUnionSubtraction,
      SetIntersection,
      AdditionSubtraction,
      CalculusOperation,
      MultiplicationDivision,
      FunctionApplication,
      UnaryPlusMinus,
      Postfix
      // Highest
    }
    public abstract record MathItem : ILatexiseable {
      private protected MathItem() { }
      public abstract string Latexise();
      public static implicit operator MathItem(AngouriMath.Entity content) => new Entity(content);
      public static explicit operator AngouriMath.Entity(MathItem item) => ((Entity)item).Content;
      /// <summary>A real number, complex number, variable, function call, vector, matrix, higher-dimensional tensor, or set</summary>
      public sealed record Entity : MathItem {
        public Entity(AngouriMath.Entity content) => Content = content;
        public AngouriMath.Entity Content { get; }
        public override string Latexise() => Content.Latexise();
      }
      /// <summary>A linked list of comma-delimited items</summary>
      public sealed record Comma : MathItem, IEnumerable<MathItem> {
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
    public static Result<MathItem> Evaluate(MathList mathList) =>
      Transform(mathList.Clone(true)).Bind(result => result is { } r ? Result.Ok(r) : Result.Err("There is nothing to evaluate"));
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
      new() {
        { Precedence.ParenthesisContext, ("(", ")") },
        { Precedence.BracketContext, ("[", "]") },
        { Precedence.BraceContext, ("{", "}") },
      };
    static readonly Dictionary<string, (string InferredClosing, Precedence KnownPrecedence)> OpenBracketInfo =
      new() {
        { "(", (")", Precedence.ParenthesisContext) },
        { "[", ("]", Precedence.BracketContext) },
        { "{", ("}", Precedence.BraceContext) },
      };
    static readonly Dictionary<(string? left, string? right), Func<MathItem?, Result<MathItem>>> InnerHandlers =
      new() {
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
        { ("{", "}"), item => item.AsEntities("set element").Bind(entities => (MathItem)MathS.Sets.Finite(entities)) },
        { ("|", "|"), item => item.AsEntity("abs argument").Bind(x => (MathItem)MathS.Abs(x)) }
      };
    static Result<MathItem?> Transform(MathList mathList, ref int i, Precedence prec) {
      MathItem? prev = null;
      MathItem? next;
      string? error;
      Precedence handlePrecedence;
      Func<Entity, Entity> handlePrefix, handlePostfix, handleFunction, handleFunctionInverse;
      Func<Entity, Entity, Entity> handleBinary;
      Entity? chainedComparisonTarget = null;
      for (; i < mathList.Count; i++) {
        var atom = mathList[i];
        MathItem? @this;
        bool subscriptAllowed = false, binaryIsRightAssociative = false;
        Result HandleSuperscript(ref MathItem? @this, ref int i, MathList superscript) {
          switch (superscript) {
            case [Atoms.Ordinary { Nucleus: "∁" }]:
              (@this, error) =
                @this.AsEntity("target of set inversion").Bind(target => (MathItem?)MathS.SetSubtraction(MathS.Sets.C, target)); // we don't support domains yet
              if (error != null) return error;
              break;
            case [Atoms.UnaryOperator { Nucleus: ("+" or "\u2212") and var direction }]:
              if (prec != Precedence.LimitSubscriptContext) return $"{direction} alone in superscript but not in limit subscript context";
              if (i != mathList.Count - 1) return $"Limit direction indicator {direction} not placed at the end";
              if (direction == "+") i = mathList.Count + 2; // signal approach from right
              else i = mathList.Count + 1; // signal approach from left
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
          case Atoms.Variable:
            var nameBuilder = new System.Text.StringBuilder(atom.Nucleus);
            if (atom is { FontStyle: FontStyle.Roman, Superscript: [], Subscript: [] }) // handle multi-character roman variables
              while (i + 1 < mathList.Count) {
                if (mathList[i + 1] is Atoms.Variable { FontStyle: FontStyle.Roman } v) {
                  nameBuilder.Append(v.Nucleus);
                  atom = v;
                  i++;
                  if (v.Superscript.Count > 0 || v.Subscript.Count > 0) break;
                } else break;
              }
            var subscript = new System.Text.StringBuilder();
            foreach (var subAtom in atom.Subscript)
              switch (subAtom) {
                case Atoms.Placeholder _:
                  return "Placeholders should be filled";
                case { Superscript.Count: > 0 }:
                  return "Unsupported exponentiation in subscript";
                case { Subscript.Count: > 0 }:
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
            // Convert θ to theta
            string GreekToLaTeXCommandName(string n) =>
              // Only latin and greek letters have the Variable atom
              LaTeXSettings.CommandForAtom(new Atoms.Variable(n))?.TrimStart('\\') ?? n;
            switch (GreekToLaTeXCommandName(nameBuilder.ToString()), atom.Subscript.Count, atom.FontStyle) {
              case ("C", 0, FontStyle.Blackboard): @this = MathS.Sets.C; break;
              case ("R", 0, FontStyle.Blackboard): @this = MathS.Sets.R; break;
              case ("Q", 0, FontStyle.Blackboard): @this = MathS.Sets.Q; break;
              case ("Z", 0, FontStyle.Blackboard): @this = MathS.Sets.Z; break;
              case ("e", 0, FontStyle.Roman or FontStyle.Default or FontStyle.Italic): @this = MathS.e; break;
              case ("pi", 0, FontStyle.Roman or FontStyle.Default or FontStyle.Italic): @this = MathS.pi; break;
              case ("i", 0, FontStyle.Roman or FontStyle.Default or FontStyle.Italic): @this = MathS.i; break;
              case ("undefined", 0, FontStyle.Roman): @this = MathS.NaN; break;
              case ("d", 0, FontStyle.Roman):
                if (prec >= Precedence.AdditionSubtraction) { i--; return prev; } // re-parse outside as this may be closing a nested integral
                if (prec != Precedence.IntegralBodyContext) return "d alone but not in integral body context";
                return prev;
              case ("for", _, FontStyle.Roman):
                if (atom.Superscript is not [])
                  return "for operator cannot have a superscript";
                else if (atom.Subscript is not [])
                  return "for operator cannot have a subscript";
                if (prec == Precedence.CasePredicateContext && prev is null) continue;
                atom.Nucleus = "for"; // for the error message
                handlePrecedence = Precedence.Provided;
                handleBinary = MathS.Provided;
                binaryIsRightAssociative = true;
                goto handleBinary;
              case (var name, _, _): @this = MathS.Var(name + underscore + GreekToLaTeXCommandName(subscript.ToString())); break;
            }
            subscriptAllowed = true;
            goto handleThis;
          case Atoms.Ordinary { Nucleus: "∞" }:
            @this = Entity.Number.Real.PositiveInfinity;
            goto handleThis;
          case Atoms.Ordinary { Nucleus: "∅" }:
            @this = MathS.Sets.Empty;
            goto handleThis;
          case Atoms.Fraction f:
            Entity? numerator, denominator;
            // Check for derivative notation: (d^n y)/(d x^n) or (d y)/(d x) where the d is not part of a longer variable name
            if (f.Numerator is ([Atoms.Variable { FontStyle: FontStyle.Roman, Nucleus: "d", Superscript: var numSuper }, ..] and not [_, Atoms.Variable { FontStyle: FontStyle.Roman }, ..]) &&
                f.Denominator is [Atoms.Variable { FontStyle: FontStyle.Roman, Nucleus: "d", Superscript: var denomSuper }, ..] and not [_, Atoms.Variable { FontStyle: FontStyle.Roman }, ..]) {

              // Parse derivative order from numerator's d exponent
              int order;
              switch (numSuper) {
                case []:
                  order = 1;
                  break;
                case [Atoms.Number { Nucleus: var n }]:
                  if (int.TryParse(n, out order)) break;
                  else return $"Derivative order must be an integer, got {n}";
                case [Atoms.UnaryOperator { Nucleus: "\u2212" }, Atoms.Number { Nucleus: var n }]:
                  if (int.TryParse(n, out order)) { order = -order; break; } else return $"Derivative order must be an integer, got {n}";
                default:
                  return "Derivative order must be an integer";
              }
              if (denomSuper.Count > 0)
                return "The d in derivative denominator cannot have an exponent. Did you mean to write it at the end of the denominator?";

              // For higher-order derivatives, check that the variable has the matching exponent
              if (order != 1 && f.Denominator.Count > 1) {
                switch (f.Denominator.Last?.Superscript) {
                  case []:
                    // No exponent on denominator but order > 1, e.g. d²y/dx
                    if (order != 1) return $"Derivative order mismatch: {order} in numerator requires {order} in denominator";
                    break;
                  case [Atoms.Number { Nucleus: var n }]:
                    if (int.TryParse(n, out var denomOrder))
                      if (order == denomOrder) break;
                      // Require both numerator and denominator in d²y/dx² to have exponent 2
                      else return $"Derivative order mismatch: {order} in numerator but {denomOrder} is in denominator";
                    else return $"Derivative order must be an integer, got {n}";
                  case [Atoms.UnaryOperator { Nucleus: "\u2212" }, Atoms.Number { Nucleus: var n }]:
                    if (int.TryParse(n, out denomOrder))
                      if (order == -denomOrder) break;
                      // Require both numerator and denominator in d²y/dx² to have exponent 2
                      else return $"Derivative order mismatch: {order} in numerator but -{denomOrder} is in denominator";
                    else return $"Derivative order must be an integer, got -{n}";
                  default:
                    return "Derivative order must be an integer";
                }
                f.Denominator.Last?.Superscript.Clear();
              }

              var numeratorIndex = 1;
              (numerator, error) = Transform(f.Numerator, ref numeratorIndex, Precedence.DefaultContext).ExpectEntityOrNull("derivative body");
              if (error != null) return error;

              var denominatorIndex = 1;
              (denominator, error) = Transform(f.Denominator, ref denominatorIndex, Precedence.DefaultContext).ExpectEntity("derivative variable");
              if (error != null) return error;

              if (numerator is null) {
                // Derivative operator (no body yet)
                handlePrecedence = Precedence.CalculusOperation;
                handlePrefix = derivativeBody => MathS.Derivative(derivativeBody, denominator, order);
                atom.Nucleus = "derivative operator"; // for the error message
                goto handlePrefix;
              }

              @this = MathS.Derivative(numerator, denominator, order);
              goto handleThis;
            }
            (numerator, error) = Transform(f.Numerator).ExpectEntity(nameof(numerator));
            if (error != null) return error;
            (denominator, error) = Transform(f.Denominator).ExpectEntity(nameof(denominator));
            if (error != null) return error;
            @this = numerator / denominator;
            goto handleThis;
          case Atoms.Radical r:
            Entity degree, radicand;
            (degree, error) = Transform(r.Degree).ExpectEntityOrNull(nameof(degree))
              .Bind(degree => degree is null ? Entity.Number.Rational.Create(1, 2) : 1 / degree);
            if (error != null) return error;
            (radicand, error) = Transform(r.Radicand).ExpectEntity(nameof(radicand));
            if (error != null) return error;
            @this = MathS.Pow(radicand, degree);
            goto handleThis;
          case Atoms.UnaryOperator { Nucleus: "+" }:
            handlePrecedence = Precedence.UnaryPlusMinus;
            handlePrefix = e => +e;
            goto handlePrefix;
          case Atoms.UnaryOperator { Nucleus: "\u2212" }:
            handlePrecedence = Precedence.UnaryPlusMinus;
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
          case Atoms.LargeOperator { Nucleus: "sinh" }:
            handleFunction = MathS.Hyperbolic.Sinh;
            handleFunctionInverse = MathS.Hyperbolic.Arsinh;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "cosh" }:
            handleFunction = MathS.Hyperbolic.Cosh;
            handleFunctionInverse = MathS.Hyperbolic.Arcosh;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "tanh" }:
            handleFunction = MathS.Hyperbolic.Tanh;
            handleFunctionInverse = MathS.Hyperbolic.Artanh;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "coth" }:
            handleFunction = MathS.Hyperbolic.Cotanh;
            handleFunctionInverse = MathS.Hyperbolic.Arcotanh;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "sech" }:
            handleFunction = MathS.Hyperbolic.Sech;
            handleFunctionInverse = MathS.Hyperbolic.Arsech;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "csch" }:
            handleFunction = MathS.Hyperbolic.Cosech;
            handleFunctionInverse = MathS.Hyperbolic.Arcosech;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "arsinh" }:
            handleFunction = MathS.Hyperbolic.Arsinh;
            handleFunctionInverse = MathS.Hyperbolic.Sinh;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "arcosh" }:
            handleFunction = MathS.Hyperbolic.Arcosh;
            handleFunctionInverse = MathS.Hyperbolic.Cosh;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "artanh" }:
            handleFunction = MathS.Hyperbolic.Artanh;
            handleFunctionInverse = MathS.Hyperbolic.Tanh;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "arcoth" }:
            handleFunction = MathS.Hyperbolic.Arcotanh;
            handleFunctionInverse = MathS.Hyperbolic.Cotanh;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "arsech" }:
            handleFunction = MathS.Hyperbolic.Arsech;
            handleFunctionInverse = MathS.Hyperbolic.Sech;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "arcsch" }:
            handleFunction = MathS.Hyperbolic.Arcosech;
            handleFunctionInverse = MathS.Hyperbolic.Cosech;
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
          case Atoms.LargeOperator { Nucleus: "lb" }:
            handleFunction = arg => MathS.Log(2, arg);
            handleFunctionInverse = arg => MathS.Pow(2, arg);
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "abs" }:
            handleFunction = MathS.Abs;
            handleFunctionInverse = arg => MathS.NaN;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "sgn" }:
            handleFunction = MathS.Signum;
            handleFunctionInverse = arg => MathS.NaN;
            goto handleFunction;
          case Atoms.LargeOperator { Nucleus: "lim", Subscript: var limitSubscript }:
            Entity limitVariable, limitTarget;
            int limitSubscriptIndex = 0;
            (limitVariable, error) = Transform(limitSubscript, ref limitSubscriptIndex, Precedence.LimitSubscriptContext).ExpectEntity("limit variable in subscript");
            if (error != null) return error;
            if (limitSubscriptIndex >= limitSubscript.Count) return "Missing → in limit subscript";
            limitSubscriptIndex++;
            (limitTarget, error) = Transform(limitSubscript, ref limitSubscriptIndex, Precedence.LimitSubscriptContext).ExpectEntity("limit target in subscript");
            if (error != null) return error;
            var limitDirection =
              limitSubscriptIndex == limitSubscript.Count + 2
              ? ApproachFrom.Left
              : limitSubscriptIndex == limitSubscript.Count + 3
              ? ApproachFrom.Right
              : ApproachFrom.BothSides;
            subscriptAllowed = true;
            handlePrecedence = Precedence.CalculusOperation;
            handlePrefix = limitBody => MathS.Limit(limitBody, limitVariable, limitTarget, limitDirection);
            goto handlePrefix;
          case Atoms.LargeOperator { Nucleus: "∫" }:
            (var integralFrom, error) = Transform(atom.Subscript);
            if (error != null) return error;
            (var integralTo, error) = Transform(atom.Superscript);
            if (error != null) return error;
            (Entity from, Entity to)? integralFromTo;
            switch (integralFrom, integralTo) {
              case (null, null): integralFromTo = null; break;
              case ( { }, { }):
                (var fromEntity, error) = integralFrom.AsEntity("integral lower limit");
                if (error != null) return error;
                (var toEntity, error) = integralTo.AsEntity("integral upper limit");
                if (error != null) return error;
                integralFromTo = (fromEntity, toEntity);
                break;
              case (null, { }): return "Integrals with only the upper limit are not supported";
              case ( { }, null): return "Integrals with only the lower limit are not supported";
            }
            i++;
            (var integralBody, error) = Transform(mathList, ref i, Precedence.IntegralBodyContext).ExpectEntityOrNull("integral body");
            integralBody ??= 1;
            if (error != null) return error;
            if (i >= mathList.Count) return "Missing integral variable. Did you forget to prepend it with upright d?";
            if (mathList[i] is not Atoms.Variable { FontStyle: FontStyle.Roman, Nucleus: "d" } integralD)
              return "Expected integral variable with upright d, got " + mathList[i].TypeName + " " + mathList[i].Nucleus;
            i++;
            (var integralVariable, error) = Transform(mathList, ref i, Precedence.FunctionApplication).ExpectEntity("integral variable");
            if (error != null) return error;
            atom.Superscript.Clear();
            @this = new Entity.Integralf(integralBody, integralVariable, integralFromTo);
            subscriptAllowed = true;
            goto handleThis;
          case Atoms.BinaryOperator { Nucleus: "+" }:
            handlePrecedence = Precedence.AdditionSubtraction;
            handleBinary = (a, b) => a + b;
            goto handleBinary;
          case Atoms.BinaryOperator { Nucleus: "\u2212" }:
            handlePrecedence = Precedence.AdditionSubtraction;
            handleBinary = (a, b) => a - b;
            goto handleBinary;
          case Atoms.BinaryOperator { Nucleus: "×" }:
          case Atoms.BinaryOperator { Nucleus: "·" }:
            handlePrecedence = Precedence.MultiplicationDivision;
            handleBinary = (a, b) => a * b;
            goto handleBinary;
          case Atoms.BinaryOperator { Nucleus: "÷" }:
          case Atoms.Ordinary { Nucleus: "/" }:
            handlePrecedence = Precedence.MultiplicationDivision;
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
            if (prec < Precedence.Comma) {
              if (prev is null) return "Missing left operand for comma";
              i++;
              (next, error) = Transform(mathList, ref i, Precedence.Comma - 1);
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
            handlePrecedence = Precedence.SetIntersection;
            handleBinary = MathS.Intersection;
            goto handleBinary;
          case Atoms.BinaryOperator { Nucleus: "∪" }:
            handlePrecedence = Precedence.SetUnionSubtraction;
            handleBinary = MathS.Union;
            goto handleBinary;
          case Atoms.BinaryOperator { Nucleus: "∖" }:
            handlePrecedence = Precedence.SetUnionSubtraction;
            handleBinary = MathS.SetSubtraction;
            goto handleBinary;
          case Atoms.Ordinary { Nucleus: "⊤" }:
            @this = Entity.Boolean.True;
            goto handleThis;
          case Atoms.Ordinary { Nucleus: "⊥" }:
            @this = Entity.Boolean.False;
            goto handleThis;
          case Atoms.Ordinary { Nucleus: "¬" }:
            handlePrecedence = Precedence.Negation;
            handlePrefix = MathS.Negation;
            goto handlePrefix;
          case Atoms.BinaryOperator { Nucleus: "∧" }:
            handlePrecedence = Precedence.Conjunction;
            handleBinary = MathS.Conjunction;
            goto handleBinary;
          case Atoms.BinaryOperator { Nucleus: "⌅" }:
            handlePrecedence = Precedence.Conjunction;
            handleBinary = (x, y) => MathS.Negation(MathS.Conjunction(x, y));
            goto handleBinary;
          case Atoms.BinaryOperator { Nucleus: "∨" }:
            handlePrecedence = Precedence.Disjunction;
            handleBinary = MathS.Disjunction;
            goto handleBinary;
          case Atoms.BinaryOperator { Nucleus: "⊻" }:
            handlePrecedence = Precedence.ExclusiveDisjunction;
            handleBinary = MathS.ExclusiveDisjunction;
            goto handleBinary;
          case Atoms.Relation { Nucleus: "↮" }:
            handlePrecedence = Precedence.Equivalence; // Same as ↔ (analogous to ≠ and =)
            handleBinary = MathS.ExclusiveDisjunction;
            goto handleBinary;
          case Atoms.Relation { Nucleus: "↔" }:
            handlePrecedence = Precedence.Equivalence;
            handleBinary = (x, y) => MathS.Negation(MathS.ExclusiveDisjunction(x, y)); // XNOR = equivalence
            goto handleBinary;
          case Atoms.Relation { Nucleus: "→" }:
            if (prec != Precedence.LimitSubscriptContext) {
              handlePrecedence = Precedence.Implication;
              handleBinary = MathS.Implication;
              binaryIsRightAssociative = true;
              goto handleBinary;
            } else return prev;
          case Atoms.Relation { Nucleus: "↛" }:
            handlePrecedence = Precedence.Implication;
            handleBinary = (x, y) => MathS.Negation(MathS.Implication(x, y));
            binaryIsRightAssociative = true;
            goto handleBinary;
          case Atoms.Relation { Nucleus: "←" }:
            handlePrecedence = Precedence.Implication;
            handleBinary = (x, y) => MathS.Implication(y, x);
            goto handleBinary;
          case Atoms.Relation { Nucleus: "↚" }:
            handlePrecedence = Precedence.Implication;
            handleBinary = (x, y) => MathS.Negation(MathS.Implication(y, x));
            goto handleBinary;
          case Atoms.Relation { Nucleus: "∈" }:
            handlePrecedence = Precedence.SetMembership;
            handleBinary = MathS.Sets.ElementInSet;
            goto handleBinary;
          case Atoms.Relation { Nucleus: "∉" }:
            handlePrecedence = Precedence.SetMembership;
            handleBinary = (element, set) => MathS.Negation(MathS.Sets.ElementInSet(element, set));
            goto handleBinary;
          case Atoms.Relation { Nucleus: "∋" }:
            handlePrecedence = Precedence.SetMembership;
            handleBinary = (set, element) => MathS.Sets.ElementInSet(element, set);
            goto handleBinary;
          // For comparison operators, we directly construct the node to explicitly not use
          // chained comparisons handling in Entity.Equalizes / MathS.Equality from AngouriMath
          // as that would interpret (x=y)=z as x=y=z. Instead, for (x=y)=z, we don't apply the expansion of x=y=z to x=y∧y=z.
          case Atoms.Relation { Nucleus: "=" }:
            handlePrecedence = Precedence.Comparison;
            handleBinary = (x, y) => new Entity.Equalsf(x, y);
            goto handleBinary;
          case Atoms.Relation { Nucleus: "≠" }:
            handlePrecedence = Precedence.Comparison;
            handleBinary = (x, y) => MathS.Negation(new Entity.Equalsf(x, y));
            goto handleBinary;
          case Atoms.Relation { Nucleus: "<" }:
            handlePrecedence = Precedence.Comparison;
            handleBinary = (x, y) => new Entity.Lessf(x, y);
            goto handleBinary;
          case Atoms.Relation { Nucleus: "≤" or "⩽" }:
            handlePrecedence = Precedence.Comparison;
            handleBinary = (x, y) => new Entity.LessOrEqualf(x, y);
            goto handleBinary;
          case Atoms.Relation { Nucleus: ">" }:
            handlePrecedence = Precedence.Comparison;
            handleBinary = (x, y) => new Entity.Greaterf(x, y);
            goto handleBinary;
          case Atoms.Relation { Nucleus: "≥" or "⩾" }:
            handlePrecedence = Precedence.Comparison;
            handleBinary = (x, y) => new Entity.GreaterOrEqualf(x, y);
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
          // cases environment
          case Atoms.Inner { LeftBoundary.Nucleus: "{", InnerList: [Atoms.Space, Atoms.Table { Environment: "array" } cases], RightBoundary.Nucleus: null }:
            var caseRows = cases.Cells.Count;
            var caseElements = new Entity.Providedf[caseRows];
            for (var row = 0; row < caseRows; row++)
              switch (cases.Cells[row]) {
                case [var col1]:
                  (var expression, error) = Transform(col1).ExpectEntity("case expression");
                  if (error != null) return error;
                  caseElements[row] = new Entity.Providedf(expression, Entity.Boolean.True);
                  break;
                case [var col1, var col2]:
                  (expression, error) = Transform(cases.Cells[row][0]).ExpectEntity("case expression");
                  if (error != null) return error;
                  Entity predicate;
                  if (col2 is [Atoms.Style,
                               Atoms.Variable { Nucleus: "o", FontStyle: FontStyle.Roman, Superscript: [], Subscript: [] },
                               Atoms.Variable { Nucleus: "t", FontStyle: FontStyle.Roman, Superscript: [], Subscript: [] },
                               Atoms.Variable { Nucleus: "h", FontStyle: FontStyle.Roman, Superscript: [], Subscript: [] },
                               Atoms.Variable { Nucleus: "e", FontStyle: FontStyle.Roman, Superscript: [], Subscript: [] },
                               Atoms.Variable { Nucleus: "r", FontStyle: FontStyle.Roman, Superscript: [], Subscript: [] },
                               Atoms.Variable { Nucleus: "w", FontStyle: FontStyle.Roman, Superscript: [], Subscript: [] },
                               Atoms.Variable { Nucleus: "i", FontStyle: FontStyle.Roman, Superscript: [], Subscript: [] },
                               Atoms.Variable { Nucleus: "s", FontStyle: FontStyle.Roman, Superscript: [], Subscript: [] },
                               Atoms.Variable { Nucleus: "e", FontStyle: FontStyle.Roman, Superscript: [], Subscript: [] }])
                    predicate = MathS.Boolean.True;
                  else {
                    var casePredicateIndex = 0;
                    (predicate, error) = Transform(cases.Cells[row][1], ref casePredicateIndex, Precedence.CasePredicateContext).ExpectEntity("case predicate");
                    if (error != null) return error;
                  }
                  caseElements[row] = new Entity.Providedf(expression, predicate);
                  break;
                default: return $"The cases environment must have 1 to 2 columns per row";
              }
            @this = MathS.Piecewise(caseElements);
            goto handleThis;
          case Atoms.Open { Nucleus: var opening }:
            if (atom.Superscript.Count > 0)
              return "Superscripts are unsupported for Open Bracket " + opening;
            if (!OpenBracketInfo.TryGetValue(opening, out var bracketInfo))
              return "Unsupported opening bracket " + opening;
            i++;
            (@this, error) = Transform(mathList, ref i, bracketInfo.KnownPrecedence);
            if (error != null) return error;
            if (i >= mathList.Count) return "Missing " + bracketInfo.InferredClosing;
            if (HandleSuperscript(ref @this, ref i, mathList[i].Superscript).Error is { } superscriptError)
              return superscriptError;
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
            if (InnerHandlers.TryGetValue((contextInfo.KnownOpening, rightBracket), out var handler))
              return handler(prev).Bind(x => (MathItem?)x);
            else return $"Unrecognized bracket pair {contextInfo.KnownOpening} {rightBracket}";
          case Atoms.Inner { LeftBoundary.Nucleus: var left, InnerList: var inner, RightBoundary.Nucleus: var right }:
            (@this, error) = Transform(inner);
            if (error != null) return error;
            (@this, error) =
              InnerHandlers.TryGetValue((left, right), out handler)
              ? handler(@this)
              : $"Unrecognized bracket pair {left ?? "(empty)"} {right ?? "(empty)"}";
            if (error != null) return error;
            goto handleThis;
          case Atoms.Space _:
          case Atoms.Style _:
          case Atoms.Comment _:
          case Atoms.Ordinary { Nucleus: var nucleus } when string.IsNullOrWhiteSpace(nucleus):
            if (atom.Superscript.Count > 0)
              return $"Superscripts are unsupported for {atom.TypeName}";
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
                case Atoms.Inner { LeftBoundary.Nucleus: "(" or "[", RightBoundary.Nucleus: ")" or "]" } inner:
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
            if (atom.Superscript is not [])
              return $"Superscripts are unsupported for {atom.TypeName} {atom.Nucleus}";
            i++;
            (next, error) = Transform(mathList, ref i, handlePrecedence);
            if (error != null) return error;
            (@this, error) = next.AsEntity("right operand for " + atom.Nucleus).Bind(e => (MathItem)handlePrefix(e));
            if (error != null) return error;
            goto handleThis;

          handleBinary:
            if (atom.Superscript is not [])
              return $"Superscripts are unsupported for {atom.TypeName} {atom.Nucleus}";
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
            if (prec < handlePrecedence) {
              i++;
              (next, error) = Transform(mathList, ref i, binaryIsRightAssociative ? handlePrecedence - 1 : handlePrecedence);
              if (error != null) return error;
              (var l, error) = prev.AsEntity("left operand for " + atom.Nucleus);
              if (error != null) return error;
              (var r, error) = next.AsEntity("right operand for " + atom.Nucleus);
              if (error != null) return error;
              if (handlePrecedence == Precedence.Comparison) {
                @this =
                  chainedComparisonTarget is { } target
                  ? MathS.Conjunction(l, handleBinary(target, r)) // Chained comparison: a < b < c becomes (a < b) ∧ (b < c)
                  : handleBinary(l, r);
                chainedComparisonTarget = r;
              } else @this = handleBinary(l, r);
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
            error = HandleSuperscript(ref @this, ref i, atom.Superscript).Error;
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
      return prev;
    }
  }
}
using System;
using System.Collections.Generic;
using AngouriMath;

namespace CSharpMath {
  using Atom;
  using Atoms = Atom.Atoms;
  public class Evaluation {
    enum Precedence {
      // Order matters
      Lowest,
      Bracket,
      AddSubtract,
      MultiplyDivide,
      FunctionApplication,
      UnaryPlusMinus
    }
    public static MathList MathListFromEntity(Entity entity) =>
      LaTeXParser.MathListFromLaTeX(entity.Latexise())
      // CSharpMath must handle all LaTeX coming from MathS or a bug is present!
      .Match(list => list, e => throw new Structures.InvalidCodePathException(e));
    public static Structures.Result<Entity> MathListToEntity(MathList mathList) {
      return Transform(mathList.Clone(true));
    }
    static Structures.Result<Entity> Transform(MathList mathList) {
      int i = 0;
      return Transform(mathList, ref i, Precedence.Lowest);
    }
    static Structures.Result<Entity> Transform(MathList mathList, ref int i, Precedence prec) {
      Entity? prevEntity = null;
      Entity nextEntity;
      string? error;
      Precedence handlePrecendence;
      Func<Entity, Entity> handleUnary;
      Func<Entity, Entity, Entity> handleBinary;
      for (; i < mathList.Count; i++) {
        var atom = mathList[i];
        Entity thisEntity;
        switch (atom) {
          case Atoms.Placeholder _:
            return "Placeholders should be filled";
          case Atoms.Number n:
            thisEntity = new NumberEntity(AngouriMath.Core.Number.Parse(n.Nucleus));
            goto setEntity;
          case Atoms.Variable v:
            thisEntity = v.Nucleus switch
            {
              "e" => MathS.e,
              "π" => MathS.pi,
              "i" => new NumberEntity(MathS.i),
              var name => new VariableEntity(name)
            };
            goto setEntity;
          case Atoms.Fraction f:
            Entity numerator, denominator;
            (numerator, error) = Transform(f.Numerator);
            if (error != null) return error;
            (denominator, error) = Transform(f.Denominator);
            if (error != null) return error;
            thisEntity = numerator / denominator;
            goto setEntity;
          case Atoms.Radical r:
            Entity degree, radicand;
            if (r.Degree.IsEmpty())
              degree = new NumberEntity(0.5);
            else {
              (degree, error) = Transform(r.Degree);
              if (error != null) return error;
              degree = 1 / degree;
            }
            (radicand, error) = Transform(r.Radicand);
            if (error != null) return error;
            thisEntity = MathS.Pow(radicand, degree);
            goto setEntity;
          case Atoms.Open { Nucleus: "(" }:
            i++;
            (thisEntity, error) = Transform(mathList, ref i, Precedence.Bracket);
            if (error != null) return error;
            goto setEntity;
          case Atoms.Close { Nucleus: ")", Superscript:var super }:
            if (prevEntity == null)
              return "Expected math before )";
            switch (prec) {
              case Precedence.Lowest:
                return "Unexpected )";
              case Precedence.Bracket:
                if (super.IsNonEmpty()) {
                  (degree, error) = Transform(super);
                  if (error != null) return error;
                  prevEntity = MathS.Pow(prevEntity, degree);
                }
                return prevEntity;
              default:
                i--;
                return prevEntity;
            }
          case Atoms.Inner { LeftBoundary: { Nucleus: "(" }, InnerList:var inner, RightBoundary: { Nucleus: ")" } }:
            (thisEntity, error) = Transform(inner);
            if (error != null) return error;
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
            handlePrecendence = Precedence.FunctionApplication;
            handleUnary = MathS.Sin;
            goto handleUnary;
          case Atoms.LargeOperator { Nucleus: "cos" }:
            handlePrecendence = Precedence.FunctionApplication;
            handleUnary = MathS.Cos;
            goto handleUnary;
          case Atoms.LargeOperator { Nucleus: "tan" }:
            handlePrecendence = Precedence.FunctionApplication;
            handleUnary = MathS.Tan;
            goto handleUnary;
          case Atoms.LargeOperator { Nucleus: "cot" }:
            handlePrecendence = Precedence.FunctionApplication;
            handleUnary = MathS.Cotan;
            goto handleUnary;
          case Atoms.LargeOperator { Nucleus: "sec" }:
            handlePrecendence = Precedence.FunctionApplication;
            handleUnary = MathS.Sec;
            goto handleUnary;
          case Atoms.LargeOperator { Nucleus: "csc" }:
            handlePrecendence = Precedence.FunctionApplication;
            handleUnary = MathS.Cosec;
            goto handleUnary;
          case Atoms.LargeOperator { Nucleus: "arcsin" }:
            handlePrecendence = Precedence.FunctionApplication;
            handleUnary = MathS.Arcsin;
            goto handleUnary;
          case Atoms.LargeOperator { Nucleus: "arccos" }:
            handlePrecendence = Precedence.FunctionApplication;
            handleUnary = MathS.Arccos;
            goto handleUnary;
          case Atoms.LargeOperator { Nucleus: "arctan" }:
            handlePrecendence = Precedence.FunctionApplication;
            handleUnary = MathS.Arctan;
            goto handleUnary;
          case Atoms.LargeOperator { Nucleus: "arccot" }:
            handlePrecendence = Precedence.FunctionApplication;
            handleUnary = MathS.Arccotan;
            goto handleUnary;
          case Atoms.LargeOperator { Nucleus: "arcsec" }:
            handlePrecendence = Precedence.FunctionApplication;
            handleUnary = MathS.Arcsec;
            goto handleUnary;
          case Atoms.LargeOperator { Nucleus: "arccsc" }:
            handlePrecendence = Precedence.FunctionApplication;
            handleUnary = MathS.Arccosec;
            goto handleUnary;
          case Atoms.LargeOperator { Nucleus: "log", Subscript:var @base }:
            handlePrecendence = Precedence.FunctionApplication;
            Entity logBase;
            if (@base.IsNonEmpty()) {
              (logBase, error) = Transform(@base);
              if (error != null) return error;
            } else logBase = new NumberEntity(10);
            handleUnary = arg => MathS.Log(arg, logBase);
            goto handleUnary;
          case Atoms.LargeOperator { Nucleus: "ln" }:
            handlePrecendence = Precedence.FunctionApplication;
            handleUnary = MathS.Ln;
            goto handleUnary;
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
          case Atoms.Space _:
          case Atoms.Ordinary { Nucleus: var nucleus } when string.IsNullOrWhiteSpace(nucleus):
            continue;
          default:
            return $"Unsupported {atom.TypeName} {atom.Nucleus}";
          handleUnary:
            i++;
            MathList? bracketArgument = null;
            if (handlePrecendence == Precedence.FunctionApplication) {
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
                  case Atoms.Close { HasCorrespondingOpen:true } close:
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
              exitFor:;
            }
            (nextEntity, error) =
              bracketArgument == null
              ? Transform(mathList, ref i, handlePrecendence)
              : Transform(bracketArgument);
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
              (nextEntity, error) = Transform(mathList, ref i, handlePrecendence);
              if (error != null) return error;
              thisEntity = handleBinary(prevEntity, nextEntity);
              prevEntity = null; // We used up the previous entity, don't keep it
              goto setEntity;
            } else {
              i--;
              return prevEntity;
            }
          setEntity:
            if (atom.Superscript.IsNonEmpty()) {
              Entity exponent;
              (exponent, error) = Transform(atom.Superscript);
              if (error != null) return error;
              thisEntity = MathS.Pow(thisEntity, exponent);
            }
            prevEntity = prevEntity is { } ? prevEntity * thisEntity : thisEntity;
            break;
        }
      }
      if (prec == Precedence.Bracket) return "Missing closing parentheses";
      if (prevEntity is null) return "Expected math but not found";
      else return prevEntity;
    }
  }
}

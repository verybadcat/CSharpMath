using System;
using System.Collections.Generic;
using AngouriMath;

namespace CSharpMath.Evaluation {
  using Atom;
  using Atoms = Atom.Atoms;
  public class MathS {
    enum Precedence {
      // Order matters
      Lowest,
      Bracket,
      AddSubtract,
      FunctionApplication,
      MultiplyDivide
    }
    public static MathList ToMathList(Entity entity) =>
      LaTeXParser.MathListFromLaTeX(entity.Latexise())
      // CSharpMath must handle all LaTeX coming from MathS or a bug is present!
      .Match(list => list, e => throw new Structures.InvalidCodePathException(e));
    public static Structures.Result<Entity> FromMathList(MathList mathList) {
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
            return "Placeholders should not be present";
          case Atoms.Number n:
            thisEntity = new NumberEntity(AngouriMath.Core.Number.Parse(n.Nucleus));
            goto setEntity;
          case Atoms.Variable v:
            thisEntity = v.Nucleus switch
            {
              "e" => AngouriMath.MathS.e,
              "π" => AngouriMath.MathS.pi,
              "i" => new NumberEntity(AngouriMath.MathS.i),
              var name => new VariableEntity(name)
            };
            goto setEntity;
          case Atoms.Fraction f:
            Entity numerator, denominator;
            (numerator, error) = Transform(f.Numerator);
            if (error != null) return error;
            (denominator, error) = Transform(f.Denominator);
            if (error != null) return error;
            thisEntity = Divf.Hang(numerator, denominator);
            goto setEntity;
          case Atoms.Radical r:
            Entity degree, radicand;
            if (r.Degree.IsEmpty())
              degree = new NumberEntity(2);
            else {
              (degree, error) = Transform(r.Degree);
              if (error != null) return error;
            }
            (radicand, error) = Transform(r.Radicand);
            thisEntity = Powf.Hang(radicand, Divf.Hang(new NumberEntity(1), degree));
            goto setEntity;
          case Atoms.Open { Nucleus: "(" }:
            i++;
            (thisEntity, error) = Transform(mathList, ref i, Precedence.Bracket);
            if (error != null) return error;
            goto setEntity;
          case Atoms.Close { Nucleus: ")" }:
            if (prec == Precedence.Lowest)
              return "Unexpected closing parenthesis";
            return prevEntity;
          case Atoms.Inner { LeftBoundary: { Nucleus: "(" }, InnerList:var inner, RightBoundary: { Nucleus: ")" } }:
            (thisEntity, error) = Transform(inner);
            if (error != null) return error;
            goto setEntity;
          case Atoms.UnaryOperator { Nucleus: "+" }:
            handlePrecendence = Precedence.MultiplyDivide;
            handleUnary = e => e;
            goto handleUnary;
          case Atoms.UnaryOperator { Nucleus: "\u2212" }:
            handlePrecendence = Precedence.MultiplyDivide;
            handleUnary = e => -e;
            goto handleUnary;
          case Atoms.LargeOperator { Nucleus: "sin" }:
            handlePrecendence = Precedence.FunctionApplication;
            handleUnary = Sinf.Hang;
            goto handleUnary;
          case Atoms.LargeOperator { Nucleus: "cos" }:
            handlePrecendence = Precedence.FunctionApplication;
            handleUnary = Cosf.Hang;
            goto handleUnary;
          case Atoms.LargeOperator { Nucleus: "tan" }:
            handlePrecendence = Precedence.FunctionApplication;
            handleUnary = Tanf.Hang;
            goto handleUnary;
          case Atoms.LargeOperator { Nucleus: "cot" }:
            handlePrecendence = Precedence.FunctionApplication;
            handleUnary = Cotanf.Hang;
            goto handleUnary;
          case Atoms.LargeOperator { Nucleus: "log", Subscript:var @base }:
            handlePrecendence = Precedence.FunctionApplication;
            Entity logBase;
            if (@base.IsNonEmpty()) {
              (logBase, error) = Transform(@base);
              if (error != null) return error;
            } else logBase = new NumberEntity(10);
            handleUnary = arg => Logf.Hang(arg, logBase);
            goto handleUnary;
          case Atoms.LargeOperator { Nucleus: "ln" }:
            handlePrecendence = Precedence.FunctionApplication;
            handleUnary = arg => Logf.Hang(arg, AngouriMath.MathS.e);
            goto handleUnary;
          case Atoms.BinaryOperator { Nucleus: "+" }:
            handlePrecendence = Precedence.AddSubtract;
            handleBinary = Sumf.Hang;
            goto handleBinary;
          case Atoms.BinaryOperator { Nucleus: "\u2212" }:
            handlePrecendence = Precedence.AddSubtract;
            handleBinary = Minusf.Hang;
            goto handleBinary;
          case Atoms.BinaryOperator { Nucleus: "*" }:
          case Atoms.BinaryOperator { Nucleus: "×" }:
          case Atoms.BinaryOperator { Nucleus: "·" }:
            handlePrecendence = Precedence.MultiplyDivide;
            handleBinary = Mulf.Hang;
            goto handleBinary;
          case Atoms.BinaryOperator { Nucleus: "÷" }:
          case Atoms.Ordinary { Nucleus: "/" }:
            handlePrecendence = Precedence.MultiplyDivide;
            handleBinary = Divf.Hang;
            goto handleBinary;
          default:
            return $"Unsupported {atom.TypeName} with nucleus \"{atom.Nucleus}\"";
          handleUnary:
            i++;
            if (prevEntity is { })
              throw new Structures.InvalidCodePathException("UnaryOperators should be at the start of a new context");
            (nextEntity, error) = Transform(mathList, ref i, handlePrecendence);
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
              thisEntity = Powf.Hang(thisEntity, exponent);
            }
            prevEntity = prevEntity is { } ? Mulf.Hang(prevEntity, thisEntity) : thisEntity;
            break;
        }
      }
      if (prevEntity is null) return "Expected math but not found";
      else return prevEntity;
    }
  }
}

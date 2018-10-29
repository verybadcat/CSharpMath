using System;
using System.Collections.ObjectModel;
using System.Drawing;

namespace CSharpMath.Editor {
  public class MathKeyboardView<TButton, TTextView> where TButton : IButton where TTextView : class, ITextView {
    public MathKeyboardView(TButton fractionButton, TButton multiplyButton, TButton equalsButton, TButton divisionButton, TButton exponentButton, TButton lessEqualsButton, TButton greaterEqualsButton, TButton shiftButton, TButton squareRootButton, TButton radicalButton, TTextView textView, TButton[] numbers, TButton[] variables, TButton[] operators, TButton[] relations, TButton[] letters, TButton[] greekLetters, TButton alphaRho, TButton deltaOmega, TButton sigmaPhi, TButton muNu, TButton lambdaBeta) {
      _currentTab = NumbersTab;
      _currentTab.Selected = true;

      Tabs = new ReadOnlyCollection<MathKeyboard<TButton, TTextView>>(new[] {
        new MathKeyboard<TButton, TTextView>(fractionButton, multiplyButton, equalsButton, divisionButton, exponentButton, lessEqualsButton, greaterEqualsButton, shiftButton, squareRootButton, radicalButton, textView, numbers, variables, operators, relations, letters, greekLetters, alphaRho, deltaOmega, sigmaPhi, muNu, lambdaBeta),
        new MathKeyboard<TButton, TTextView>(fractionButton, multiplyButton, equalsButton, divisionButton, exponentButton, lessEqualsButton, greaterEqualsButton, shiftButton, squareRootButton, radicalButton, textView, numbers, variables, operators, relations, letters, greekLetters, alphaRho, deltaOmega, sigmaPhi, muNu, lambdaBeta),
        new MathKeyboard<TButton, TTextView>(fractionButton, multiplyButton, equalsButton, divisionButton, exponentButton, lessEqualsButton, greaterEqualsButton, shiftButton, squareRootButton, radicalButton, textView, numbers, variables, operators, relations, letters, greekLetters, alphaRho, deltaOmega, sigmaPhi, muNu, lambdaBeta),
        new MathKeyboard<TButton, TTextView>(fractionButton, multiplyButton, equalsButton, divisionButton, exponentButton, lessEqualsButton, greaterEqualsButton, shiftButton, squareRootButton, radicalButton, textView, numbers, variables, operators, relations, letters, greekLetters, alphaRho, deltaOmega, sigmaPhi, muNu, lambdaBeta),
      });
    }
    //public static readonly MathKeyboardView<TButton, TTextView> Instance = new MathKeyboardView<TButton, TTextView>();

    public ReadOnlyCollection<MathKeyboard<TButton, TTextView>> Tabs { get; }
    public MathKeyboard<TButton, TTextView> NumbersTab => Tabs[0];
    public MathKeyboard<TButton, TTextView> LettersTab => Tabs[1];
    public MathKeyboard<TButton, TTextView> FunctionsTab => Tabs[2];
    public MathKeyboard<TButton, TTextView> OperationsTab => Tabs[3];
    private MathKeyboard<TButton, TTextView> _currentTab;
    public MathKeyboard<TButton, TTextView> CurrentTab {
      get => _currentTab;
      set {
        _currentTab.Selected = false;
        _currentTab = value;
        value.Selected = true;
      }
    }
    public Atoms.MathList MathList { get; set; }

    public void StartedEditing(TTextView label) {
      foreach (var tab in Tabs)
        tab.textView = label;
    }

    public void FinishedEditing(TTextView label) {
      foreach (var tab in Tabs)
        tab.textView = null;
    }
  }
}
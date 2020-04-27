namespace CSharpMath {
  using System;
  partial class Evaluation {
    public static double? Predict(Span<double> originalList) {
      if (originalList.IsEmpty) return null;
      static void RunningSums(Span<double> numbers, Func<double, double, double> map) {
        for (var i = 1; i < numbers.Length; i++)
          numbers[i] = map(numbers[i - 1], numbers[i]);
      }
      static void AntiRunningSums(Span<double> numbers, Func<double, double, double> map) {
        for (var i = numbers.Length - 1; i > 0; i--)
          numbers[i] = map(numbers[i], numbers[i - 1]);
      }
      static ReadOnlySpan<double> RepeatingUnit(ReadOnlySpan<double> numbers) {
        static bool AllEqual(ReadOnlySpan<double> span1, ReadOnlySpan<double> span2) {
          if (span1.Length != span2.Length) return false;
          for (var i = 0; i < span1.Length; i++)
            if (Math.Abs(span1[i] - span2[i]) > 1e-15)
              return false;
          return true;
        }
        // The repeating unit must occur at least twice in numbers
        for (int subLength = 1; subLength <= numbers.Length / 2; subLength++) {
          var sub = numbers.Slice(0, subLength);
          for (int subIndex = subLength; subIndex <= numbers.Length; subIndex += subLength) {
            // Make sure 1,2,1,2,1 has repeating unit 1,2 but not 1,2,1,2,2 so consider sub slices
            var subSliceLength = Math.Min(subLength, numbers.Length - subIndex);
            if (!AllEqual(sub.Slice(0, subSliceLength), numbers.Slice(subIndex, subSliceLength)))
              goto notRepeatingUnit;
          }
          return sub;
          notRepeatingUnit:;
        }
        return ReadOnlySpan<double>.Empty;
      }
      Span<double> list = stackalloc double[originalList.Length];
      originalList.CopyTo(list);
      Span<double> newList = stackalloc double[list.Length + 1];
      static bool TryInterpretSequence(Span<double> list, Span<double> newList, int depth, ContinuationType type) {
        // Don't consider starting number, e.g. 1,2,4,8 deriv-> 1,2,2,2
        var repeatingUnit = RepeatingUnit(list.Slice(1));
        var startAt = 1;
        if (repeatingUnit.IsEmpty && depth == 0) {
          // Only consider starting number when it can be made sense by the user, e.g. 1,2,1,2 or 1,1
          repeatingUnit = RepeatingUnit(list);
          startAt = 0;
        }
        if (!repeatingUnit.IsEmpty) {
          list.CopyTo(newList);
          newList[newList.Length - 1] = repeatingUnit[(list.Length - startAt) % repeatingUnit.Length];
          return true;
        }
        if (type == ContinuationType.Geometric && list.IndexOf(0) >= 0)
          return false; // Do not try to divide by zero
        foreach (var elem in list) System.Diagnostics.Debug.WriteLine("1/" + new string(' ', depth) + type + elem);
        AntiRunningSums(list, type switch
        {
          ContinuationType.Arithmetic => (a, b) => a - b,
          ContinuationType.Geometric => (a, b) => a / b,
          _ => throw new NotImplementedException()
        });
        foreach (var elem in list) System.Diagnostics.Debug.WriteLine("2/" + new string(' ', depth) + type + elem);
        foreach (var t in stackalloc ContinuationType[] {
          ContinuationType.Arithmetic,
          ContinuationType.Geometric
        })
          if (depth < 2 && TryInterpretSequence(list, newList, depth + 1, t)) {
            foreach (var elem in newList) System.Diagnostics.Debug.WriteLine("3/" + new string(' ', depth) + type + elem);
            RunningSums(newList, type switch
            {
              ContinuationType.Arithmetic => (a, b) => a + b,
              ContinuationType.Geometric => (a, b) => a * b,
              _ => throw new NotImplementedException()
            });
            foreach (var elem in newList) System.Diagnostics.Debug.WriteLine("4/" + new string(' ', depth) + type + elem);
            return true;
          }
        RunningSums(list, type switch
        {
          ContinuationType.Arithmetic => (a, b) => a + b,
          ContinuationType.Geometric => (a, b) => a * b,
          _ => throw new NotImplementedException()
        });
        return false;
      }
      foreach (var t in stackalloc ContinuationType[] {
        ContinuationType.Arithmetic,
        ContinuationType.Geometric
      })
        if (TryInterpretSequence(list, newList, 0, t))
          return newList[newList.Length - 1];
      return null;
    }
    enum ContinuationType { Arithmetic, Geometric }
  }
}
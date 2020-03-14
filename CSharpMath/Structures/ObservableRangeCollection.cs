using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace CSharpMath.Structures {
// No need to scream at helper "disposables"
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
  public class ObservableRangeCollection<T> : ObservableCollection<T> {
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
    public ObservableRangeCollection() : base() { }
    public ObservableRangeCollection(IEnumerable<T> collection) : base(collection) { }
    public ObservableRangeCollection(List<T> list) : base(list) { }
    readonly BatchOperation batch = new BatchOperation();
    /// <summary>You should use this in a using block.</summary>
    public BatchOperation BatchOperationBlock() => batch.Deploy();
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
      // intercept this when it gets called inside the AddRange method.
      if (!batch.IsInForce) base.OnCollectionChanged(e);
    }
    public void AddRange(IEnumerable<T> items) {
      using (BatchOperationBlock()) foreach (var item in items) Add(item);
      base.OnCollectionChanged(
        new NotifyCollectionChangedEventArgs(
          NotifyCollectionChangedAction.Add,
          items is IList l ? l : items.ToList()
        )
      );
    }
    public void RemoveRange(IEnumerable<T> items) {
      using (BatchOperationBlock()) foreach (var item in items) Remove(item);
      base.OnCollectionChanged(
        new NotifyCollectionChangedEventArgs(
          NotifyCollectionChangedAction.Remove,
          items is IList l ? l : items.ToList()
        )
      );
    }
#pragma warning disable CA1034 // Nested types should not be visible
    public sealed class BatchOperation : IDisposable {
#pragma warning restore CA1034 // Nested types should not be visible
      public bool IsInForce { get; private set; }
      internal BatchOperation Deploy() { IsInForce = true; return this; }
      public void Dispose() => IsInForce = false;
    }
  }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace CSharpMath.Rendering
{
  public class ObservableRangeCollection<T> : ObservableCollection<T> {
    public ObservableRangeCollection() : base() { }
    public ObservableRangeCollection(IEnumerable<T> collection) : base(collection) { }

    BatchOperation batch = new BatchOperation();

    /// <summary>
    /// You should use this in a using block.
    /// </summary>
    /// <returns></returns>
    public BatchOperation BatchOperationBlock() => batch.Deploy();

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
      // intercept this when it gets called inside the AddRange method.
      if(!batch.IsInForce)
        base.OnCollectionChanged(e);
    }


    public void AddRange(IEnumerable<T> items) {
      using(BatchOperationBlock())
        foreach (T item in items)
          Add(item);

      var e = new NotifyCollectionChangedEventArgs(
          NotifyCollectionChangedAction.Add,
          items is IList l ? l : items.ToList());
      base.OnCollectionChanged(e);
    }

    public void RemoveRange(IEnumerable<T> items) {
      using (BatchOperationBlock())
        foreach (T item in items)
          Remove(item);

      var e = new NotifyCollectionChangedEventArgs(
          NotifyCollectionChangedAction.Remove,
          items is IList l ? l : items.ToList());
      base.OnCollectionChanged(e);
    }

    public class BatchOperation : IDisposable {
      internal BatchOperation() => IsInForce = false;

      public bool IsInForce { get; private set; }

      internal BatchOperation Deploy() { IsInForce = true; return this; }

      public void Dispose() => IsInForce = false;
    }
  }
}

namespace CSharpMath.Helpers
{
#warning Revoke if https://github.com/dotnet/corert/issues/6179 is fixed
  readonly struct Pair<T, K> 
    {
        public Pair(T first, K second)
        {
            First = first;
            Second = second;
        }

        public T First { get; }
        public K Second { get; }
    }

    static class Pair
    {
        public static Pair<T, K> Create<T, K>(T first, K second) => new Pair<T, K>(first, second);
    }
}

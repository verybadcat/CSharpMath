namespace CSharpMath.Helpers
{
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
}

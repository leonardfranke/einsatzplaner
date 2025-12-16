namespace Optimizer
{
    public static class DictionaryExtensions
    {
        public static void TryAppend<S, T>(this Dictionary<S, List<T>> dict, S key, T value)
        {
            if(!dict.ContainsKey(key))
                dict[key] = [value];
            else
                dict[key].Add(value);
        }
    }
}

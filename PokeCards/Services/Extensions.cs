namespace PokeCards.Services;

public static class Extensions
{
    public static IEnumerable<T> GetRangeOrLess<T>(this List<T>? list, int index, int count)
    {
        if ((list?.Count ?? 0) == 0)
            return Enumerable.Empty<T>();
        var listCount = list.Count;

        if (listCount < index)
            return Enumerable.Empty<T>();

        if (index + count > listCount)
            count = listCount - index;
        
        return list.GetRange(index, count);
    }
}
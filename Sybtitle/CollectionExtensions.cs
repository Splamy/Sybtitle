namespace System.Collections.Generic;

using System.Diagnostics;
using System.Linq;

public static class CollectionExtensions
{
    [DebuggerStepThrough]
    private static bool NotNull<T>(T obj) => obj is not null;

    [DebuggerStepThrough]
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enu) where T : class
        => enu.Where(NotNull)!;

    [DebuggerStepThrough]
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enu) where T : struct
    {
        foreach (var item in enu)
        {
            if (item is { } some)
            {
                yield return some;
            }
        }
    }

    [DebuggerStepThrough]
    public static IEnumerable<TResult> SelectNotNull<T, TResult>(this IEnumerable<T> enu, Func<T, TResult?> selector) where TResult : class
        => enu.Select(selector).Where(NotNull)!;

    [DebuggerStepThrough]
    public static IEnumerable<TResult> SelectNotNull<T, TResult>(this IEnumerable<T> enu, Func<T, TResult?> selector) where TResult : struct
    {
        foreach (var item in enu)
        {
            if (selector(item) is { } some)
            {
                yield return some;
            }
        }
    }

    [DebuggerStepThrough]
    public static void ForEach<T>(this IEnumerable<T> enu, Action<T> action)
    {
        foreach (var item in enu)
        {
            action(item);
        }
    }

    [DebuggerStepThrough]
    public static string Join<T>(this IEnumerable<T> enu, string? separator) => string.Join(separator, enu);
    [DebuggerStepThrough]
    public static string Join<T>(this IEnumerable<T> enu, char separator) => string.Join(separator, enu);
    [DebuggerStepThrough]
    public static string Join(this IEnumerable<string> enu, string? separator) => string.Join(separator, enu);

    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> add)
    {
        foreach (var item in add)
        {
            collection.Add(item);
        }
    }

    /// <summary>
    /// Returns an Enumerator over all contiguous windows of length 2.
    /// The windows overlap. If the <paramref name="source"/> is shorter than 2,
    /// the iterator returns no values.
    /// </summary>
    public static IEnumerable<(T, T)> Window2<T>(this IEnumerable<T> source)
    {
        T previous = default!;
        using (var it = source.GetEnumerator())
        {
            if (!it.MoveNext()) { yield break; }

            previous = it.Current;

            while (it.MoveNext())
            {
                yield return (previous, it.Current);
                previous = it.Current;
            }
        }
    }

    /// <summary>
    /// Returns an Enumerator over all contiguous windows of length 3.
    /// The windows overlap. If the <paramref name="source"/> is shorter than 3,
    /// the iterator returns no values.
    /// </summary>
    public static IEnumerable<(T, T, T)> Window3<T>(this IEnumerable<T> source)
    {
        T t1 = default!;
        T t2 = default!;
        using (var it = source.GetEnumerator())
        {
            if (!it.MoveNext()) { yield break; }
            t1 = it.Current;
            if (!it.MoveNext()) { yield break; }
            t2 = it.Current;

            while (it.MoveNext())
            {
                yield return (t1, t2, it.Current);
                t1 = t2;
                t2 = it.Current;
            }
        }
    }

    /// <summary>
    /// Returns an Enumerator over all contiguous windows of length <paramref name="size"/>.
    /// The windows overlap. If the <paramref name="source"/> is shorter than <paramref name="size"/>,
    /// the iterator returns no values.
    /// </summary>
    /// <param name="size">The window size.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="size"/> is 0 or negative.</exception>
    public static IEnumerable<T[]> Window<T>(this IEnumerable<T> source, int size)
    {
        if (size < 1) { throw new ArgumentOutOfRangeException(nameof(size)); }

        if (source.TryGetNonEnumeratedCount(out var sourceLen) && sourceLen < size)
        {
            return Enumerable.Empty<T[]>();
        }

        return DoWindow(source, size);

        static IEnumerable<T[]> DoWindow(IEnumerable<T> source, int size)
        {
            var q = new Queue<T>(size);
            using (var it = source.GetEnumerator())
            {
                while (q.Count < size && it.MoveNext())
                {
                    q.Enqueue(it.Current);
                }

                while (q.Count == size)
                {
                    yield return q.ToArray();
                    q.Dequeue();
                    if (it.MoveNext())
                    {
                        q.Enqueue(it.Current);
                    }
                }
            }
        }
    }
}

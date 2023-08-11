using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.Core;

namespace NitroxModel;

public static class Extensions
{
    public static TAttribute GetAttribute<TAttribute>(this Enum value)
        where TAttribute : Attribute
    {
        Type type = value.GetType();
        string name = Enum.GetName(type, value);

        return type.GetField(name)
                   .GetCustomAttributes(false)
                   .OfType<TAttribute>()
                   .SingleOrDefault();
    }

    /// <summary>
    ///     Removes all items from the list when the predicate returns true.
    /// </summary>
    /// <param name="list">The list to remove items from.</param>
    /// <param name="extraParameter">An extra parameter to supply to the predicate.</param>
    /// <param name="predicate">The predicate that tests each item in the list for removal.</param>
    public static void RemoveAllFast<TItem, TParameter>(this IList<TItem> list, TParameter extraParameter, Func<TItem, TParameter, bool> predicate)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            TItem item = list[i];
            if (predicate.Invoke(item, extraParameter))
            {
                // Optimization for Unity mono: swap item to end and remove it. This reduces GC pressure for resizing arrays.
                list[i] = list[^1];
                list.RemoveAt(list.Count - 1);
            }
        }
    }

    public static int GetIndex<T>(this T[] list, T itemToFind) => Array.IndexOf(list, itemToFind);

    /// <summary>
    ///     Resolves a type using <see cref="NitroxServiceLocator.LocateService{T}" />. If the result is not null it will cache and return the same type on future calls.
    /// </summary>
    /// <typeparam name="T">Type to get and cache from <see cref="NitroxServiceLocator" /></typeparam>
    /// <returns>The requested type or null if not available.</returns>
    public static T Resolve<T>(bool prelifeTime = false) where T : class
    {
        return prelifeTime ? NitroxServiceLocator.Cache<T>.ValuePreLifetime : NitroxServiceLocator.Cache<T>.Value;
    }

    public static string AsByteUnitText(this uint byteSize)
    {
        // Uint can't go past 4GiB, so we don't need to worry about overflow.
        string[] suf = { "B", "KiB", "MiB", "GiB" };
        if (byteSize == 0)
        {
            return $"0{suf[0]}";
        }
        int place = Convert.ToInt32(Math.Floor(Math.Log(byteSize, 1024)));
        double num = Math.Round(byteSize / Math.Pow(1024, place), 1);
        return num + suf[place];
    }

    public static string GetFirstNonAggregateMessage(this Exception exception) => exception switch
    {
        AggregateException ex => ex.InnerExceptions.FirstOrDefault(e => e is not AggregateException)?.Message ?? ex.Message,
        _ => exception.Message
    };


    /// <returns>
    /// <inheritdoc cref="Enumerable.SequenceEqual{TSource}(IEnumerable{TSource}, IEnumerable{TSource})"/><br />
    /// <see langword="true" /> if both IEnumerables are null.
    /// </returns>
    /// <remarks><see cref="ArgumentNullException"/> can't be thrown because of <paramref name="first"/> or <paramref name="second"/> being null.</remarks>
    /// <inheritdoc cref="Enumerable.SequenceEqual{TSource}(IEnumerable{TSource}, IEnumerable{TSource})"/>
    public static bool SequenceEqualOrBothNull<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
    {
        if (first != null && second != null)
        {
            return first.SequenceEqual(second);
        }
        return first == second;
    }
}

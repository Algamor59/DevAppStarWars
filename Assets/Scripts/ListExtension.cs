using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        for (int i = 0; i < n; i++)
        {
            int r = Random.Range(i, n);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
}

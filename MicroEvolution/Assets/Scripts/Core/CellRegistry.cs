using System.Collections.Generic;
using UnityEngine;

namespace MicroEvolution.Core
{
    public static class CellRegistry
    {
        static readonly List<LivingCell> Cells = new List<LivingCell>(64);

        public static IReadOnlyList<LivingCell> All => Cells;

        public static void Register(LivingCell cell)
        {
            if (cell != null && !Cells.Contains(cell))
                Cells.Add(cell);
        }

        public static void Unregister(LivingCell cell)
        {
            Cells.Remove(cell);
        }

        public static LivingCell FindNearest(Vector3 origin, float range, System.Predicate<LivingCell> match)
        {
            LivingCell best = null;
            var bestDist = range * range;
            for (var i = Cells.Count - 1; i >= 0; i--)
            {
                var c = Cells[i];
                if (c == null)
                {
                    Cells.RemoveAt(i);
                    continue;
                }

                if (match != null && !match(c)) continue;
                var d = (c.transform.position - origin).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    best = c;
                }
            }

            return best;
        }
    }
}

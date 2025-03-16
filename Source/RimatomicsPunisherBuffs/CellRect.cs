using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimatomicsPunisherBuffs
{
    public class CellRect : IEnumerable<IntVec3>
    {
        private readonly IntRange x;

        private readonly IntRange z;

        public CellRect(IntVec3 center, int radius)
        {
            Map map = Find.CurrentMap;

            int minX = Mathf.Max(center.x - radius, 0);
            int maxX = Mathf.Min(center.x + radius, map.Size.x);

            int minZ = Mathf.Max(center.z - radius, 0);
            int maxZ = Mathf.Min(center.z + radius, map.Size.z);

            x = new IntRange(minX, maxX);
            z = new IntRange(minZ, maxZ);
        }

        public CellRect(int xStart, int xEnd, int zStart, int zEnd)
        {
            x = new IntRange(xStart, xEnd);
            z = new IntRange(zStart, zEnd);
        }

        public IEnumerator<IntVec3> GetEnumerator()
        {
            for (int i = x.min; i <= x.max; i++)
            {
                for (int j = z.min; j <= z.max; j++)
                {
                    yield return new IntVec3(i, 0, j);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
namespace HoleBox

{
    using System.Collections.Generic;
    using BasePuzzle.PuzzlePackages.Core;using UnityEngine;

    public static class PoolUtils
    {
        public static void ReleaseStickman(this List<Stickman> stickmans)
        {
            if (stickmans == null)
            {
                stickmans = new List<Stickman>();
                return;
            }

            foreach (var s in stickmans)
            {
                if (s && s.gameObject.activeSelf)
                    PrefabPool<Stickman>.Release(s);
            }

            stickmans = new List<Stickman>();
        }
    }
}
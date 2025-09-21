namespace HoleBox
{
    using UnityEngine;

    public class GameLogicUltils
    {
        public static Color GetColor(int holeID)
        {
            if (GameConstants.ObstacleID == holeID)
            {
                return Color.black;
            }

            if (holeID > 0 && holeID <= GameConstants.ColorID.Length)
            {
                return GameConstants.ColorID[holeID - 1];
            }

            return Color.white;
        }
    }

    public static class GameConstants
    {
        public static readonly int ObstacleID = -2;
        
        public static Color[] ColorID = new Color[]
        {
            Color.red,
            Color.green,
            Color.blue,
            Color.yellow,
            Color.cyan,
            Color.magenta
        };
    }
}
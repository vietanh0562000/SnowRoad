namespace HoleBox
{
    public class ObstaclePool : AHolePool<ObstacleData, Obstacle>
    {
        public ObstaclePool(Obstacle t) : base(t) { }
        protected override int capacity => 50;
        protected override int maxSize  => 100;
    }
}
namespace HoleBox
{
    public class StickManPool : AHolePool<StickManData, StickManBox>
    {
        public StickManPool(StickManBox t) : base(t) { }
        protected override int capacity => 50;
        protected override int maxSize  => 100;
    }
}
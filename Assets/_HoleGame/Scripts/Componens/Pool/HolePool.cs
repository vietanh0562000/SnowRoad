namespace HoleBox
{
    using global::HoleBox.HoleBox;

    public class HolePool : AHolePool<HoleBoxData, Hole>
    {
        public HolePool(Hole t) : base(t) { }
        protected override int capacity => 5;
        protected override int maxSize  => 10;
    }
}
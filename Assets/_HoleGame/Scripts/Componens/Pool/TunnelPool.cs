namespace HoleBox
{
    public class TunnelPool : AHolePool<TunnelData, Tunnel>
    {
        public TunnelPool(Tunnel t) : base(t) { }
        protected override int capacity => 5;
        protected override int maxSize  => 20;
    }
}
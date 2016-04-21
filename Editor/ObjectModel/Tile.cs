namespace TiledUtilities
{
    public struct Tile
    {
        private const uint FlipHorizontalFlag = 0x80000000;
        private const uint FlipVerticalFlag = 0x40000000;
        private const uint FlipDiagonalFlag = 0x20000000;

        public uint gid { get; private set; }
        public bool flipHorizontal { get; private set; }
        public bool flipVertical { get; private set; }
        public bool flipDiagnoal { get; private set; }

        public Tile(uint id)
            : this()
        {
            gid = id & ~(FlipHorizontalFlag | FlipVerticalFlag | FlipDiagonalFlag);
            flipHorizontal = (id & FlipHorizontalFlag) != 0;
            flipVertical = (id & FlipVerticalFlag) != 0;
            flipDiagnoal = (id & FlipDiagonalFlag) != 0;
        }

        public override string ToString()
        {
            return string.Format("{0} H:{1} V:{2} D:{3}", gid, flipHorizontal, flipVertical, flipDiagnoal);
        }
    }
}

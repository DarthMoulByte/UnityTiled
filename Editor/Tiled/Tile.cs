namespace Tiled
{
    public struct Tile
    {
        private const uint FlipHorizontalFlag = 0x80000000;
        private const uint FlipVerticalFlag = 0x40000000;
        private const uint FlipDiagonalFlag = 0x20000000;

        public uint Gid { get; private set; }
        public bool IsFlippedHorizontal { get; private set; }
        public bool IsFlippedVertical { get; private set; }
        public bool IsFlippedDiagonal { get; private set; }

        public Tile(uint id)
            : this()
        {
            Gid = id & ~(FlipHorizontalFlag | FlipVerticalFlag | FlipDiagonalFlag);
            IsFlippedHorizontal = (id & FlipHorizontalFlag) != 0;
            IsFlippedVertical = (id & FlipVerticalFlag) != 0;
            IsFlippedDiagonal = (id & FlipDiagonalFlag) != 0;
        }

        public override string ToString()
        {
            return string.Format("{0} H:{1} V:{2} D:{3}", Gid, IsFlippedHorizontal, IsFlippedVertical, IsFlippedDiagonal);
        }
    }
}

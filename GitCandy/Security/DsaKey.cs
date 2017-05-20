namespace GitCandy.Security
{
    public sealed class DsaKey : IHostKey
    {
        public byte[] P;
        public byte[] Q;
        public byte[] G;
        public byte[] Y;
        public byte[] Seed;
        public int Counter;
        public byte[] X;
    }
}

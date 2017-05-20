namespace GitCandy.Security
{
    public sealed class RsaKey : IHostKey
    {
        public byte[] Modulus;
        public byte[] Exponent;
        public byte[] P;
        public byte[] Q;
        public byte[] DP;
        public byte[] DQ;
        public byte[] InverseQ;
        public byte[] D;
    }
}

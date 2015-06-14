using System.Security.Cryptography;
using System.Text;

namespace GitCandy.Ssh.Algorithms
{
    public class DssKey : PublicKeyAlgorithm
    {
        private readonly DSACryptoServiceProvider _algorithm = new DSACryptoServiceProvider();

        public DssKey(string xml)
            : base(xml)
        {
        }

        public override string Name
        {
            get { return "ssh-dss"; }
        }

        protected override void ImportKey(string xml)
        {
            _algorithm.FromXmlString(xml);
        }

        public override void LoadKeyAndCertificatesData(byte[] data)
        {
            using (var worker = new SshDataWorker(data))
            {
                if (worker.ReadString(Encoding.ASCII) != this.Name)
                    throw new CryptographicException("Key and certificates were not created with this algorithm.");

                var args = new DSAParameters();
                args.P = worker.ReadMpint();
                args.Q = worker.ReadMpint();
                args.G = worker.ReadMpint();
                args.Y = worker.ReadMpint();

                _algorithm.ImportParameters(args);
            }
        }

        public override byte[] CreateKeyAndCertificatesData()
        {
            using (var worker = new SshDataWorker())
            {
                var args = _algorithm.ExportParameters(false);

                worker.Write(this.Name, Encoding.ASCII);
                worker.WriteMpint(args.P);
                worker.WriteMpint(args.Q);
                worker.WriteMpint(args.G);
                worker.WriteMpint(args.Y);

                return worker.ToByteArray();
            }
        }

        public override bool VerifyData(byte[] data, byte[] signature)
        {
            return _algorithm.VerifyData(data, signature);
        }

        public override bool VerifyHash(byte[] hash, byte[] signature)
        {
            return _algorithm.VerifyHash(hash, "SHA1", signature);
        }

        public override byte[] SignData(byte[] data)
        {
            return _algorithm.SignData(data);
        }

        public override byte[] SignHash(byte[] hash)
        {
            return _algorithm.SignHash(hash, "SHA1");
        }
    }
}

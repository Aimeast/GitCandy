using GitCandy.Ssh;
using System.Linq;

namespace GitCandy.Configuration
{
    public class HostKeyResloverAttribute : RecommendedValueResloverAttribute
    {
        public override object GetValue()
        {
            return KeyUtils.SupportedAlgorithms
                .Select(x => new HostKey { KeyType = x, KeyXml = KeyUtils.GeneratePrivateKey(x) })
                .ToList();
        }
    }
}

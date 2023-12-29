using KeyValium.Memory;

namespace KeyValium.Encryption
{
    internal interface IEncryption : IDisposable
    {
        Span<byte> Encrypt(AnyPage page);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageno"></param>
        /// <param name="cipher"></param>
        /// <returns></returns>
        void Decrypt(AnyPage page);
    }
}

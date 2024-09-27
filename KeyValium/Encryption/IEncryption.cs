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

        /// <summary>
        /// For testing purposes only
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagenumber"></param>
        void Decrypt(Span<byte> page, ulong pagenumber);  
    }
}

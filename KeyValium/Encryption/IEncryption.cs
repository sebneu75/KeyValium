using KeyValium.Memory;

namespace KeyValium.Encryption
{
    internal interface IEncryption : IDisposable
    {
        Span<byte> Encrypt(AnyPage page);

        /// <summary>
        /// the returned byte[] must be allocated on the fixed heap! [ GC.AllocateArray<byte>(PageSize, true) ]
        /// </summary>
        /// <param name="pageno"></param>
        /// <param name="cipher"></param>
        /// <returns></returns>
        void Decrypt(AnyPage page);
    }
}

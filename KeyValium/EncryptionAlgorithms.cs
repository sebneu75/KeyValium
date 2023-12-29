namespace KeyValium
{
    /// <summary>
    /// The supported encryption algorithms.
    /// </summary>
    public enum EncryptionAlgorithms
    {
        /// <summary>
        /// Pages are encrypted with AES. The IV is created by generating an 
        /// Md5 hash from mixing the pagenumber with the encryption key.
        /// </summary>
        AesMd5
    }
}

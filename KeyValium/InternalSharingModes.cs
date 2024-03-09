namespace KeyValium
{
    /// <summary>
    /// Different modes for sharing database access.
    /// </summary>
    public enum InternalSharingModes : ushort
    {
        /// <summary>
        /// The database is opened exclusively. Subsequent attempts to open the database will fail.
        /// </summary>
        Exclusive = Limits.SharingMode_Exclusive,

        /// <summary>
        /// The database is opened in shared mode. Access is managed with a lockfile. 
        /// A Mutex is used to synchronize access among multiple database instances.
        /// The lockfile contains the MachineId of the computer from which it has been created. 
        /// Subsequent attempts to open the database from a different machine will fail.
        /// </summary>
        SharedLocal = Limits.SharingMode_SharedLocal,

        /// <summary>
        /// The database is opened in shared mode. Access is managed with a lockfile.  
        /// An additional lockfile is used to synchronize access among multiple database instances.
        /// The database can be used from multiple computers on a network share.
        /// </summary>
        SharedNetwork = Limits.SharingMode_SharedNetwork
    }
}
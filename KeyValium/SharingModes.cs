namespace KeyValium
{
    /// <summary>
    /// Different modes for sharing database access.
    /// </summary>
    public enum SharingModes : ushort
    {
        /// <summary>
        /// The database is opened exclusively. Subsequent attempts to open the database will fail.
        /// </summary>
        Exclusive = 0,

        /// <summary>
        /// The database is opened in shared mode. Access is managed with a lockfile. 
        /// A Mutex is used to synchronize access among multiple database instances.
        /// The lockfile contains the MachineId of the computer from which it has been created. 
        /// Subsequent attempts to open the database from a different machine will fail.
        /// </summary>
        SharedLocal = 1,

        /// <summary>
        /// The database is opened in shared mode. Access is managed with a lockfile.  
        /// An additional lockfile is used to synchronize access among multiple database instances.
        /// The database can be used from multiple computers on a network share.
        /// </summary>
        SharedNetwork = 2
    }
}
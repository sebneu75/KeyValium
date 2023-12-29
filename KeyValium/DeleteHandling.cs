namespace KeyValium
{
    /// <summary>
    /// Different modes for handling the deletion of an entry pointed to by a cursor.
    /// </summary>
    internal enum DeleteHandling
    {
        /// <summary>
        /// The cursor is invalidated.
        /// </summary>
        Invalidate,

        /// <summary>
        /// The cursor moves to the next key.
        /// </summary>
        MoveToNext,

        /// <summary>
        /// The cursor moves to the previous key.
        /// </summary>
        MoveToPrevious
    }
}

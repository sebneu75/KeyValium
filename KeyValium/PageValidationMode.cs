using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium
{
    /// <summary>
    /// defines when validation happens and the strength of the validation
    /// </summary>
    [Flags]
    public enum PageValidationMode
    {
        /// <summary>
        /// no validation
        /// </summary>
        None = 0x0000,

        /// <summary>
        /// Validation happens after a page is read from disk
        /// </summary>
        AfterReadFromDisk = 0x0001,

        /// <summary>
        /// Validation happens before a page is written to disk
        /// </summary>
        BeforeWriteToDisk = 0x0002,

        /// <summary>
        /// Default ValidationMode
        /// </summary>
        Default = AfterReadFromDisk,

        /// <summary>
        /// all validations enabled
        /// </summary>
        All = 0xffff
    }
}

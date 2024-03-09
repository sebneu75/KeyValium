using KeyValium.Collections;
using KeyValium.Inspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Recovery
{
    internal class Verification
    {
        internal static VerificationResult VerifyDatabase(Database db)
        {
            if (db.Options.InternalSharingMode != InternalSharingModes.Exclusive)
            {
                throw new NotSupportedException("Database must be opened exclusively for verification.");
            }

            var result = new VerificationResult();

            db.RefreshMetaEntries(false);

            for (int i = 0; i < db.MetaEntries.Length; i++)
            {
                var meta = db.MetaEntries[i];

                var expected = new PageRangeList();
                expected.AddRange(0, meta.LastPage);

                var all = DbInspector.GetPageRange(db, meta.DataRootPage);
                var fp = DbInspector.GetPageRange(db, meta.FsRootPage);

                all.AddRanges(fp);

                // add range of fileheader and metapages
                all.AddRange(0, Limits.MetaPages);

                var pages = string.Join(", ", all.ToList());

                if (all.RangeCount == 0)
                {
                    result.AddError(string.Format("Database is empty."));
                }
                else if (all.RangeCount > 1)
                {
                    result.AddError(string.Format("Memory leak in Database! (Gaps): {0}", pages));
                }
                else
                {
                    var range = all.ToList().First();
                    if (range.Last != meta.LastPage)
                    {
                        result.AddError(string.Format("Memory leak in Database! LastPage={0} LastReferencedPage={1}", meta.LastPage, range.Last));

                        throw new ArgumentException();
                    }
                }
            }

            return result;
        }

    }
}

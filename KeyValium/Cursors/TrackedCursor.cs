using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Cursors
{
    internal class TrackedCursor : IDisposable
    {
        internal TrackedCursor(TreeRef treeref, Cursor cursor)
        {
            if (cursor == null)
            {
                throw new ArgumentNullException("cursor");
            }

            if (treeref != null && treeref.Cursor != cursor)
            {
                throw new KeyValiumException(ErrorCodes.InvalidCursor, "Cursor does not belong to TreeRef!");
            }

            TreeRef = treeref;
            Cursor = cursor;
        }

        internal Cursor Cursor;

        internal TreeRef TreeRef;

        public void Dispose()
        {
            if (TreeRef != null)
            {
                TreeRef.Dispose();
                TreeRef = null;
            }
            else
            {
                Cursor.Dispose();
                Cursor = null;
            }
        }
    }
}

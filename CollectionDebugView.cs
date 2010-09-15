using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    public class CollectionDebugView
    {
        private ICollection collection;

        public CollectionDebugView(ICollection collection)
        {
            this.collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public object[] Items
        {
            get
            {
                object[] array = new object[collection.Count];
                collection.CopyTo(array, 0);
                return array;
            }
        }
    }
}

using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider.Engine.Collections
{
    public class EnumerableDebugView
    {
        private IEnumerable enumerable;

        public EnumerableDebugView(IEnumerable enumerable)
        {
            this.enumerable = enumerable;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public object[] Items
        {
            get
            {
                int count = 0;
                foreach (object item in enumerable)
                {
                    count++;
                }
                object[] array = new object[count];
                int index = 0;
                foreach (object item in enumerable)
                {
                    array[index++] = item;
                }
                return array;
            }
        }
    }
}

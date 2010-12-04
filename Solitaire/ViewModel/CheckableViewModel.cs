using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider.Solitaire.ViewModel
{
    public class CheckableViewModel<T>
    {
        public CheckableViewModel(T value)
        {
            Value = value;
        }

        public T Value { get; set; }
        public string Name { get; set; }
        public bool IsChecked { get; set; }
    }
}

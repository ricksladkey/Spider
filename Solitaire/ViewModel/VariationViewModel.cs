using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spider.Engine.Core;

namespace Spider.Solitaire.ViewModel
{
    public class VariationViewModel : CheckableViewModel<Variation>
    {
        public VariationViewModel(Variation variation, bool isChecked)
            : base(variation)
        {
            Name = Value.ToString();
            IsChecked = isChecked;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spider.Engine;

namespace Spider.Solitaire.ViewModel
{
    public class VariationViewModel
    {
        public VariationViewModel(Variation variation)
        {
            Variation = variation;
            Name = Variation.ToString();
            IsChecked = Variation == Variation.Spiderette2;
        }

        public Variation Variation { get; private set; }
        public string Name { get; private set; }
        public bool IsChecked { get; set; }
    }
}

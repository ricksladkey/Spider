using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spider.Engine;

namespace Spider.Solitaire.ViewModel
{
    public class EmptySpaceViewModel : CardViewModel
    {
        public EmptySpaceViewModel()
            : base(Card.Empty)
        {
        }
    }
}

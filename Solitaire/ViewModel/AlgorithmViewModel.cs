using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spider.Engine.GamePlay;

namespace Spider.Solitaire.ViewModel
{
    public class AlgorithmViewModel : CheckableViewModel<AlgorithmType>
    {
        public AlgorithmViewModel(AlgorithmType algorithmType, bool isChecked)
            : base(algorithmType)
        {
            Name = Value.ToString();
            IsChecked = isChecked;
        }
    }
}

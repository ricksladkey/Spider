using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Spider.Solitaire.ViewModel;

namespace Spider.Solitaire.View
{
    public class CardTemplateSelector : DataTemplateSelector
    {
        public DataTemplate EmptySpaceTemplate { get; set; }
        public DataTemplate DownCardTemplate { get; set; }
        public DataTemplate UpCardTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var cardViewModel = item as CardViewModel;
            switch (cardViewModel.CardType)
            {
                case CardType.EmptySpace: return EmptySpaceTemplate;
                case CardType.Down: return DownCardTemplate;
                case CardType.Up: return UpCardTemplate;
            }
            return null;
        }
    }
}

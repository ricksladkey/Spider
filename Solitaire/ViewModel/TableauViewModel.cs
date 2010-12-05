using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spider.Engine;
using System.Collections.ObjectModel;
using Spider.GamePlay;

namespace Spider.Solitaire.ViewModel
{
    public class TableauViewModel
    {
        public TableauViewModel(SpiderViewModel model)
        {
            Model = model;

            DiscardPiles = new PileViewModel();
            Piles = new ObservableCollection<PileViewModel>();
            StockPile = new PileViewModel();
            MovePile = new PileViewModel();
        }

        public SpiderViewModel Model { get; private set; }

        public PileViewModel DiscardPiles { get; private set; }
        public ObservableCollection<PileViewModel> Piles { get; private set; }
        public PileViewModel StockPile { get; private set; }
        public PileViewModel MovePile { get; private set; }

        public CardViewModel FromCard { get; set; }
        public CardViewModel ToCard { get; set; }

        public Tableau Tableau { get { return Model.Game.Tableau; } }
        public int CheckPoint { get { return Tableau.CheckPoint; } }
        public int FirstSpace { get { return Tableau.NumberOfSpaces == 0 ? -1 : Tableau.Spaces[0]; } }

        public void Revert(int checkPoint)
        {
            Tableau.Revert(checkPoint);
        }

        public void Deal()
        {
            Tableau.Deal();
        }

        public bool TryMove()
        {
            Move move = new Move(FromCard.Column, FromCard.Row, ToCard.Column);
            return Tableau.TryMove(move);
        }

        public void Refresh()
        {
            Refresh(DiscardPiles, GetDiscardCards());

            while (Piles.Count > Tableau.NumberOfPiles)
            {
                Piles.RemoveAt(Piles.Count - 1);
            }
            while (Piles.Count < Tableau.NumberOfPiles)
            {
                Piles.Add(new PileViewModel());
            }
            for (int column = 0; column < Tableau.NumberOfPiles; column++)
            {
                Refresh(Piles[column], GetPileCards(column));
            }

            Refresh(StockPile, GetStockCards());
            Refresh(MovePile, GetMoveCards());
        }

        private void Refresh(ObservableCollection<CardViewModel> collection, IEnumerable<CardViewModel> cards)
        {
            int i = 0;
            foreach (var card in cards)
            {
                if (i == collection.Count)
                {
                    collection.Add(card);
                }
                else if (collection[i].GetType() != card.GetType() || !collection[i].Equals(card))
                {
                    collection[i] = card;
                }
                ++i;
            }
            while (i < collection.Count)
            {
                collection.RemoveAt(collection.Count - 1);
            }
        }

        private IEnumerable<CardViewModel> GetDiscardCards()
        {
            for (int i = 0; i < Tableau.DiscardPiles.Count; i++)
            {
                Pile pile = Tableau.DiscardPiles[i];
                yield return new UpCardViewModel { Card = pile[pile.Count - 1] };
            }
        }

        private IEnumerable<CardViewModel> GetPileCards(int column)
        {
            if (FromCard != null && FromCard.Column == column)
            {
                for (int row = 0; row < Tableau.DownPiles[column].Count; row++)
                {
                    Card card = Tableau.DownPiles[column][row];
                    bool isSelectable = FromCard.Row == 0 && row == Tableau.DownPiles[column].Count - 1;
                    yield return new DownCardViewModel { Card = card, Column = column, IsSelectable = isSelectable };
                }
                for (int row = 0; row < FromCard.Row; row++)
                {
                    Card card = Tableau.UpPiles[column][row];
                    yield return new UpCardViewModel { Card = card, Column = column, Row = row, IsSelectable = true };
                }
                if (Tableau.DownPiles[column].Count == 0 && FromCard.Row == 0)
                {
                    yield return new EmptySpaceViewModel { Column = column, IsSelectable = true };
                }
            }
            else
            {
                for (int row = 0; row < Tableau.DownPiles[column].Count; row++)
                {
                    Card card = Tableau.DownPiles[column][row];
                    yield return new DownCardViewModel { Card = card };
                }
                for (int row = 0; row < Tableau.UpPiles[column].Count; row++)
                {
                    Card card = Tableau.UpPiles[column][row];
                    yield return new UpCardViewModel { Card = card, Column = column, Row = row, IsSelectable = true };
                }
                if (Tableau.IsSpace(column))
                {
                    yield return new EmptySpaceViewModel { Column = column, IsSelectable = true };
                }
            }
        }

        private IEnumerable<CardViewModel> GetStockCards()
        {
            Pile stockPile = Tableau.StockPile;
            for (int i = 0; i < stockPile.Count; i += Tableau.NumberOfPiles)
            {
                bool isSelectable = i + Tableau.NumberOfPiles >= stockPile.Count;
                yield return new DownCardViewModel { Card = Card.Empty, Column = -1, Row = -1, IsSelectable = isSelectable };
            }
        }

        private IEnumerable<CardViewModel> GetMoveCards()
        {
            if (FromCard != null)
            {
                Pile fromPile = Tableau.UpPiles[FromCard.Column];
                for (int row = FromCard.Row; row < fromPile.Count; row++)
                {
                    yield return new UpCardViewModel { Card = fromPile[row] };
                }
            }
        }
    }
}

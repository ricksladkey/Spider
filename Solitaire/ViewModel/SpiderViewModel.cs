using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spider.Engine;
using Spider.GamePlay;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

namespace Spider.Solitaire.ViewModel
{
    public class SpiderViewModel : ViewModelBase
    {
        private readonly string sampleData = @"
            @2|AhAh|Ah9hJsQh3s--3h6s2s4s--6sTs6h8s|3s2sAs3s2hKs-KsQsJsTs
            9s8s7s6s5h-Jh-4h8h7h-KhQhJhKsQsJsTs9s8h7s6h5s4s3s2sAsTh9s8s7
            sAh--2h4h-Kh7h-2sAs-KsQsJsTs9s8s7s6s5s|3h5hQs5s9h4s5sAsTh4s@
        ";

        private int current;
        private List<int> checkPoints;

        public SpiderViewModel()
        {
            Variations = new Variation[]
            {
                Variation.Spider1,
                Variation.Spider2,
                Variation.Spider4,
                Variation.Spiderette1,
                Variation.Spiderette2,
                Variation.Spiderette4,
            }.Select(variation => new VariationViewModel(variation)).ToArray();

            Variation = Variation.Spider2;
            AlgorithmType = AlgorithmType.Study;

            checkPoints = new List<int>();

            NewCommand = new RelayCommand(New);
            ExitCommand = new RelayCommand(Exit);
            CopyCommand = new RelayCommand(Copy, CanCopy);
            PasteCommand = new RelayCommand(Paste, CanPaste);
            UndoCommand = new RelayCommand(Undo, CanUndo);
            RedoCommand = new RelayCommand(Redo, CanRedo);
            DealCommand = new RelayCommand(Deal, CanDeal);
            MoveCommand = new RelayCommand(Move, CanMove);
            SelectCommand = new RelayCommand<CardViewModel>(Select, CanSelect);
            SetVariationCommand = new RelayCommand<VariationViewModel>(SetVariation);

            DiscardPiles = new PileViewModel();
            Piles = new ObservableCollection<PileViewModel>();
            StockPile = new PileViewModel();
            MovePile = new PileViewModel();

            if (IsInDesignMode)
            {
                Game = new Game(sampleData, AlgorithmType);
            }
            else
            {
                Game = new Game(Variation, AlgorithmType);
            }
            ResetUndoAndRefresh();
        }

        public ICommand NewCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
        public ICommand CopyCommand { get; private set; }
        public ICommand PasteCommand { get; private set; }
        public ICommand UndoCommand { get; private set; }
        public ICommand RedoCommand { get; private set; }
        public ICommand DealCommand { get; private set; }
        public ICommand MoveCommand { get; private set; }
        public ICommand SelectCommand { get; private set; }
        public ICommand SetVariationCommand { get; private set; }

        public Game Game { get; private set; }
        public PileViewModel DiscardPiles { get; private set; }
        public ObservableCollection<PileViewModel> Piles { get; private set; }
        public PileViewModel StockPile { get; private set; }
        public PileViewModel MovePile { get; private set; }
        public CardViewModel FromCard { get; private set; }
        public CardViewModel ToCard { get; private set; }

        public Tableau Tableau { get { return Game.Tableau; } }

        public Variation Variation { get; private set; }
        public AlgorithmType AlgorithmType { get; private set; }

        public IEnumerable<VariationViewModel> Variations { get; private set; }

        /// <summary>
        /// Raised when this workspace should be removed from the UI.
        /// </summary>
        public event EventHandler RequestClose;

        void OnRequestClose()
        {
            EventHandler handler = this.RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void New()
        {
            Game = new Game(Variation, AlgorithmType);
            Game.Start();
            ResetUndoAndRefresh();
        }

        private void Exit()
        {
            OnRequestClose();
        }

        private void Copy()
        {
            Clipboard.SetData(DataFormats.Text, Game.ToAsciiString());
        }

        private bool CanCopy()
        {
            return true;
        }

        private void Paste()
        {
            var data = Clipboard.GetData(DataFormats.Text) as string;
            try
            {
                Game = new Game(data, AlgorithmType);
            }
            catch (Exception e)
            {
                Utils.WriteLine("Exception: {0}", e.Message);
            }
            ResetUndoAndRefresh();
        }

        private bool CanPaste()
        {
            return true;
        }

        private void Undo()
        {
            current--;
            Tableau.Revert(checkPoints[current]);
            Refresh();
        }

        private bool CanUndo()
        {
            return current > 0;
        }

        private void Redo()
        {
            current++;
            Tableau.Revert(checkPoints[current]);
            Refresh();
        }

        private bool CanRedo()
        {
            return current < checkPoints.Count - 1;
        }

        private void Deal()
        {
            Tableau.Deal();
            AddCheckPoint();
            Refresh();
        }

        private bool CanDeal()
        {
            return Tableau.StockPile.Count > 0;
        }

        private void Move()
        {
            if (!Game.MakeMove() && Tableau.StockPile.Count > 0)
            {
                Tableau.Deal();
            }
            AddCheckPoint();
            Refresh();
        }

        private bool CanMove()
        {
            return true;
        }

        private void Select(CardViewModel card)
        {
            if (card == null)
            {
                ResetMoveAndRefresh();
                return;
            }

            if (card.Column == -1 && card.Row == -1)
            {
                Deal();
                AddCheckPoint();
                ResetMoveAndRefresh();
                return;
            }

            if (FromCard == null)
            {
                FromCard = card;
                Refresh();
                return;
            }

            ToCard = card;
            if (FromCard.Column == ToCard.Column)
            {
                ResetMoveAndRefresh();
                return;
            }

            Move move = new Move(FromCard.Column, FromCard.Row, ToCard.Column);
            if (Tableau.MoveIsValid(move))
            {
                Tableau.Move(move);
                AddCheckPoint();
                ResetMoveAndRefresh();
                return;
            }

            ResetMoveAndRefresh();
        }

        private void SetVariation(VariationViewModel variation)
        {
            Variation = variation.Variation;
            Game = new Game(Variation, AlgorithmType);
            ResetUndoAndRefresh();
        }

        private void AddCheckPoint()
        {
            current++;
            checkPoints.RemoveRange(current, checkPoints.Count - current);
            checkPoints.Add(Tableau.CheckPoint);
        }

        private void ResetUndoAndRefresh()
        {
            current = 0;
            checkPoints.Clear();
            checkPoints.Add(Tableau.CheckPoint);
            Refresh();
        }

        private void ResetMoveAndRefresh()
        {
            FromCard = null;
            ToCard = null;
            Refresh();
        }

        private bool CanSelect(CardViewModel card)
        {
            return card != null && card.IsSelectable;
        }

        private void Refresh()
        {
            Refresh(DiscardPiles, GetDiscardCards());

            while (Piles.Count > Game.NumberOfPiles)
            {
                Piles.RemoveAt(Piles.Count - 1);
            }
            while (Piles.Count < Game.NumberOfPiles)
            {
                Piles.Add(new PileViewModel());
            }
            for (int column = 0; column < Game.NumberOfPiles; column++)
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
            for (int i = 0; i < stockPile.Count; i += Game.NumberOfPiles)
            {
                bool isSelectable = i + Game.NumberOfPiles >= stockPile.Count;
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

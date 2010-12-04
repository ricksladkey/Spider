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

        private Variation[] supportedVariations;
        private int current;
        private List<int> checkPoints;

        public SpiderViewModel()
        {
            supportedVariations = new Variation[]
            {
                Variation.Spider1,
                Variation.Spider2,
                Variation.Spider4,
                Variation.Spiderette1,
                Variation.Spiderette2,
                Variation.Spiderette4,
            };
            
            Variation = Variation.Spider2;
            AlgorithmType = AlgorithmType.Study;
            Variations = new ObservableCollection<VariationViewModel>();

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

            if (IsInDesignMode)
            {
                Game = new Game(sampleData, AlgorithmType);
            }
            else
            {
                Game = new Game(Variation, AlgorithmType);
            }

            Tableau = new TableauViewModel(this);

            RefreshVariations();
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
        public TableauViewModel Tableau { get; private set; }
        public Variation Variation { get; private set; }
        public AlgorithmType AlgorithmType { get; private set; }

        public ObservableCollection<VariationViewModel> Variations { get; private set; }

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

            if (Tableau.FromCard == null)
            {
                Tableau.FromCard = card;
                Refresh();
                return;
            }

            Tableau.ToCard = card;
            if (Tableau.FromCard.Column == Tableau.ToCard.Column)
            {
                ResetMoveAndRefresh();
                return;
            }

            if (Tableau.TryMove())
            {
                AddCheckPoint();
                ResetMoveAndRefresh();
                return;
            }

            ResetMoveAndRefresh();
        }

        private bool CanSelect(CardViewModel card)
        {
            return card != null && card.IsSelectable;
        }

        private void SetVariation(VariationViewModel variation)
        {
            Variation = variation.Variation;
            Game = new Game(Variation, AlgorithmType);
            RefreshVariations();
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
            Tableau.FromCard = null;
            Tableau.ToCard = null;
            Refresh();
        }

        private void Refresh()
        {
            Tableau.Refresh();
        }

        private void RefreshVariations()
        {
            Variations.Clear();
            foreach (Variation variation in supportedVariations)
            {
                bool isChecked = variation == Variation;
                Variations.Add(new VariationViewModel(variation, isChecked));
            }
        }

    }
}

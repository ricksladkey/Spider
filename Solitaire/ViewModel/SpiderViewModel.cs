using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spider.Engine;
using Spider.GamePlay;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Spider.Solitaire.ViewModel
{
    public class SpiderViewModel : ViewModelBase
    {
        public SpiderViewModel()
        {
            NewCommand = new RelayCommand(param => New());
            ExitCommand = new RelayCommand(param => Exit());
            DealCommand = new RelayCommand(param => Deal(), param => CanDeal());

            DiscardPiles = new PileViewModel();
            Piles = new ObservableCollection<PileViewModel>();
            StockPile = new PileViewModel();

            string data = @"
                @2|AhAs|KhTh3s5h9s-Ah-5hKsAs7sKs-Jh-7sKs8s8h-9hJh--6s3hQh-7s9s8h
                Jh-9s3hJh4s|7h6h5h4h3h2hAh-2s-7h6s5s4s-KhQsJsTs9s-2sAs-Th-Js
                Ts-2h-Kh-2s|9hTsAs9h3sQsJs5sTh4s8s3sQh9h8h2s5hQsAhTh3s4s5s2h
                8sAh7h6h6s4h4h8hQh5sQsTsAs7sKh2h6hKs8s4hQhJs6s3h6h7h@
            ";
            Game = new Game(data, AlgorithmType.Search);
            Refresh();
        }

        public ICommand NewCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
        public ICommand DealCommand { get; private set; }

        public Game Game { get; private set; }
        public PileViewModel DiscardPiles { get; private set; }
        public ObservableCollection<PileViewModel> Piles { get; private set; }
        public PileViewModel StockPile { get; private set; }

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
        }

        private void Exit()
        {
            OnRequestClose();
        }

        private void Deal()
        {
            Game.Tableau.Deal();
            Refresh();
        }

        private bool CanDeal()
        {
            return Game.Tableau.StockPile.Count > 0;
        }

        private void Refresh()
        {
            DiscardPiles.Clear();
            for (int i = 0; i < Game.Tableau.DiscardPiles.Count; i++)
            {
                Pile pile = Game.Tableau.DiscardPiles[i];
                DiscardPiles.Add(new UpCardViewModel(pile[pile.Count - 1]));
            }

            while (Piles.Count > Game.NumberOfPiles)
            {
                Piles.RemoveAt(Piles.Count - 1);
            }
            while (Piles.Count < Game.NumberOfPiles)
            {
                Piles.Add(new PileViewModel());
            }
            for (int row = 0; row < Game.NumberOfPiles; row++)
            {
                Piles[row].Clear();
                foreach (var card in Game.Tableau.DownPiles[row])
                {
                    Piles[row].Add(new DownCardViewModel(card));
                }
                foreach (var card in Game.Tableau.UpPiles[row])
                {
                    Piles[row].Add(new UpCardViewModel(card));
                }
            }

            Pile stockPile = Game.Tableau.StockPile;
            StockPile.Clear();
            for (int i = 0; i < stockPile.Count; i += Game.NumberOfPiles)
            {
                StockPile.Add(new DownCardViewModel(stockPile[i]));
            }
        }
    }
}

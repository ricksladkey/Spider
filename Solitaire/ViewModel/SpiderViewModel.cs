using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spider.Engine;
using Spider.GamePlay;
using System.Collections.ObjectModel;

namespace Spider.Solitaire.ViewModel
{
    public class SpiderViewModel
    {
        public SpiderViewModel()
        {
            AceOfHearts = new CardViewModel(new Card(Face.Ace, Suit.Hearts));
            string data = @"
                @2||KhTh3s5h9s-Ah-5hKsAs7sKs-Jh-7sKs8s8h-9hJh--6s3hQh-7s9s8h
                Jh-9s3hJh4s|7h6h5h4h3h2hAh-2s-7h6s5s4s-KhQsJsTs9s-2sAs-Th-Js
                Ts-2h-Kh-2s|9hTsAs9h3sQsJs5sTh4s8s3sQh9h8h2s5hQsAhTh3s4s5s2h
                8sAh7h6h6s4h4h8hQh5sQsTsAs7sKh2h6hKs8s4hQhJs6s3h6h7h@
            ";
            Game = new Game(data, AlgorithmType.Search);
            Pile = new ObservableCollection<CardViewModel>();
            Pile.Clear();
            foreach (var card in Game.Tableau.DownPiles[0])
            {
                Pile.Add(new CardViewModel(card));
            }
        }

        public CardViewModel AceOfHearts { get; private set; }
        public Game Game { get; private set; }
        public ObservableCollection<CardViewModel> Pile { get; private set; }
    }
}

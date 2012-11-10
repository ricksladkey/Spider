using Spider.Solitaire.ViewModel;
using System;
using System.Windows.Input;
using Spider.Engine.Core;
using Spider.Engine.GamePlay;
using Xunit;

namespace Spider.Tests
{
    /// <summary>
    ///This is a test class for SpiderViewModelTest and is intended
    ///to contain all SpiderViewModelTest Unit Tests
    ///</summary>
    public class SpiderViewModelTest
    {
        private readonly string sampleData = @"
            @2|AhAh|Ah9hJsQh3s--3h6s2s4s--6sTs6h8s|3s2sAs3s2hKs-KsQsJsTs
            9s8s7s6s5h-Jh-4h8h7h-KhQhJhKsQsJsTs9s8h7s6h5s4s3s2sAsTh9s8s7
            sAh--2h4h-Kh7h-2sAs-KsQsJsTs9s8s7s6s5s|3h5hQs5s9h4s5sAsTh4s@
        ";

        /// <summary>
        /// A test for SpiderViewModel Constructor
        /// </summary>
        [Fact]
        public void SpiderViewModelConstructorTest1()
        {
            var target = new SpiderViewModel();
            try
            {
                Assert.Equal(target.Variation.NumberOfFoundations, target.Tableau.DiscardPiles.Count);
                Assert.Equal(target.Variation.NumberOfPiles, target.Tableau.Piles.Count);
                foreach (var pile in target.Tableau.Piles)
                {
                    Assert.Equal(1, pile.Count);
                    Assert.Equal(pile[0].CardType, CardType.EmptySpace);
                }
                Assert.Equal(0, target.Tableau.StockPile.Count);
            }
            finally
            {
                target.Dispose();
            }
        }

        /// <summary>
        /// A test for SpiderViewModel Constructor
        /// </summary>
        [Fact]
        public void SpiderViewModelConstructorTest2()
        {
            var target = new SpiderViewModel(sampleData);
            try
            {
                Assert.Equal(target.Variation.NumberOfFoundations, target.Tableau.DiscardPiles.Count);
                Assert.Equal(CardType.Up, target.Tableau.DiscardPiles[1][0].CardType);
                Assert.Equal(CardType.EmptySpace, target.Tableau.DiscardPiles[2][0].CardType);
                Assert.Equal(target.Variation.NumberOfPiles, target.Tableau.Piles.Count);
                var pile = target.Tableau.Piles[0];
                Assert.Equal(11, pile.Count);
                Assert.Equal(pile[0].CardType, CardType.Down);
                Assert.Equal(1, target.Tableau.StockPile.Count);
            }
            finally
            {
                target.Dispose();
            }
        }

        /// <summary>
        /// A test for NewCommand
        /// </summary>
        [Fact]
        public void NewCommandTest()
        {
            var target = new SpiderViewModel();
            try
            {
                target.NewCommand.Execute(null);
                Assert.Equal(target.Variation.NumberOfFoundations, target.Tableau.DiscardPiles.Count);
                Assert.Equal(target.Variation.NumberOfPiles, target.Tableau.Piles.Count);
                foreach (var pile in target.Tableau.Piles)
                {
                    Assert.True(pile.Count > 1);
                    Assert.Equal(pile[0].CardType, CardType.Down);
                    Assert.Equal(pile[pile.Count - 1].CardType, CardType.Up);
                }
                Assert.True(target.Tableau.StockPile.Count > 0);
            }
            finally
            {
                target.Dispose();
            }
        }

        /// <summary>
        /// A test for SetVariationCommand
        /// </summary>
        [Fact]
        public void SetVariationCommandTest()
        {
            var originalVariation = Variation.Spider2;
            var newVariation = Variation.Spiderette4;

            var target = new SpiderViewModel();
            try
            {
                Assert.Equal(originalVariation, target.Variation);
                foreach (var variation in target.Variations)
                {
                    Assert.Equal(originalVariation == variation.Value, variation.IsChecked);
                }
                Assert.Equal(originalVariation.NumberOfPiles, target.Game.NumberOfPiles);

                target.SetVariationCommand.Execute(new VariationViewModel(newVariation, false));

                Assert.Equal(newVariation, target.Variation);
                foreach (var variation in target.Variations)
                {
                    Assert.Equal(newVariation == variation.Value, variation.IsChecked);
                }
                Assert.Equal(newVariation.NumberOfPiles, target.Game.NumberOfPiles);
            }
            finally
            {
                target.Dispose();
            }
        }

        /// <summary>
        ///A test for SetAlgorithmCommand
        ///</summary>
        [Fact]
        public void SetAlgorithmCommandTest()
        {
            var target = new SpiderViewModel();
            try
            {
                var originalAlgorithm = AlgorithmType.Search;
                var newAlgorithm = AlgorithmType.Study;

                Assert.Equal(originalAlgorithm, target.AlgorithmType);
                foreach (var variation in target.Algorithms)
                {
                    Assert.Equal(originalAlgorithm == variation.Value, variation.IsChecked);
                }

                target.SetAlgorithmCommand.Execute(new AlgorithmViewModel(newAlgorithm, false));

                Assert.Equal(newAlgorithm, target.AlgorithmType);
                foreach (var variation in target.Algorithms)
                {
                    Assert.Equal(newAlgorithm == variation.Value, variation.IsChecked);
                }
            }
            finally
            {
                target.Dispose();
            }
        }
    }
}

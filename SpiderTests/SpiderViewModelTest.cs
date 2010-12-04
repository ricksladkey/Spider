using Spider.Solitaire.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Input;
using Spider.Engine;
using Spider.GamePlay;

namespace Spider.Tests
{
    
    
    /// <summary>
    ///This is a test class for SpiderViewModelTest and is intended
    ///to contain all SpiderViewModelTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SpiderViewModelTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for SpiderViewModel Constructor
        ///</summary>
        [TestMethod()]
        public void SpiderViewModelConstructorTest()
        {
            SpiderViewModel target = new SpiderViewModel();
        }

        /// <summary>
        ///A test for SetVariationCommand
        ///</summary>
        [TestMethod()]
        public void SetVariationCommandTest()
        {
            Variation originalVariation = Variation.Spider2;
            Variation newVariation = Variation.Spiderette4;

            SpiderViewModel target = new SpiderViewModel();

            Assert.AreEqual(originalVariation, target.Variation);
            foreach (var variation in target.Variations)
            {
                Assert.AreEqual(originalVariation == variation.Value, variation.IsChecked);
            }
            Assert.AreEqual(originalVariation.NumberOfPiles, target.Game.NumberOfPiles);

            target.SetVariationCommand.Execute(new VariationViewModel(newVariation, false));

            Assert.AreEqual(newVariation, target.Variation);
            foreach (var variation in target.Variations)
            {
                Assert.AreEqual(newVariation == variation.Value, variation.IsChecked);
            }
            Assert.AreEqual(newVariation.NumberOfPiles, target.Game.NumberOfPiles);
        }

        /// <summary>
        ///A test for SetAlgorithmCommand
        ///</summary>
        [TestMethod()]
        public void SetAlgorithmCommandTest()
        {
            AlgorithmType originalAlgorithm = AlgorithmType.Study;
            AlgorithmType newAlgorithm = AlgorithmType.Search;

            SpiderViewModel target = new SpiderViewModel();

            Assert.AreEqual(originalAlgorithm, target.AlgorithmType);
            foreach (var variation in target.Algorithms)
            {
                Assert.AreEqual(originalAlgorithm == variation.Value, variation.IsChecked);
            }

            target.SetAlgorithmCommand.Execute(new AlgorithmViewModel(newAlgorithm, false));

            Assert.AreEqual(newAlgorithm, target.AlgorithmType);
            foreach (var variation in target.Algorithms)
            {
                Assert.AreEqual(newAlgorithm == variation.Value, variation.IsChecked);
            }
        }
    }
}

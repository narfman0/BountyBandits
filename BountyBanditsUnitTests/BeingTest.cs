using BountyBandits.Character;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using BountyBandits;
using BountyBandits.Animation;

namespace BountyBanditsUnitTests
{
    [TestClass()]
    public class BeingTest
    {
        private TestContext testContextInstance;

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

        [TestMethod()]
        public void attackComputeTest()
        {
            Game game = new Game();
            game.resetPhysics();
            Being attacker = new Being("test-attacker", 1, game, AnimationController.fromXML(game.Content, "pirate"), null, true, false);
            Being target = new Being("test-target", 1, game, AnimationController.fromXML(game.Content, "nerd"), null, true, false);
            attacker.setDepth(1);
            target.setDepth(1);
            float damage = attacker.attackCompute(target);
            Console.Out.WriteLine("damage=" + damage);
            Assert.IsTrue(damage > 0f);
        }
    }
}

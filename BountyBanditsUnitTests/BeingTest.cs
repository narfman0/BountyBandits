using BountyBandits.Character;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using BountyBandits;
using BountyBandits.Animation;
using System.Collections.Generic;
using BountyBandits.GameScreen;
using Microsoft.Xna.Framework;

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
        private Dictionary<String, Being> beingsMap;
        private BountyBandits.Game game;

        #region Additional test attributes
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        [TestInitialize()]
        public void Initialize()
        {
            game = new BountyBandits.Game();
            game.resetPhysics();
            beingsMap = new Dictionary<String, Being>();
            foreach (BeingTypes beingType in Enum.GetValues(typeof(BeingTypes)))
                beingsMap.Add(beingType.ToString(), new Being(beingType.ToString(), 1, AnimationController.fromXML(game.Content, beingType.ToString()), null, true, false));
        }
        
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        [TestMethod()]
        public void attackComputeTest()
        {
            foreach (PlayerTypes playerType in Enum.GetValues(typeof(PlayerTypes)))
                foreach (BeingTypes beingType in Enum.GetValues(typeof(BeingTypes)))
                    for (int level = 1; level < 99; level++)
                    {
                        Being attacker = new Being("test-" + playerType.ToString(), level, AnimationController.fromXML(game.Content, playerType.ToString()), null, true, false);
                        Being target = new Being("test-target-" + beingType.ToString(), level, AnimationController.fromXML(game.Content, beingType.ToString()), null, false, false);
                        attacker.body.Position = new Vector2(-48,0);
                        target.body.Position = new Vector2(48, 0);
                        attacker.isFacingLeft = false;
                        target.isFacingLeft = true;
                        attacker.setDepth(1);
                        target.setDepth(1);
                        float attackerDmg = attacker.attackCompute(target);
                        float targetDmg = target.attackCompute(attacker);
                        Assert.IsTrue(attackerDmg > level / 2);
                        Assert.IsTrue(3 * targetDmg < attacker.getStat(BountyBandits.Stats.StatType.Life));
                    }
        }

        [TestMethod()]
        public void getCritChanceTest()
        {
            Being attacker = new Being("test-pirate", 1, AnimationController.fromXML(game.Content, "pirate"), null, true, false);
            Being target = new Being("test-amish", 1, AnimationController.fromXML(game.Content, "amish"), null, true, false);
            float pirateCrit = attacker.getCritChance(target);
            float amishCrit = target.getCritChance(attacker);
            Assert.AreEqual(pirateCrit, .05f, .05f);
            Assert.AreEqual(amishCrit, .01f, .05f);
        }
    }
}

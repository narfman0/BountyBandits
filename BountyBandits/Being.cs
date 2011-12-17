using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using BountyBandits.Animation;
using BountyBandits.Stats;
using BountyBandits.Inventory;
using BountyBandits.Map;
using System.Xml;

namespace BountyBandits
{
    public class Being
    {
        #region Fields
        Game gameref;
        public string name;
        private StatSet myStats = new StatSet();
        private InventoryManager itemManager = new InventoryManager();
        private const int TIME_TO_CHANGE_DEPTHS = 300;
        int timeOfLastJump = 0, timeToNextHeal = 0, directionMoving = 0;
        public int currenthealth = 5, currentspecial = 5, xp = 0, level, xpOfNextLevel = 100, 
            unusedAttr = 0, timeOfLastDepthChange = 0;
        public bool isFacingLeft = false, isDead = false;
        private bool attackComputed = true;
        public Body body; private Vector2 pos; //used to draw when dead
        public Geom geom;
        public AnimationController controller; 
        private int currFrame; 
        public AnimationInfo currAnimation;
        public Input input;
        public bool isPlayer, isLocal;
        public Guid guid;

        //player specific fields
        public UnlockedManager unlocked = new UnlockedManager(); //first is difficulty, second is actual level
        public Menu menu = new Menu();
        //enemy specific fields
        public int targetPlayer = -1;
        #endregion
        public Being(string name, int level, Game gameref, AnimationController controller, 
            Input input, bool isPlayer, bool isLocal)
        {
            this.gameref = gameref;
            this.name = name;
            this.controller = controller;
            this.level = level;
            this.isPlayer = isPlayer;
            this.isLocal = isLocal;
            this.input = input;
            changeAnimation("idle");
            foreach (Stat stat in controller.statRatios.statsTable.Values)
                myStats.setStatValue(stat.getType(), stat.getValue() * level);
            guid = Guid.NewGuid();
            newLevel();
        }
        ~Being()
        {
            if(geom != null) gameref.physicsSimulator.Remove(geom);
            if(body != null) gameref.physicsSimulator.Remove(body);
        }
        public void attack(string attackName)
        {
            if (!isDead && !currAnimation.name.Contains("attack"))
            {
                changeAnimation(attackName);
                attackComputed = false;
                if(isTouchingGeom(true))
                    body.LinearVelocity /= 2f;
            }
        }
        public void changeAnimation(string name)
        {
            if (currAnimation.name != name)
            {
                currAnimation = controller.getAnimationInfo(name);
                currFrame = currAnimation.start;
            }
        }
        public void draw()
        {
            Vector2 drawPoint = Vector2.Zero;
            ++currFrame;
            if (currFrame >= currAnimation.end)
            {
                if (isDead) 
                    currFrame = currAnimation.end - 1;
                else if (currAnimation.name.Contains("attack"))
                    changeAnimation("idle");
                else
                    currFrame = currAnimation.start;
            }

            if (Environment.TickCount - timeOfLastDepthChange < TIME_TO_CHANGE_DEPTHS)
            {
                int difference = Environment.TickCount - timeOfLastDepthChange;
                float slidex = Game.DEPTH_X_OFFSET / TIME_TO_CHANGE_DEPTHS * difference;   // 0 < x < DEPTH_X_OFFSET
                float slidey = Game.DEPTH_MULTIPLE / TIME_TO_CHANGE_DEPTHS * difference;   // 0 < y < DEPTH_MULTIPLE
                slidex *= directionMoving; slidey *= directionMoving;
                if (directionMoving == -1)
                {
                    slidex += Game.DEPTH_X_OFFSET;
                    slidey -= 2 * Game.DEPTH_MULTIPLE;
                }
                else
                {
                    slidex -= Game.DEPTH_X_OFFSET;
                    slidey -= 4 * Game.DEPTH_MULTIPLE;
                }
                slidey -= ((controller.frames[currFrame].Height / 128) - 1) * 128f;
                drawPoint = new Vector2(getPos().X - (controller.frames[currFrame].Width / 2f) - slidex, getPos().Y - (controller.frames[currFrame].Height / 2) - slidey);
            }
            else
                drawPoint = new Vector2(getPos().X - (controller.frames[currFrame].Width / 2f), getPos().Y + controller.frames[currFrame].Height / 2f);

            SpriteEffects effects = isFacingLeft ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 avePos = gameref.getAvePosition();
            gameref.drawGameItem(controller.frames[currFrame], new Vector2(drawPoint.X - avePos.X + gameref.res.ScreenWidth / 2, drawPoint.Y - avePos.Y + gameref.res.ScreenHeight / 2), 0, getDepth(), Vector2.One, effects, Vector2.Zero);
        }
        public int getDepth()
        {
            if (geom.CollisionCategories == CollisionCategory.Cat1)     return 0;
            else if (geom.CollisionCategories == CollisionCategory.Cat2)return 1;
            else if (geom.CollisionCategories == CollisionCategory.Cat3)return 2;
            else if (geom.CollisionCategories == CollisionCategory.Cat4)return 3;
            else return -1;
        }
        public InventoryManager getItemManager() { return itemManager; }
        public int getStat(StatType type)
        {
            return myStats.getStat(type).getValue() + itemManager.getStatBonus(type);
        }
        public void upgradeStat(StatType type, int value)
        {
            myStats.addStatValue(type, value);
        }
		public Vector2 getPos()
		{
			if(!isDead)	return body.Position;
			else		return pos;
		}
        public void giveXP(int xp)
        {
            this.xp += xp;
            if (this.xp >= xpOfNextLevel)
            {
                //TODOjrob make some fancy effect on levelup
                myStats.setStatValue(StatType.Life, myStats.getStatValue(StatType.Life) + 1);
                myStats.setStatValue(StatType.Special, myStats.getStatValue(StatType.Special) + 1);
                xpOfNextLevel = gameref.xpManager.getXPToLevelUp(++level);
                unusedAttr += 5;
            }
        }
		public bool isTouchingGeom(bool countGround)
        {
            foreach (Geom geometry in gameref.physicsSimulator.GeomList)
                if (geom != geometry &&
                    (geom.CollisionCategories & geometry.CollisionCategories) != CollisionCategory.None &&
                    AABB.Intersect(ref geom.AABB, ref geometry.AABB))
                    if (countGround || !geometry.Equals(gameref.groundGeom)) 
                        return true;
            return false;
        }
        public void jump()
        {
            if (Environment.TickCount - timeOfLastJump > 750 && !isDead)
            {
                timeOfLastJump = Environment.TickCount;
                if (isTouchingGeom(true))
                    body.ApplyForce(new Vector2(0, 150 + 4 * getStat(StatType.Strength) + 50 * getStat(StatType.Agility)));
            }
        }
		public bool lane(bool up)
		{
			if (Environment.TickCount - timeOfLastDepthChange > TIME_TO_CHANGE_DEPTHS && !isDead &&
                ((up && geom.CollisionCategories != CollisionCategory.Cat1) ||
                    (!up && geom.CollisionCategories != CollisionCategory.Cat4)))
            {
                if (up) setCollisionCategories((CollisionCategory)((int)geom.CollisionCategories / 2));
                else setCollisionCategories((CollisionCategory)(2 * (int)geom.CollisionCategories));
                if (isTouchingGeom(false))
                    if (up) setCollisionCategories((CollisionCategory)(2 * (int)geom.CollisionCategories));
                    else setCollisionCategories((CollisionCategory)((int)geom.CollisionCategories / 2));
                else
                {
                    timeOfLastDepthChange = Environment.TickCount;
                    directionMoving = (up) ? -1 : 1;
                    return true;
                }
            }
			return false;
		}
        public void move(Vector2 force)
        {
            if (!isDead && Math.Abs(body.LinearVelocity.X) < 25 + getStat(StatType.Speed)
                && isTouchingGeom(true) && !currAnimation.name.Contains("attack"))
            {
                body.ApplyForce(new Vector2((float)getStat(StatType.Speed) * force.X, force.Y));
                isFacingLeft = (force.X > 0) ? false : true;
            }
        }
        public void newLevel()
        {
            Texture2D tex = controller.frames[currFrame];
            body = BodyFactory.Instance.CreateRectangleBody(gameref.physicsSimulator, tex.Width / 2, tex.Height, ((float)myStats.getStatValue(StatType.Strength)) / 5f);
            body.Position = new Vector2(10 + tex.Width, 10 + tex.Height / 2);
            geom = GeomFactory.Instance.CreateRectangleGeom(gameref.physicsSimulator, body, tex.Width / 2, tex.Height);
            geom.FrictionCoefficient = .1f;
            body.MomentOfInertia = float.MaxValue;
            setDepth(input==null?gameref.rand.Next(4):(int)input.getPlayerIndex());
            currenthealth = getStat(StatType.Life);
            currentspecial = getStat(StatType.Special);
            
        }
        public void setCollisionCategories(CollisionCategory newCat)
        {
            geom.CollisionCategories = newCat;
            geom.CollidesWith = newCat;
        }
        public void update(GameTime gameTime)
        {
            if (!isDead)
            {
                #region Dead
                if (currenthealth < 1)
                {
                    isDead = true;
                    changeAnimation("death1");
                    pos = body.Position;
                    gameref.physicsSimulator.Remove(body);
                    gameref.physicsSimulator.Remove(geom);
                    if (!isPlayer && gameref.rand.Next(20)==0 && !gameref.network.isClient())   //nodrop check. should query entity
                        gameref.dropItem(pos, this);
                }
                #endregion
                #region Change animation to idle/walk
                else if (!currAnimation.name.Contains("attack"))  //only change animations if done attacking
                {
                    if (Math.Abs(body.LinearVelocity.X) < 5)
                        changeAnimation("idle");
                    else if (Math.Abs(body.LinearVelocity.X) >= 5)
                        changeAnimation("walk");
                }
                #endregion
                #region Compute attack
                else if (!attackComputed && currAnimation.keyframe <= currFrame)
                {
                    attackComputed = true;
                    List<Being> enemies = gameref.spawnManager.enemies;
                    if (targetPlayer != -1) 
                        enemies = gameref.players;  //if being is an enemy
                    foreach (Being enemy in enemies)
                        if (!enemy.isDead && (enemy.geom.CollisionCategories & geom.CollisionCategories) != CollisionCategory.None)
                        {
                            Vector2 dimensions = new Vector2(controller.frames[currFrame].Width + getStat(StatType.Range), 
                                controller.frames[currFrame].Height + getStat(StatType.Range));
                            Geom collisionGeom = GeomFactory.Instance.CreateRectangleGeom(gameref.physicsSimulator, body, dimensions.X, dimensions.Y);
                            collisionGeom.CollisionCategories = geom.CollisionCategories;
                            collisionGeom.CollidesWith = geom.CollidesWith;
                            if (enemy.geom.Collide(collisionGeom))
                            {
                                int opposingRoll = 0; for (int i = 0; i < 5; ++i) opposingRoll += gameref.rand.Next(20);
                                bool criticalHit = (getStat(StatType.Agility) - enemy.getStat(StatType.Agility) + gameref.rand.Next(100) > opposingRoll) ? true : false;
                                if (currAnimation.name.Contains("attack"))
                                {
                                    float damage = (float)getStat(StatType.Agility) / 8f + (float)getStat(StatType.Strength) / 5f + (float)gameref.rand.NextDouble() - .5f;
                                    if (criticalHit)
                                        damage *= 2;
                                    if (damage > 0)
                                        enemy.currenthealth -= (int)damage;
                                    if (enemy.currenthealth <= 0 && targetPlayer == -1)
                                        foreach (Being being in gameref.players)
                                            being.giveXP(gameref.xpManager.getKillXPPerLevel(enemy.level));
                                }
                            }
                            gameref.physicsSimulator.Remove(collisionGeom);

                        }
                }
                #endregion
                #region Health regen
                if (Environment.TickCount - timeToNextHeal > 2000)
                {
                    timeToNextHeal = Environment.TickCount;
                    bool isEnemyAlive = false;
                    foreach (Being enemy in gameref.spawnManager.enemies)
                        if (enemy.currenthealth > 0)
                            isEnemyAlive = true;
                    if (gameref.spawnManager.enemies.Count < 1 && targetPlayer == -1 && !isEnemyAlive && body.LinearVelocity.LengthSquared() < 20)
                    {
                        currenthealth += getStat(StatType.Life) / 5 + getStat(StatType.Agility) / 10;
                        if (currenthealth > getStat(StatType.Life)) currenthealth = getStat(StatType.Life);
                    }
                }
                #endregion
            }
			#region Respawn
            else
            {
				bool enemiesAlive = false;
				foreach(Being enemy in gameref.spawnManager.enemies)
					if(enemy == this || enemy.currenthealth > 0)
						enemiesAlive=true;
				if(!enemiesAlive)
				{
                    currenthealth = getStat(StatType.Life) / 3;
					changeAnimation("idle");
					isDead = false;
				}
            }
			#endregion
        }
        public XmlElement asXML(XmlNode parentNode)
        {
            XmlElement beingElement = parentNode.OwnerDocument.CreateElement("being");
            beingElement.SetAttribute("name", name);
            beingElement.SetAttribute("xp", xp.ToString());
            beingElement.SetAttribute("level", level.ToString());
            beingElement.SetAttribute("unusedAttr", unusedAttr.ToString());
            beingElement.SetAttribute("xpOfNextLevel", xpOfNextLevel.ToString());
            beingElement.SetAttribute("animationControllerName", controller.name);
            beingElement.SetAttribute("guid", guid.ToString());
            beingElement.AppendChild(myStats.asXML(beingElement));
            beingElement.AppendChild(itemManager.asXML(beingElement));
            beingElement.AppendChild(unlocked.asXML(beingElement));
            return beingElement;
        }
        public static Being fromXML(XmlElement element, Game gameref)
        {
            String name = element.GetAttribute("name"),
                controllerName = element.GetAttribute("animationControllerName");
            StatSet stats = StatSet.fromXML((XmlElement)element.GetElementsByTagName("stats").Item(0));
            Being being = new Being(name, -1, gameref, gameref.animationManager.getController(controllerName), null, false, false);
            being.xp = int.Parse(element.GetAttribute("xp"));
            being.level = int.Parse(element.GetAttribute("level"));
            being.unusedAttr = int.Parse(element.GetAttribute("unusedAttr"));
            being.xpOfNextLevel = int.Parse(element.GetAttribute("xpOfNextLevel"));
            being.guid = new Guid(element.GetAttribute("guid"));
            being.unlocked = UnlockedManager.fromXML((XmlElement)element.GetElementsByTagName("levelsUnlocked").Item(0));
            foreach (Stat stat in stats.statsTable.Values)
                being.myStats.setStatValue(stat.getType(), stat.getValue());
            return being;
        }
        public void setDepth(int depth)
        {
            switch (depth)
            {
                case 0:
                    setCollisionCategories(CollisionCategory.Cat1);
                    break;
                case 1:
                    setCollisionCategories(CollisionCategory.Cat2);
                    break;
                case 2:
                    setCollisionCategories(CollisionCategory.Cat3);
                    break;
                case 3:
                    setCollisionCategories(CollisionCategory.Cat4);
                    break;
            }
        }
    }
}

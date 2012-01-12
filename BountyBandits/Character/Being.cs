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

namespace BountyBandits.Character
{
    public class Being
    {
        #region Fields
        protected Game gameref;
        public string name;
        private StatSet myStats = new StatSet();
        private InventoryManager itemManager = new InventoryManager();
        private const int TIME_TO_CHANGE_DEPTHS = 300;
        int timeOfLastJump = 0, timeToNextHeal = 0;
        public int xp = 0, level, xpOfNextLevel = 100, unusedAttr = 0, timeOfLastDepthChange = 0;
        public float currentspecial = 5;
        private float currentHealth = 5f, weight = 1f;
        public bool isFacingLeft = false, isDead = false, isMovingUp;
        private bool attackComputed = true;
        public Body body; private Vector2 pos; //used to draw when dead
        public Geom geom;
        public AnimationController controller;
        private float currFrame;
        public AnimationInfo currAnimation;
        public Input input;
        public bool isPlayer, isLocal;
        public Guid guid;
        public CombatTextManager combatText;

        //player specific fields
        public UnlockedManager unlocked = new UnlockedManager(); //first is difficulty, second is actual level
        public Menu menu = new Menu();
        #endregion
        #region Properties
        public float CurrentHealth
        {
            get { return currentHealth; }
            set
            {
                float difference = value - currentHealth;
                currentHealth = value;
                if (Game.instance.network.isServer())
                    gameref.network.sendBeingCurrentHP(guid, CurrentHealth);
            }
        }
        public float Weight {
            get { return weight + ((float)myStats.getStatValue(StatType.Strength) / 50f); }
            set { weight = value; } 
        }
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
            if (geom != null) gameref.physicsSimulator.Remove(geom);
            if (body != null) gameref.physicsSimulator.Remove(body);
        }
        public void attack(string attackName)
        {
            if (!isDead && !currAnimation.name.Contains("attack"))
            {
                changeAnimation(attackName);
                if (isTouchingGeom(true))
                    body.LinearVelocity /= 2f;
            }
        }
        public void attackCompute(Being enemy)
        {
            float toHit = .95f; //calculate ths, prob from agility;
            toHit = Math.Min(.95f, toHit);
            toHit = Math.Max(.05f, toHit);

            if ((toHit > (float)gameref.rand.NextDouble() * .1f) && !enemy.isDead &&
                (enemy.geom.CollisionCategories & geom.CollisionCategories) != CollisionCategory.None)
            {
                Vector2 dimensions = new Vector2(getRange(), controller.frames[getCurrentFrame()].Height + getStat(StatType.Range)),
                    positionOffset = new Vector2(getFacingMultiplier() * controller.frames[getCurrentFrame()].Width / 2, 0);
                Geom collisionGeom = GeomFactory.Instance.CreateRectangleGeom(gameref.physicsSimulator, body, dimensions.X, dimensions.Y, positionOffset, 0);
                collisionGeom.CollisionCategories = geom.CollisionCategories;
                collisionGeom.CollidesWith = geom.CollidesWith;

                if (enemy.geom.Collide(collisionGeom))
                {
                    int opposingRoll = 0; for (int i = 0; i < 5; ++i) opposingRoll += gameref.rand.Next(20);
                    bool criticalHit = (getStat(StatType.Agility) - enemy.getStat(StatType.Agility) + gameref.rand.Next(100) > opposingRoll) ? true : false;
                    if (currAnimation.name.Contains("attack"))
                    {
                        float damage = getDamage();
                        damage -= enemy.getStat(StatType.DamageReduction);
                        damage /= enemy.getDefense();
                        if (criticalHit)
                            damage *= 2;
                        if (damage > 0)
                        {
                            float lifeSteal = getLifeSteal();
                            if (lifeSteal > 0f)
                            {
                                CurrentHealth += damage * lifeSteal;
                                combatText.add(enemy.guid, "+" + (int)damage, CombatTextType.HealthAdded);
                            }
                            enemy.CurrentHealth -= damage;
                            if(Game.instance.network.isServer())
                                gameref.network.sendBeingCurrentHP(enemy.guid, enemy.CurrentHealth);
                            enemy.combatText.add(enemy.guid, "-" + (int)damage, CombatTextType.HealthTaken);
                        }
                        if (enemy.CurrentHealth <= 0f && isPlayer)
                            foreach (Being being in gameref.players.Values)
                                being.giveXP(gameref.xpManager.getKillXPPerLevel(enemy.level));
                        Game.instance.network.sendAddXP(Game.instance.xpManager.getKillXPPerLevel(enemy.level));
                        if (getStat(StatType.Knockback) > 0)
                            enemy.move(new Vector2(getFacingMultiplier() * getStat(StatType.Knockback), 0));
                    }
                }
                gameref.physicsSimulator.Remove(collisionGeom);
            }
        }
        public void changeAnimation(string name)
        {
            if (currAnimation.name != name)
            {
                if (isLocal || (this is Enemy && gameref.network.isServer()))
                    gameref.network.sendBeingAnimationChange(guid, name);
                currAnimation = controller.getAnimationInfo(name);
                currFrame = currAnimation.start;
                if (currAnimation.name.Contains("attack") && !gameref.network.isClient())
                    attackComputed = false;
            }
        }
        public void draw()
        {
            Vector2 drawPoint = Vector2.Zero;
            currFrame += getAttackSpeed();
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
                slidex *= getMovingUpMultiple(); slidey *= getMovingUpMultiple();
                if (isMovingUp)
                {
                    slidex += Game.DEPTH_X_OFFSET;
                    slidey -= 2 * Game.DEPTH_MULTIPLE;
                }
                else
                {
                    slidex -= Game.DEPTH_X_OFFSET;
                    slidey -= 4 * Game.DEPTH_MULTIPLE;
                }
                slidey -= ((controller.frames[getCurrentFrame()].Height / 128) - 1) * 128f;
                drawPoint = new Vector2(getPos().X - (controller.frames[getCurrentFrame()].Width / 2f) - slidex, getPos().Y - (controller.frames[getCurrentFrame()].Height / 2) - slidey);
            }
            else
                drawPoint = new Vector2(getPos().X - (controller.frames[getCurrentFrame()].Width / 2f), getPos().Y + controller.frames[getCurrentFrame()].Height / 2f);

            SpriteEffects effects = isFacingLeft ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Texture2D texture = controller.frames[getCurrentFrame()];
            Vector2 avePos = gameref.getAvePosition(),
                drawPos = new Vector2(drawPoint.X - avePos.X + gameref.res.ScreenWidth / 2, drawPoint.Y - avePos.Y + gameref.res.ScreenHeight / 2);
            combatText.draw(gameref, new Vector2(drawPos.X + texture.Width / 2, drawPos.Y), getDepth());
            gameref.drawGameItem(texture, drawPos, 0, getDepth(), Vector2.One, effects, Vector2.Zero);
        }
        public int getDepth()
        {
            if (geom.CollisionCategories == CollisionCategory.Cat1) return 0;
            else if (geom.CollisionCategories == CollisionCategory.Cat2) return 1;
            else if (geom.CollisionCategories == CollisionCategory.Cat3) return 2;
            else if (geom.CollisionCategories == CollisionCategory.Cat4) return 3;
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
            if (!isDead) return body.Position;
            else return pos;
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
                    body.ApplyForce(new Vector2(0, 250 + 4 * getStat(StatType.Strength) + 4 * getStat(StatType.Agility)));
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
                    isMovingUp = up;
                    return true;
                }
            }
            return false;
        }
        public void move(Vector2 force)
        {
            if (!isDead && Math.Abs(body.LinearVelocity.X) < 25 * getSpeedMultiplier()
                && isTouchingGeom(true) && !currAnimation.name.Contains("attack"))
            {
                body.ApplyForce(new Vector2(getSpeedMultiplier() * force.X, force.Y) * .95f);
                body.Position += (force * .1f);
                isFacingLeft = (force.X > 0) ? false : true;
            }
        }
        public void newLevel()
        {
            combatText = new CombatTextManager();
            Texture2D tex = controller.frames[getCurrentFrame()];
            body = BodyFactory.Instance.CreateRectangleBody(gameref.physicsSimulator, tex.Width / 2, tex.Height, Weight);
            body.Position = new Vector2(10 + tex.Width, 10 + tex.Height / 2);
            geom = GeomFactory.Instance.CreateRectangleGeom(gameref.physicsSimulator, body, tex.Width / 2, tex.Height);
            geom.FrictionCoefficient = .1f;
            body.MomentOfInertia = float.MaxValue;
            setDepth(input == null ? gameref.rand.Next(4) : (int)input.getPlayerIndex());
            CurrentHealth = (float)getStat(StatType.Life);
            currentspecial = getStat(StatType.Special);

        }
        public void setCollisionCategories(CollisionCategory newCat)
        {
            geom.CollisionCategories = newCat;
            geom.CollidesWith = newCat;
        }
        public virtual void update(GameTime gameTime)
        {
            if (!isDead)
            {
                #region Dead
                if (CurrentHealth <= 0f)
                {
                    CurrentHealth = 0f;
                    isDead = true;
                    changeAnimation("death1");
                    pos = body.Position;
                    gameref.physicsSimulator.Remove(body);
                    gameref.physicsSimulator.Remove(geom);
                    if (!isPlayer && gameref.rand.Next(20) == 0 && !gameref.network.isClient())   //nodrop check. should query entity
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
                    List<Being> enemies = new List<Being>();
                    if (!isPlayer)
                        enemies.AddRange(gameref.players.Values);
                    else
                        enemies.AddRange(gameref.spawnManager.enemies.Values);
                    foreach (Being enemy in enemies)
                        attackCompute(enemy);
                }
                #endregion
                #region Health regen
                if (Environment.TickCount - timeToNextHeal > 2000)
                {
                    timeToNextHeal = Environment.TickCount;
                    bool isEnemyAlive = false;
                    foreach (Being enemy in gameref.spawnManager.enemies.Values)
                        if (enemy.CurrentHealth > 0f)
                            isEnemyAlive = true;
                    if (gameref.spawnManager.enemies.Count < 1 && isPlayer && !isEnemyAlive && body.LinearVelocity.LengthSquared() < 20)
                    {
                        CurrentHealth += (float)getStat(StatType.Life) / 5f + (float)getStat(StatType.Agility) / 10f;
                        if (CurrentHealth > (float)getStat(StatType.Life))
                            CurrentHealth = (float)getStat(StatType.Life);
                    }
                }
                #endregion
            }
            #region Respawn
            else
            {
                bool enemiesAlive = false;
                foreach (Being enemy in gameref.spawnManager.enemies.Values)
                    if (enemy == this || enemy.CurrentHealth > 0f)
                        enemiesAlive = true;
                if (!enemiesAlive)
                {
                    CurrentHealth = (float)getStat(StatType.Life) / 3f;
                    changeAnimation("idle");
                    isDead = false;
                }
            }
            #endregion
            combatText.update();
        }
        public float getRange()
        {
            //range of character from character midpoint
            return controller.frames[getCurrentFrame()].Width / 2 + getStat(StatType.Range);
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
            AnimationController controller = gameref.animationManager.getController(element.GetAttribute("animationControllerName"));
            Being being = new Being(element.GetAttribute("name"), 1, gameref, controller, null, true, false);
            being.copyValues(element);
            return being;
        }
        protected void copyValues(XmlElement element)
        {
            StatSet stats = StatSet.fromXML((XmlElement)element.GetElementsByTagName("stats").Item(0));
            xp = int.Parse(element.GetAttribute("xp"));
            level = int.Parse(element.GetAttribute("level"));
            unusedAttr = int.Parse(element.GetAttribute("unusedAttr"));
            xpOfNextLevel = int.Parse(element.GetAttribute("xpOfNextLevel"));
            guid = new Guid(element.GetAttribute("guid"));
            unlocked = UnlockedManager.fromXML((XmlElement)element.GetElementsByTagName("levelsUnlocked").Item(0));
            foreach (Stat stat in stats.statsTable.Values)
                myStats.setStatValue(stat.getType(), stat.getValue());
            itemManager = InventoryManager.fromXML((XmlElement)element.GetElementsByTagName("inventory").Item(0));
        }
        public void setDepth(int depth)
        {
            setCollisionCategories(PhysicsHelper.depthToCollisionCategory(depth));
        }
        public int getCurrentFrame()
        {
            return (int)currFrame;
        }
        #region Gameplay getter functions
        public float getAttackSpeed()
        {
            return (100f + getStat(StatType.Speed) + getStat(StatType.Agility) / 3) / 100f;
        }
        public float getDefenseTotal()
        {
            return getStat(StatType.Defense) * (1f + getStat(StatType.EnhancedDefense)) + getStat(StatType.Strength) / 3f;
        }
        public float getDefense()
        {
            return (100f + getDefenseTotal()) / 100f;
        }
        public float getDamage()
        {
            float damage = getStat(StatType.Agility) / 8f + getStat(StatType.Strength) / 5f + (float)gameref.rand.NextDouble() - .5f;
            damage *= ((100f + getStat(StatType.EnhancedDamage)) / 100f);
            return damage;
        }
        public float getLifeSteal()
        {
            return (float)getStat(StatType.LifeSteal) / 100f;
        }
        public int getFacingMultiplier()
        {
            return isFacingLeft ? -1 : 1;
        }
        public float getSpeedMultiplier()
        {
            return (100f + (float)getStat(StatType.Speed)) / 100f;
        }
        private int getMovingUpMultiple()
        {
            return isMovingUp ? -1 : 1;
        }
        #endregion
    }
}

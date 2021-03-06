﻿using System;
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
        public string name;
        private StatSet stats = new StatSet();
        private InventoryManager itemManager = new InventoryManager();
        private const int TIME_TO_CHANGE_DEPTHS = 300, SETTLE_TIME = 10000;
        int timeOfLastJump = 0, timeToNextHeal = 0, timeToBeSettled;
        public int xp = 0, level, xpOfNextLevel = 100, unusedAttr = 0, timeOfLastDepthChange = 0, stunDuration = 0;
        public float currentspecial = 5;
        private float currentHealth = 5f, weight = 68f;//68kgs == 150lbs
        public bool isFacingLeft = false, isDead = false, isSettled, isMovingUp, isPlayer, isLocal;
        private bool attackComputed = true;
        public Body body; private Vector2 pos; //used to draw when dead
        public Geom geom;
        public AnimationController controller;
        private float currFrame;
        public AnimationInfo currAnimation;
        /// <summary>
        /// List to hold force frames. When the frame is hit, a force is applied.
        /// This is a mutable list, whereas the one in currAnimation is immutable.
        /// Each force will be popped off as it is applied, so the enxt ready
        /// is always at index 0
        /// </summary>
        private List<ForceFrame> currAnimationForceFrames = new List<ForceFrame>();
        public Input input;
        public Guid guid;
        public CombatTextManager combatText;

        //player specific fields
        public UnlockedManager unlocked = new UnlockedManager(); //first is difficulty, second is actual level
        public Menu menu = new Menu();
        #endregion
        #region Properties
        public bool IsStunned { get { return stunDuration > 0; } }
        public float CurrentHealth
        {
            get { return currentHealth; }
            set
            {
                float difference = value - currentHealth;
                currentHealth = value;
                if (Game.instance.network != null && Game.instance.network.isServer())
                    Game.instance.network.sendBeingCurrentHP(guid, CurrentHealth);
            }
        }
        public float Weight
        {
            get { return weight + ((float)stats.getStatValue(StatType.Strength) / 2f); }
            set { weight = value; }
        }
        #endregion
        public Being(string name, int level, AnimationController controller,
            Input input, bool isPlayer, bool isLocal)
        {
            this.name = name;
            this.controller = controller;
            this.level = level;
            this.isPlayer = isPlayer;
            this.isLocal = isLocal;
            this.input = input;
            changeAnimation("idle");
            if (isPlayer)
            {
                stats.setStatValue(StatType.Agility, 10);
                stats.setStatValue(StatType.Life, 10);
                stats.setStatValue(StatType.Magic, 10);
                stats.setStatValue(StatType.Strength, 10);
                stats.setStatValue(StatType.Speed, 10);
                foreach (Stat stat in controller.statRatios.statsTable.Values)
                    stats.addStatValue(stat.getType(), stat.getValue() * (level - 1));
            }
            else
                foreach (Stat stat in controller.statRatios.statsTable.Values)
                    stats.setStatValue(stat.getType(), stat.getValue() * level);
            guid = Guid.NewGuid();
            newLevel();
        }
        ~Being()
        {
            if (geom != null) Game.instance.physicsSimulator.Remove(geom);
            if (body != null) Game.instance.physicsSimulator.Remove(body);
        }
        public void attack(string attackName)
        {
            if (!isDead && !currAnimation.name.Contains("attack"))
            {
                changeAnimation(attackName);
                if (isTouchingGeom(true) != null && currAnimation.slowIfTouchingGeom)
                    body.LinearVelocity /= 2f;
            }
        }
        public float attackCompute(Being enemy)
        {
            float toHit = .95f, damage = 0; //TODO calculate ths, prob from agility;
            toHit = Math.Max(.05f, Math.Min(.95f, toHit));

            if ((toHit > (float)Game.instance.rand.NextDouble() * .1f) && !enemy.isDead &&
                (enemy.geom.CollisionCategories & geom.CollisionCategories) != CollisionCategory.None &&
                isInRange(enemy))
            {
                if (currAnimation.stunDuration > 0)
                {
                    enemy.stunDuration = Math.Max(currAnimation.stunDuration, enemy.stunDuration);
                    enemy.changeAnimation("idle");
                }

                damage = getDamage() * currAnimation.dmgMultiplier;
                damage -= enemy.getStat(StatType.DamageReduction);
                damage /= enemy.getDefense();

                //Check crit
                if (getCritChance(enemy) >= Game.instance.rand.NextDouble())
                    damage *= 2;

                int xpAdded = Game.instance.xpManager.getKillXPPerLevel(enemy.level);
                if (damage > 0)
                {
                    float lifeSteal = getLifeSteal();
                    if (lifeSteal > 0f)
                    {
                        CurrentHealth += damage * lifeSteal;
                        combatText.add(enemy.guid, "+" + (int)damage, CombatTextType.HealthAdded);
                    }
                    enemy.CurrentHealth -= damage;
                    if (Game.instance.network != null && Game.instance.network.isServer())
                        Game.instance.network.sendBeingCurrentHP(enemy.guid, enemy.CurrentHealth);
                    if (enemy.CurrentHealth <= 0)
                        enemy.combatText.add(enemy.guid, xpAdded.ToString() + " XP", CombatTextType.XPAdded);
                    else
                        enemy.combatText.add(enemy.guid, "-" + (int)damage, CombatTextType.HealthTaken);
                }
                if (enemy.CurrentHealth <= 0 && isPlayer && Game.instance.network != null)
                {
                    if (!Game.instance.network.isClient())
                        foreach (Being being in Game.instance.players.Values)
                            being.giveXP(xpAdded);
                    if (Game.instance.network.isServer())
                        Game.instance.network.sendAddXP(xpAdded);
                }
                if (getStat(StatType.Knockback) > 0)
                    enemy.move(new Vector2(getFacingMultiplier() * getStat(StatType.Knockback), 0));
            }
            return damage;
        }
        public bool isInRange(Being target)
        {
            Vector2 dimensions = new Vector2((currAnimation.aoe ? 2 : 1) * getRange(),
                controller.getFrameDimensions(getCurrentFrame()).Y + getStat(StatType.Range));
            Vector2 positionOffset = currAnimation.aoe ? Vector2.Zero :
                new Vector2(getFacingMultiplier() * controller.getFrameDimensions(getCurrentFrame()).X / 2, 0);
            Geom collisionGeom = GeomFactory.Instance.CreateRectangleGeom(Game.instance.physicsSimulator,
                body, dimensions.X, dimensions.Y, positionOffset, 0);
            collisionGeom.CollisionCategories = geom.CollisionCategories;
            collisionGeom.CollidesWith = geom.CollidesWith;
            bool inRange = target.geom.Collide(collisionGeom);
            Game.instance.physicsSimulator.Remove(collisionGeom);
            return inRange;
        }
        public void changeAnimation(string name)
        {
            if (currAnimation == null || currAnimation.name != name && controller.getAnimationInfo(name) != null)
            {
                if (isLocal || (this is Enemy && Game.instance.network.isServer()))
                    Game.instance.network.sendBeingAnimationChange(guid, name);
                currAnimation = controller.getAnimationInfo(name);
                currFrame = currAnimation.start;
                if (currAnimation.name.Contains("attack") && !Game.instance.network.isClient())
                    attackComputed = false;
                foreach (ForceFrame frame in currAnimation.forces)
                    currAnimationForceFrames.Add(frame.clone());
            }
        }
        public void draw(Vector2 avePosition)
        {
            Vector2 drawPoint = Vector2.Zero, frameDimensions = controller.getFrameDimensions(getCurrentFrame());
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
                drawPoint = new Vector2(getPos().X - (frameDimensions.X / 2f) - slidex, getPos().Y - (frameDimensions.Y / 2) - slidey);
            }
            else
                drawPoint = new Vector2(getPos().X - (frameDimensions.X / 2f), getPos().Y + frameDimensions.Y / 2f);

            SpriteEffects effects = isFacingLeft ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 drawPos = new Vector2(drawPoint.X - avePosition.X + Game.instance.res.ScreenWidth / 2, drawPoint.Y - avePosition.Y + Game.instance.res.ScreenHeight / 2);
            combatText.draw(new Vector2(drawPos.X + frameDimensions.X / 2, drawPos.Y), getDepth());
            Game.instance.currentState.getScreen().drawGameItem(controller.frames[getCurrentFrame()], drawPos, 0, getDepth(), Vector2.One, effects, Vector2.Zero);
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
            return stats.getStat(type).getValue() + itemManager.getStatBonus(type);
        }
        public void upgradeStat(StatType type, int value)
        {
            stats.addStatValue(type, value);
        }
        public Vector2 getPos()
        {
            if (!isSettled)
                return body.Position;
            else return pos;
        }
        public void giveXP(int xp)
        {
            this.xp += xp;
            if (this.xp >= xpOfNextLevel)
            {
                //TODOjrob make some fancy effect on levelup
                stats.setStatValue(StatType.Life, stats.getStatValue(StatType.Life) + 1);
                stats.setStatValue(StatType.Special, stats.getStatValue(StatType.Special) + 1);
                xpOfNextLevel = Game.instance.xpManager.getXPToLevelUp(++level);
                unusedAttr += 5;
            }
        }
        public Geom isTouchingGeom(bool countGround)
        {
            foreach (Geom geometry in Game.instance.physicsSimulator.GeomList)
                if (geom != geometry &&
                    (geom.CollisionCategories & geometry.CollisionCategories) != CollisionCategory.None &&
                    AABB.Intersect(ref geom.AABB, ref geometry.AABB) && geom.Collide(geometry))
                    if (countGround || !geometry.Equals(Game.instance.groundGeom))
                        return geom;
            return null;
        }
        public void jump()
        {
            if (Environment.TickCount - timeOfLastJump > 750 && !isDead)
            {
                timeOfLastJump = Environment.TickCount;
                Geom touching = isTouchingGeom(true);
                if (touching != null)
                {
                    float jumpForce = Weight * (450 + 6 * getStat(StatType.Agility));
                    //Vector2 pos = body.Position;
                    //Feature nearest = touching.GetNearestFeature(ref pos, 1);
                    Vector2 featureNormal = new Vector2(0, 1);//nearest.Normal;
                    body.ApplyForce(jumpForce * featureNormal);
                }
            }
        }
        public bool lane(bool up)
        {
            if (!currAnimation.name.Contains("attack") &&
                Environment.TickCount - timeOfLastDepthChange > TIME_TO_CHANGE_DEPTHS && !isDead &&
                ((up && getDepth() != 0) || (!up && getDepth() != 3)))
            {
                setDepth(getDepth() + (up ? -1 : 1));
                if (isTouchingGeom(false) != null)
                    setDepth(getDepth() + (up ? 1 : -1));
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
            if (!isDead && Math.Abs(body.LinearVelocity.X) < 20 * getSpeedMultiplier()
                && isTouchingGeom(true) != null && !currAnimation.name.Contains("attack"))
            {
                body.ApplyForce(new Vector2(getSpeedMultiplier() * force.X, force.Y) * .95f);
                body.Position += (force / Game.FORCE_AMOUNT);
                isFacingLeft = (force.X > 0) ? false : true;
            }
        }
        public void newLevel()
        {
            menu.setMenuActive(false);
            menu.setMenuItem(0);
            combatText = new CombatTextManager();
            Vector2 texDimensions = controller.frames.Count == 0 ? new Vector2(128, 128) :
                controller.getFrameDimensions(getCurrentFrame());
            body = BodyFactory.Instance.CreateRectangleBody(Game.instance.physicsSimulator, texDimensions.X / 2, texDimensions.Y, Weight);
            body.Position = new Vector2(10 + texDimensions.X, 10 + texDimensions.Y / 2);
            geom = GeomFactory.Instance.CreateRectangleGeom(Game.instance.physicsSimulator, body, texDimensions.X / 2, texDimensions.Y);
            geom.FrictionCoefficient = .1f;
            geom.OnCollision += geomOnCollision;
            body.MomentOfInertia = float.MaxValue;
            setDepth(input == null ? Game.instance.rand.Next(4) : (int)input.getPlayerIndex());
            CurrentHealth = (float)getStat(StatType.Life);
            currentspecial = getStat(StatType.Special);
        }
        /// <summary>
        /// Answer partially derived from
        /// http://stackoverflow.com/questions/3308012/oncollision-event-handler-problems-in-c-sharp-xna-with-farseer-physics
        /// </summary>
        private bool geomOnCollision(Geom me, Geom hit, ContactList contactList)
        {
            float hitVelocity = 0f;
            foreach (Contact contact in contactList)
            {
                Vector2 position = contact.Position;
                Vector2 v0, v1 = Vector2.Zero;
                me.Body.GetVelocityAtWorldPoint(ref position, out v0);
                if (!hit.Body.IsStatic)
                    hit.Body.GetVelocityAtWorldPoint(ref position, out v1);
                v0 -= v1;
                hitVelocity = Math.Max(v0.Length(), hitVelocity);
            }
            //better force calculation, still naive because it doesn't account for multiple
            //geometries in the game world. better than before, however.
            float force = hitVelocity * Math.Min(me.Body.Mass, hit.Body.Mass);
            #region Apply tag multipliers
            if (hit.Body.Tag != null)
            {
                Int16 currentTag = (Int16)hit.Body.Tag;
                force *= currentTag;
                hit.Body.Tag = (Int16)(currentTag / 10);
                if (currentTag <= 1)
                    hit.Body.Tag = null;
            }
            #endregion
            int dmg = (int)getDamageFromForce(force);
            Log.write(LogType.Debug, "force: " + force + " dmg: " + dmg);
            if (dmg > 0)
            {
                CurrentHealth -= dmg;
                combatText.add(guid, "-" + dmg, CombatTextType.HealthTaken);
            }
            return true;
        }
        public static float getDamageFromForce(float force)
        {
            return 2.74737f * (float)Math.Log(force, Math.E) - 23f;
        }
        public virtual void update(GameTime gameTime)
        {
            if (!isDead)
            {
                #region Animation forces
                while (currAnimationForceFrames.Count > 0 && currAnimationForceFrames[0].frame <= currFrame)
                {
                    ForceFrame forceFrame = currAnimationForceFrames[0];
                    if (forceFrame.isEnemy)
                    {
                        foreach (Being enemy in Game.instance.spawnManager.enemies.Values)
                            if (isInRange(enemy))
                            {
                                abilityForce(enemy);
                                enemy.body.Tag = forceFrame.tag;
                            }
                    }
                    else if (forceFrame.isSelf)
                    {
                        abilityForce(this);
                        body.Tag = forceFrame.tag;
                    }
                    else
                    {
                        Texture2D tex = controller.frames[getCurrentFrame()];
                        float x = body.Position.X + (tex.Width * (isFacingLeft ? -.5f : .5f));
                        float y = body.Position.Y - tex.Height * .25f;
                        List<Geom> collide = Game.instance.physicsSimulator.CollideAll(x, y);
                        foreach (Geom geom in collide)
                        {
                            if (geom.CollidesWith == this.geom.CollidesWith)
                            {
                                Vector2 force = forceFrame.force;
                                force += 300 * new Vector2(getStat(StatType.Strength), getStat(StatType.Strength));
                                force.X *= isFacingLeft ? -1 : 1;
                                geom.Body.ApplyForceAtWorldPoint(force, new Vector2(x, y));
                                geom.Body.Tag = forceFrame.tag;
                            }
                        }
                    }
                    currAnimationForceFrames.RemoveAt(0);
                }
                #endregion
                #region Stunned
                stunDuration -= gameTime.ElapsedGameTime.Milliseconds;
                stunDuration = Math.Max(0, stunDuration);
                #endregion
                #region Dead
                if (CurrentHealth <= 0f)
                {
                    CurrentHealth = 0f;
                    isDead = true;
                    changeAnimation("death1");
                    if (!isPlayer && Game.instance.rand.Next(20) == 0 && !Game.instance.network.isClient())   //nodrop check. should query entity
                        Game.instance.dropItem(pos, this);
                    timeToBeSettled = (int)gameTime.TotalGameTime.TotalMilliseconds + SETTLE_TIME;
                    isSettled = false;
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
                    if (currAnimation.teleport != null)
                        body.Position += currAnimation.teleport * getFacingMultiplier();
                    attackComputed = true;
                    if (!IsStunned)
                    {
                        if (currAnimation.name.Contains("Ranged") && !Game.instance.network.isClient())
                            attackRanged();
                        else
                        {
                            List<Being> enemies = new List<Being>();
                            if (!isPlayer)
                                enemies.AddRange(Game.instance.players.Values);
                            else
                                enemies.AddRange(Game.instance.spawnManager.enemies.Values);
                            foreach (Being enemy in enemies)
                                attackCompute(enemy);
                        }
                    }
                }
                #endregion
                #region Health regen
                if (Environment.TickCount - timeToNextHeal > 2000 && !isDead)
                {
                    timeToNextHeal = Environment.TickCount;
                    bool isEnemyAlive = false;
                    foreach (Being enemy in Game.instance.spawnManager.enemies.Values)
                        if (enemy.CurrentHealth > 0f)
                            isEnemyAlive = true;
                    if (Game.instance.spawnManager.enemies.Count < 1 && isPlayer && !isEnemyAlive && body.LinearVelocity.LengthSquared() < 20)
                    {
                        CurrentHealth += (float)getStat(StatType.Life) / 25f + (float)getStat(StatType.Agility) / 40f;
                        if (CurrentHealth > (float)getStat(StatType.Life))
                            CurrentHealth = (float)getStat(StatType.Life);
                    }
                }
                #endregion
            }
            else
            {
                #region settle
                if (!isSettled && (int)gameTime.TotalGameTime.TotalMilliseconds > timeToBeSettled)
                {
                    isSettled = true;
                    pos = body.Position;
                    Game.instance.physicsSimulator.Remove(body);
                    Game.instance.physicsSimulator.Remove(geom);
                }
                #endregion
                #region Respawn
                bool enemiesAlive = false;
                foreach (Being enemy in Game.instance.spawnManager.enemies.Values)
                    if (enemy == this || enemy.CurrentHealth > 0f)
                        enemiesAlive = true;
                if (!enemiesAlive)
                {
                    newLevel();
                    CurrentHealth = (float)getStat(StatType.Life) / 3f;
                    changeAnimation("idle");
                    isDead = false;
                }
                #endregion
            }
            combatText.update();
        }
        private void attackRanged()
        {
            if (currAnimation.projectileTexture != null)
            {
                Texture2D projectileTexture = Game.instance.texMan.getTex(currAnimation.projectileTexture);
                Geom projectileGeom = PhysicsHelper.textureToGeom(Game.instance.physicsSimulator, projectileTexture, currAnimation.projectileWeight);
                projectileGeom.Body.Position = getPos() + getFacingMultiplier() * new Vector2(controller.frames[(int)currFrame].Width / 2, 0);
                projectileGeom.Body.ApplyForce(new Vector2(getFacingMultiplier() * (9000 + 2000 * getStat(StatType.Agility)) / currAnimation.projectileWeight, 250 * currAnimation.projectileWeight));
                projectileGeom.Body.Rotation = getFacingMultiplier() * 1.57079633f;
                projectileGeom.CollisionCategories = (CollisionCategory)PhysicsHelper.depthToCollisionCategory(getDepth());
                projectileGeom.CollidesWith = geom.CollisionCategories;
                projectileGeom.Body.Tag = (Int16)25;
                GameItem item = new GameItem();
                item.body = projectileGeom.Body;
                item.loc = projectileGeom.Body.Position;
                item.width = 1;
                item.startdepth = (uint)getDepth();
                item.rotation = projectileGeom.Body.Rotation;
                item.name = currAnimation.projectileTexture;
                item.polygonType = PhysicsPolygonType.Polygon;
                item.geom = projectileGeom;
                Game.instance.activeItems.Add(item.guid, item);
                Game.instance.network.sendFullObjectsUpdate();
            }
        }
        public float getRange()
        {
            //range of character from character midpoint
            return controller.getFrameDimensions(getCurrentFrame()).X / 2 + getStat(StatType.Range);
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
            beingElement.AppendChild(stats.asXML(beingElement));
            beingElement.AppendChild(itemManager.asXML(beingElement));
            beingElement.AppendChild(unlocked.asXML(beingElement));
            return beingElement;
        }
        public static Being fromXML(XmlElement element)
        {
            AnimationController controller = Game.instance.animationManager.getController(element.GetAttribute("animationControllerName"));
            Being being = new Being(element.GetAttribute("name"), 1, controller, null, true, false);
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
                this.stats.setStatValue(stat.getType(), stat.getValue());
            itemManager = InventoryManager.fromXML((XmlElement)element.GetElementsByTagName("inventory").Item(0));
        }
        public void setDepth(int depth)
        {
            CollisionCategory newCat = PhysicsHelper.depthToCollisionCategory(depth);
            geom.CollisionCategories = newCat;
            geom.CollidesWith = newCat;
        }
        public int getCurrentFrame()
        {
            return (int)currFrame;
        }
        private void abilityForce(Being being)
        {
            Vector2 force = currAnimationForceFrames[0].force;
            force.X *= isFacingLeft ? -1 : 1;
            being.body.ApplyForce(force * body.Mass);
        }
        #region Gameplay getter functions
        public float getCritChance(Being enemy)
        {
            return ((getStat(StatType.Agility) * .5f) / level - (enemy.getStat(StatType.Agility) * .1f) / enemy.level) / 100f;
        }
        public float getAttackSpeed()
        {
            return (100f + getStat(StatType.Speed) + getStat(StatType.Agility) / 3) / 150f;
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
            float damage = getStat(StatType.Agility) / 8f + getStat(StatType.Strength) / 5f + (float)Game.instance.rand.NextDouble() - .5f;
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

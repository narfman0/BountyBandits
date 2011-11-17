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

namespace BountyBandits
{
    public class Being
    {
        #region Fields
        Game gameref;
        public string name;
        public BountyBandits.Stats.Stats myStats = new BountyBandits.Stats.Stats();
        private BountyBandits.Inventory.Manager itemManager = new BountyBandits.Inventory.Manager();
        public int currenthealth = 5, maxspecial = 5, currentspecial = 5;
        int timeOfLastDepthChange = 0, timeOfLastJump = 0, timeToChangeDepths = 300, timeToNextHeal = 0, framesUntilReady = 0, directionMoving = 0;
        public int xp = 0, level = 1, xpOfNextLevel = 100, unusedAttr = 0;
        bool isFacingLeft = false, attackComputed = true;
        public bool isDead = false;
        public Body body;	Vector2 pos; //used to draw when dead
        public Geom geom;
        public BountyBandits.Animation.Controller controller; int currFrame; public BountyBandits.Animation.AnimationInfo currAnimation;
        public KeyboardState prevKeyboardState = new KeyboardState(); public GamePadState prevGamePadState = new GamePadState();

        //player specific fields
        public Vector2 unlocked = Vector2.Zero; //first is difficulty, second is actual level
        public PlayerIndex controllerIndex;
        public Menu menu = new Menu();
        //enemy specific fields
        public int targetPlayer = -1;
        #endregion
        public Being(string name, int maxhealth, Game gameref, BountyBandits.Animation.Controller controller)
        {
            this.gameref = gameref;
            this.name = name;
            this.currenthealth = maxhealth;
            this.controller = controller;
            myStats.setStatValue(BountyBandits.Stats.Type.Strength, 5);
            myStats.setStatValue(BountyBandits.Stats.Type.Speed, 5);
            myStats.setStatValue(BountyBandits.Stats.Type.Agility, 5);
            myStats.setStatValue(BountyBandits.Stats.Type.Magic, 5);
            myStats.setStatValue(BountyBandits.Stats.Type.Life, maxhealth);

            newLevel();
        }
        ~Being()
        {
            if(geom != null) gameref.physicsSimulator.Remove(geom);
            if(body != null) gameref.physicsSimulator.Remove(body);
        }
        public void attack(string attackName)
        {
            if (framesUntilReady.Equals(0) && !isDead)
            {
                changeAnimation(attackName);
                framesUntilReady = currAnimation.end - currAnimation.start;
                attackComputed = false;
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
        public void draw(int currentDepth)
        {
            Vector2 drawPoint = Vector2.Zero;
            ++currFrame;
            if (currFrame >= currAnimation.end)
            {
                if (isDead) currFrame = currAnimation.end - 1;
                else currFrame = currAnimation.start;
            }

            if (Environment.TickCount - timeOfLastDepthChange < timeToChangeDepths)
            {
                int difference = Environment.TickCount - timeOfLastDepthChange;
                float slidex = Game.DEPTH_X_OFFSET / timeToChangeDepths * difference;   // 0 < x < DEPTH_X_OFFSET
                float slidey = Game.DEPTH_MULTIPLE / timeToChangeDepths * difference;   // 0 < y < DEPTH_MULTIPLE
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
                drawPoint = new Vector2(getPos().X - (controller.frames[currFrame].Width / 2f) - slidex, getPos().Y - (controller.frames[currFrame].Height / 2) - slidey);
            }
            else
                drawPoint = new Vector2(getPos().X - (controller.frames[currFrame].Width / 2f), getPos().Y + controller.frames[currFrame].Height / 2f);

            SpriteEffects effects = isFacingLeft ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            gameref.drawItem(controller.frames[currFrame], new Vector2(drawPoint.X - gameref.getAveX() + gameref.res.ScreenWidth / 2, drawPoint.Y), 0, currentDepth, Vector2.One, effects, Vector2.Zero);
        }
        public int getDepth()
        {
            if (geom.CollisionCategories == CollisionCategory.Cat1)     return 0;
            else if (geom.CollisionCategories == CollisionCategory.Cat2)return 1;
            else if (geom.CollisionCategories == CollisionCategory.Cat3)return 2;
            else if (geom.CollisionCategories == CollisionCategory.Cat4)return 3;
            else return -1;
        }
        public BountyBandits.Inventory.Manager getItemManager() { return itemManager; }
        public int getStat(BountyBandits.Stats.Type type)
        {
            return myStats.getStat(type).getValue() + itemManager.getStatBonus(type);
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
                myStats.setStatValue(BountyBandits.Stats.Type.Life, myStats.getStatValue(BountyBandits.Stats.Type.Life) + 1);
                maxspecial += 1;
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
                {
                    if (countGround)    return true;
                    else if (!geometry.Equals(gameref.groundGeom))
                            return true;
                }
            return false;
        }
        public void jump()
        {
            if (Environment.TickCount - timeOfLastJump > 750 && !isDead)
            {
                timeOfLastJump = Environment.TickCount;
                if (isTouchingGeom(true))
                    body.ApplyForce(new Vector2(0, 150 + 4 * getStat(BountyBandits.Stats.Type.Strength) + 50 * getStat(BountyBandits.Stats.Type.Agility)));
            }
        }
		public bool lane(bool up)
		{
			if (Environment.TickCount - timeOfLastDepthChange > timeToChangeDepths && !isDead &&
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
            if (!isDead && Math.Abs(body.LinearVelocity.X) < 25 + getStat(BountyBandits.Stats.Type.Speed) && isTouchingGeom(true))
            {
                body.ApplyForce(new Vector2((float)getStat(BountyBandits.Stats.Type.Speed) * force.X, force.Y));
                isFacingLeft = (force.X > 0) ? false : true;
            }
        }
        public void newLevel()
        {
            Texture2D tex = controller.frames[currFrame];
            body = BodyFactory.Instance.CreateRectangleBody(gameref.physicsSimulator, tex.Width / 3, tex.Height, myStats.getStatValue(BountyBandits.Stats.Type.Strength) / 5f);
            body.Position = new Vector2(10 + tex.Width / 2, 10 + tex.Height / 2);
            geom = GeomFactory.Instance.CreateRectangleGeom(gameref.physicsSimulator, body, tex.Width / 3, tex.Height);
            geom.FrictionCoefficient = .1f;
            body.MomentOfInertia = float.MaxValue;
            setCollisionCategories(CollisionCategory.Cat1);
            currenthealth = getStat(BountyBandits.Stats.Type.Life);
            currentspecial = maxspecial;
        }
        public void setCollisionCategories(CollisionCategory newCat)
        {
            geom.CollisionCategories = newCat;
            geom.CollidesWith = newCat;
        }
        public void update(GameTime gameTime)
        {
            if(framesUntilReady > 0) --framesUntilReady;
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
                }
                #endregion
                #region Change animation to idle/walk
                else if (framesUntilReady == 0)  //only change animations if done attacking
                {
                    if (Math.Abs(body.LinearVelocity.X) < 5)
                        changeAnimation("idle");
                    else if (Math.Abs(body.LinearVelocity.X) >= 5)
                        changeAnimation("walk");
                }
                #endregion
                #region Compute attack
                else if (!attackComputed)
                {
                    #region get target(s)
                    Being target = null;
                    List<Being> enemies = gameref.spawnManager.enemies;
                    if (targetPlayer != -1) enemies = gameref.players;  //if being is an enemy
                    foreach (Being enemy in enemies)
                        if ((!enemy.isDead &&
                            (enemy.geom.CollisionCategories & geom.CollisionCategories) != CollisionCategory.None) &&
                            enemy.geom.Collide(geom))
                            target = enemy;
                    #endregion
                    #region attack target
                    if (target != null)
                    {
                        int opposingRoll = 0; for (int i = 0; i < 5; ++i) opposingRoll += gameref.rand.Next(20);
                        bool criticalHit = (getStat(BountyBandits.Stats.Type.Agility) - target.getStat(BountyBandits.Stats.Type.Agility) + gameref.rand.Next(100) > opposingRoll) ? true : false;
                        if (currAnimation.name.Equals("attack1") &&
                            (framesUntilReady == (currAnimation.end - currAnimation.start) / 2))
                        {
                            attackComputed = true;
                            float damage = (float)getStat(BountyBandits.Stats.Type.Agility) / 8f + (float)getStat(BountyBandits.Stats.Type.Strength) / 5f + (float)gameref.rand.NextDouble() - .5f;
                            if (criticalHit) damage *= 2;

                            if(damage>0)
                                target.currenthealth -= (int)damage;
                            if (target.currenthealth <= 0 && targetPlayer == -1)
                                foreach (Being being in gameref.players)
                                    being.giveXP(gameref.xpManager.getKillXPPerLevel(target.level));
                        }
                    }
                    #endregion
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
                        currenthealth += getStat(BountyBandits.Stats.Type.Life) / 5 + getStat(BountyBandits.Stats.Type.Agility) / 10;
                        if (currenthealth > getStat(BountyBandits.Stats.Type.Life)) currenthealth = getStat(BountyBandits.Stats.Type.Life);
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
                    currenthealth = getStat(BountyBandits.Stats.Type.Life) / 3;
					changeAnimation("idle");
					isDead = false;
				}
            }
			#endregion
        }
    }
}

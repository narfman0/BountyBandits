using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerGames.FarseerPhysics;
using BountyBandits.Stats;

namespace BountyBandits
{
    public class SpawnManager
    {
        List<SpawnPoint> spawnPoints;
        Game gameref;
        public List<Being> enemies = new List<Being>();
        public SpawnManager(Game gameref)
        {
            this.gameref = gameref;
        }
        public void newLevel(Level newLvl)
        {
            enemies.Clear();
            spawnPoints = new List<SpawnPoint>();
            foreach (SpawnPoint point in newLvl.spawns)
                spawnPoints.Add(point.Clone());
        }
        public void spawnGroup(string type, uint level, uint amount)
        {
            for (int i = 0; i < amount; ++i)
            {
                Being enemy = new Being(type, (int)level, gameref, 
                    gameref.animationManager.getController(type));
                enemy.currenthealth = enemy.getStat(StatType.Life);
                enemy.currentspecial = enemy.maxspecial;

                int side = (gameref.rand.Next(2) == 0) ? -1 : 1;
                if (gameref.getAveX() - gameref.res.ScreenWidth / 2 < 16) 
                    side = 1;
                else if (gameref.mapManager.getCurrentLevel().background.Width - gameref.getAveX() < 128 + gameref.res.ScreenWidth) 
                    side = -1;
                int lanesToMoveDown = gameref.rand.Next(4);
                for (int laneDown = 0; laneDown < lanesToMoveDown; laneDown++)
                    enemy.setCollisionCategories((CollisionCategory)(2 * (int)enemy.geom.CollisionCategories));

                Vector2 posOffset = new Vector2(side*(gameref.res.ScreenWidth / 2 + enemy.controller.frames[0].Width+18), enemy.controller.frames[0].Height+1);
                while (gameref.physicsSimulator.Collide(new Vector2(gameref.getAveX() + posOffset.X, posOffset.Y)) != null)
                    posOffset.X += (float)(side * enemy.controller.frames[0].Width);
                enemy.body.Position = new Vector2(gameref.getAveX() + posOffset.X, posOffset.Y);
                enemies.Add(enemy);
            }
        }
        public void update(GameTime gameTime)
        {
            #region Activate spawns
            foreach (SpawnPoint spawnp in spawnPoints)
                if (spawnp.loc.X < gameref.getAveX() && !spawnp.isSpawned)
                {
                    spawnGroup(spawnp.type, spawnp.weight/*level*/, spawnp.count);
                    spawnp.isSpawned = true;
                }
            #endregion
            #region AI
            foreach (Being enemy in enemies)
            {
                enemy.update(gameTime);
                //BEGIN AI TODOjrob
                if (enemy.targetPlayer == -1 || gameref.players[enemy.targetPlayer].currenthealth < 1)
                    enemy.targetPlayer = gameref.rand.Next(gameref.players.Count-1);
                Being targetPlayer = gameref.players[enemy.targetPlayer];
                if (targetPlayer.getDepth() < enemy.getDepth())         enemy.lane(true);
                else if (targetPlayer.getDepth() > enemy.getDepth())    enemy.lane(false);
                if (enemy.isTouchingGeom(false) &&
                    enemy.body.LinearVelocity.X < .01f &&
                    enemy.getPos().Y + 10 < targetPlayer.getPos().Y)
                    enemy.jump();
                if (Math.Abs(targetPlayer.getPos().X - enemy.getPos().X) >
                    targetPlayer.controller.frames[0].Width / 3 + enemy.controller.frames[0].Width / 3)
                {
                    if (targetPlayer.getPos().X < enemy.getPos().X)
                        enemy.move(new Vector2(-Game.FORCE_AMOUNT, 0));
                    else
                        enemy.move(new Vector2(Game.FORCE_AMOUNT, 0));
                }
                if(Vector2.Distance(targetPlayer.getPos(), enemy.getPos()) < enemy.getStat(StatType.Range))
                    enemy.attack("attack1");
            }
            #endregion
            #region Kill current remnants (only first 10, so hopefully they are off the map already)
            if (enemies.Count > 25)
            {
                List<Being> toKill = new List<Being>();
                foreach (Being enemy in enemies)
                    if (enemy.isDead && toKill.Count < 10)
                        toKill.Add(enemy);
                foreach (Being deadenemy in toKill)
                    enemies.Remove(deadenemy);
            }
            #endregion
        }
    }
}

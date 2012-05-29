using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerGames.FarseerPhysics;
using BountyBandits.Stats;
using BountyBandits.Map;
using BountyBandits.Character;
using BountyBandits.Animation;

namespace BountyBandits
{
    public class SpawnManager
    {
        private List<SpawnPoint> spawnPoints;
        public Dictionary<Guid, Enemy> enemies = new Dictionary<Guid, Enemy>();
        
        public void newLevel(Level newLvl)
        {
            enemies.Clear();
            spawnPoints = new List<SpawnPoint>();
            if (!Game.instance.network.isClient())
                foreach (SpawnPoint point in newLvl.spawns)
                    spawnPoints.Add(point.Clone());
        }

        public void spawnGroup(string type, uint level, uint amount)
        {
            spawnGroup(type, level, amount, getRandomLocation(Game.instance.animationManager.getController(type)));
        }

        public void spawnGroup(string type, uint level, uint amount, Vector2 pos)
        {
            for (int i = 0; i < amount; ++i)
            {
                Enemy enemy = new Enemy(type, (int)level,
                    Game.instance.animationManager.getController(type));
                int side = getSide(enemy.controller);
                enemy.CurrentHealth = (float)enemy.getStat(StatType.Life);
                enemy.currentspecial = (float)enemy.getStat(StatType.Special);
                enemy.setDepth(Game.instance.rand.Next(4));
                enemy.body.Position = pos;
                while (enemy.isTouchingGeom(false) != null)
                    enemy.body.Position = new Vector2(enemy.body.Position.X + side * enemy.controller.frames[0].Width, enemy.body.Position.Y);
                enemies.Add(enemy.guid, enemy);
                Game.instance.network.sendNewEnemy(enemy);
            }
        }

        private int getSide(AnimationController controller)
        {
            int side = (Game.instance.rand.Next(2) == 0) ? -1 : 1;
            if (Game.instance.getAvePosition().X - Game.instance.res.ScreenWidth / 2 < 16)
                side = 1;
            else if (Game.instance.mapManager.getCurrentLevel().levelLength - Game.instance.getAvePosition().X < controller.frames[0].Width + Game.instance.res.ScreenWidth)
                side = -1;
            return side;
        }

        private Vector2 getRandomLocation(AnimationController controller)
        {
            Vector2 posOffset = new Vector2(getSide(controller) * (Game.instance.res.ScreenWidth / 2 + controller.frames[0].Width + 18), 
                controller.frames[0].Height + 1),
                       avePosition = Game.instance.getAvePosition();
            return new Vector2(avePosition.X + posOffset.X, avePosition.Y - Game.instance.res.ScreenHeight / 2 + posOffset.Y);
        }

        public void update(GameTime gameTime)
        {
            #region Activate spawns
            Vector2 avePosition = Game.instance.getAvePosition();
            foreach (SpawnPoint spawnp in spawnPoints)
                if (spawnp.insideTrigger(new Vector2(avePosition.X - Game.instance.res.ScreenWidth/2, avePosition.Y - Game.instance.res.ScreenHeight / 2)) && !spawnp.isSpawned)
                {
                    spawnGroup(spawnp.type, spawnp.weight/*level*/, spawnp.count, spawnp.loc);
                    spawnp.isSpawned = true;
                }
            #endregion
            updateEnemies(gameTime);
            #region Kill current remnants (only first 10, so hopefully they are off the map already)
            if (enemies.Count > 25)
            {
                List<Enemy> toKill = new List<Enemy>();
                foreach (Enemy enemy in enemies.Values)
                    if (enemy.isDead && toKill.Count < 10)
                        toKill.Add(enemy);
                foreach (Enemy deadenemy in toKill)
                    enemies.Remove(deadenemy.guid);
            }
            #endregion
        }

        public void updateEnemies(GameTime gameTime)
        {
            foreach (Being enemy in enemies.Values)
                enemy.update(gameTime);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerGames.FarseerPhysics;
using BountyBandits.Stats;
using BountyBandits.Map;
using BountyBandits.Character;

namespace BountyBandits
{
    public class SpawnManager
    {
        private List<SpawnPoint> spawnPoints;
        private Game gameref;
        public Dictionary<Guid, Enemy> enemies = new Dictionary<Guid, Enemy>();
        public SpawnManager(Game gameref)
        {
            this.gameref = gameref;
        }
        public void newLevel(Level newLvl)
        {
            enemies.Clear();
            spawnPoints = new List<SpawnPoint>();
            if(!gameref.network.isClient())
                foreach (SpawnPoint point in newLvl.spawns)
                    spawnPoints.Add(point.Clone());
        }
        public void spawnGroup(string type, uint level, uint amount)
        {
            for (int i = 0; i < amount; ++i)
            {
                Enemy enemy = new Enemy(type, (int)level, gameref, 
                    gameref.animationManager.getController(type));
                enemy.CurrentHealth = (float)enemy.getStat(StatType.Life);
                enemy.currentspecial = (float)enemy.getStat(StatType.Special);

                int side = (gameref.rand.Next(2) == 0) ? -1 : 1;
                if (gameref.getAvePosition().X - gameref.res.ScreenWidth / 2 < 16) 
                    side = 1;
                else if (gameref.mapManager.getCurrentLevel().levelLength - gameref.getAvePosition().X < enemy.controller.frames[0].Width + gameref.res.ScreenWidth) 
                    side = -1;
                enemy.setDepth(gameref.rand.Next(4));

                Vector2 posOffset = new Vector2(side * (gameref.res.ScreenWidth / 2 + enemy.controller.frames[0].Width + 18), enemy.controller.frames[0].Height + 1),
                    avePosition = gameref.getAvePosition();
                enemy.body.Position = new Vector2(avePosition.X + posOffset.X, avePosition.Y - gameref.res.ScreenHeight / 2 + posOffset.Y);
                while (enemy.isTouchingGeom(false))
                    enemy.body.Position = new Vector2(enemy.body.Position.X + side * enemy.controller.frames[0].Width, enemy.body.Position.Y);
                enemies.Add(enemy.guid, enemy);
                gameref.network.sendNewEnemy(enemy);
            }
        }
        public void update(GameTime gameTime)
        {
            #region Activate spawns
            Vector2 avePosition = gameref.getAvePosition();
            foreach (SpawnPoint spawnp in spawnPoints)
                if ((spawnp.loc.X < avePosition.X || spawnp.loc.Y < avePosition.Y - gameref.res.ScreenHeight / 2)
                    && !spawnp.isSpawned)
                {
                    spawnGroup(spawnp.type, spawnp.weight/*level*/, spawnp.count);
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

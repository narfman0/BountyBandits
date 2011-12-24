using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BountyBandits.Animation;
using Microsoft.Xna.Framework;

namespace BountyBandits.Character
{
    public class Enemy : Being
    {
        public int targetPlayer = -1;

        public Enemy(string name, int level, Game gameref, AnimationController controller)
            :base(name, level, gameref, controller, null, false, false) { }

        public override void update(GameTime gameTime)
        {
            base.update(gameTime);
            //figure out if anyone is alive
            bool someoneAlive = false;
            foreach (Being player in gameref.players)
                if (player.currenthealth > 0f)
                    someoneAlive = true;
            //and target that random alive person
            while (someoneAlive && (targetPlayer == -1 || gameref.players[targetPlayer].currenthealth <= 0f))
                targetPlayer = gameref.rand.Next(gameref.players.Count);
            if (targetPlayer < 0)
                return;
            Being targetBeing = gameref.players[targetPlayer];
            if (targetBeing.getDepth() < getDepth()) lane(true);
            else if (targetBeing.getDepth() > getDepth()) lane(false);
            if (isTouchingGeom(false) &&
                body.LinearVelocity.X < .01f &&
                getPos().Y + 10 < targetBeing.getPos().Y)
                jump();
            if (Math.Abs(targetBeing.getPos().X - getPos().X) >
                targetBeing.controller.frames[0].Width / 3 + controller.frames[0].Width / 3)
            {
                bool isLeft = targetBeing.getPos().X < getPos().X;
                move(new Vector2((isLeft ? -1 : 1) * Game.FORCE_AMOUNT, 0));
            }
            if (Vector2.Distance(targetBeing.getPos(), getPos()) < getRange())
                attack("attack1");
        }
    }
}

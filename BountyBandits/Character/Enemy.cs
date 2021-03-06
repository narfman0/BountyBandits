﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BountyBandits.Animation;
using Microsoft.Xna.Framework;
using System.Xml;

namespace BountyBandits.Character
{
    public class Enemy : Being
    {
        public Guid targetPlayer = Guid.Empty;
        private long timeOfLastAttack = 0;

        public Enemy(string name, int level, AnimationController controller)
            :base(name, level, controller, null, false, false) { }

        public override void update(GameTime gameTime)
        {
            base.update(gameTime);
            //figure out if anyone is alive
            bool someoneAlive = false;
            foreach (Being player in Game.instance.players.Values)
                if (player.CurrentHealth > 0f)
                    someoneAlive = true;
            //and target that random alive person
            while (someoneAlive && (targetPlayer == Guid.Empty || Game.instance.players[targetPlayer].isDead))
                targetPlayer = Game.instance.players.Values.ElementAt(Game.instance.rand.Next(Game.instance.players.Count)).guid;
            if (targetPlayer == Guid.Empty)
                return;
            Being targetBeing = Game.instance.players[targetPlayer];
            if (targetBeing.getDepth() != getDepth()) 
                lane(targetBeing.getDepth() < getDepth());
            if (isTouchingGeom(false) != null &&
                body.LinearVelocity.X < .01f &&
                getPos().Y + 10 < targetBeing.getPos().Y)
                jump();
            if (Math.Abs(targetBeing.getPos().X - getPos().X) >
                targetBeing.controller.frames[0].Width / 3 + controller.frames[0].Width / 3)
                move(new Vector2((targetBeing.getPos().X < getPos().X ? -1 : 1) * Game.FORCE_AMOUNT, 0));
            if (Environment.TickCount - timeOfLastAttack > getAttackWaitTime() &&
                Vector2.Distance(targetBeing.getPos(), getPos()) < getRange())
            {
                timeOfLastAttack = Environment.TickCount;
                attack("attack1");
            }
        }

        private long getAttackWaitTime()
        {
            return (long)(4073.95 * Math.Pow(Math.E, -.0184674 * level));
        }

        public static new Enemy fromXML(XmlElement element)
        {
            AnimationController controller = Game.instance.animationManager.getController(element.GetAttribute("animationControllerName"));
            Enemy being = new Enemy(element.GetAttribute("name"), 1, controller);
            being.copyValues(element);
            return being;
        }
    }
}

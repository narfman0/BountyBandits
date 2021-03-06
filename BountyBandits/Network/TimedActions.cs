﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BountyBandits.Network
{
    class TimedActions
    {
        private Dictionary<TimedUpdate, long> serverTimedUpdates;
        public TimedActions()
        {
            serverTimedUpdates = new Dictionary<TimedUpdate, long>();
        }

        public bool isActionReady(GameTime gameTime, int updateTime, TimedUpdate updateType)
        {
            long stateChangeTime = updateTime;
            if (serverTimedUpdates.ContainsKey(updateType))
            {
                stateChangeTime = serverTimedUpdates[updateType];
                serverTimedUpdates.Remove(updateType);
            }
            stateChangeTime -= gameTime.ElapsedGameTime.Ticks;
            if (stateChangeTime < 0)
            {
                serverTimedUpdates.Add(updateType, updateTime);
                return true;
            }
            serverTimedUpdates.Add(updateType, stateChangeTime);
            return false;
        }
    }
}

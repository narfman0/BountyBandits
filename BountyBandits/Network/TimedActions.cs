using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BountyBandits.Network
{
    class TimedActions
    {
        private Dictionary<ServerTimedUpdate, long> serverTimedUpdates;
        public TimedActions()
        {
            serverTimedUpdates = new Dictionary<ServerTimedUpdate, long>();
        }

        public bool isActionReady(GameTime gameTime, int updateTime, ServerTimedUpdate updateType)
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

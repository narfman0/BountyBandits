using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace BountyBandits.Network
{
    public struct BeingNetworkState
    {
        public Guid guid;
        public Vector2 position, velocity;

        public static void writeBeingState(NetOutgoingMessage msg, Being being)
        {
            msg.Write(being.getPos().X);
            msg.Write(being.getPos().Y);
            msg.Write(being.body.LinearVelocity.X);
            msg.Write(being.body.LinearVelocity.Y);
            msg.Write(being.guid.ToString());
        }

        public static BeingNetworkState readBeingState(NetIncomingMessage msg)
        {
            BeingNetworkState state = new BeingNetworkState();
            state.position = new Vector2(msg.ReadFloat(), msg.ReadFloat());
            state.velocity = new Vector2(msg.ReadFloat(), msg.ReadFloat());
            state.guid = new Guid(msg.ReadString());
            return state;
        }
    }
}

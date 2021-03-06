﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using BountyBandits.Character;

namespace BountyBandits.Network
{
    public struct BeingNetworkState
    {
        public Guid guid;
        public Vector2 position, velocity;
        public bool isFacingLeft, isMovingUp;
        public int depth;
        public float currentHP;
        public int stunDuration;
        public short tag;

        public static void writeState(NetOutgoingMessage msg, Being being)
        {
            msg.Write(being.getPos().X);
            msg.Write(being.getPos().Y);
            msg.Write(being.body.LinearVelocity.X);
            msg.Write(being.body.LinearVelocity.Y);
            msg.Write(being.guid.ToString());
            msg.Write(being.isFacingLeft);
            msg.Write((byte)being.getDepth());
            msg.Write(being.isMovingUp);
            msg.Write(being.CurrentHealth);
            msg.Write(being.stunDuration);
            msg.Write(being.body.Tag != null);
            if(being.body.Tag != null)
                msg.Write((short)being.body.Tag);
        }

        public static BeingNetworkState readState(NetIncomingMessage msg)
        {
            BeingNetworkState state = new BeingNetworkState();
            state.position = new Vector2(msg.ReadFloat(), msg.ReadFloat());
            state.velocity = new Vector2(msg.ReadFloat(), msg.ReadFloat());
            state.guid = new Guid(msg.ReadString());
            state.isFacingLeft = msg.ReadBoolean();
            state.depth = msg.ReadByte();
            state.isMovingUp = msg.ReadBoolean();
            state.currentHP = msg.ReadFloat();
            state.stunDuration = msg.ReadInt32();
            if(msg.ReadBoolean())
                state.tag = msg.ReadInt16();
            return state;
        }
    }
    public struct GameItemNetworkState
    {
        public Guid guid;
        public Vector2 position, velocity;
        public float rotation, angularVelocity;
        public Int16 tag;

        public static void writeState(NetOutgoingMessage msg, GameItem gameItem)
        {
            msg.Write(gameItem.body.Position.X);
            msg.Write(gameItem.body.Position.Y);
            msg.Write(gameItem.body.LinearVelocity.X);
            msg.Write(gameItem.body.LinearVelocity.Y);
            msg.Write(gameItem.body.Rotation);
            msg.Write(gameItem.body.AngularVelocity);
            msg.Write((Int16)(gameItem.body.Tag == null ? (short)0 : gameItem.body.Tag));
            msg.Write(gameItem.guid.ToString());
        }

        public static GameItemNetworkState readState(NetIncomingMessage msg)
        {
            GameItemNetworkState state = new GameItemNetworkState();
            state.position = new Vector2(msg.ReadFloat(), msg.ReadFloat());
            state.velocity = new Vector2(msg.ReadFloat(), msg.ReadFloat());
            state.rotation = msg.ReadFloat();
            state.angularVelocity = msg.ReadFloat();
            state.tag = msg.ReadInt16();
            state.guid = new Guid(msg.ReadString());
            return state;
        }
    }
}

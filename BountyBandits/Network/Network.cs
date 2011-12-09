﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using System.Net;
using System.Xml;
using Microsoft.Xna.Framework;
using System.Threading;

namespace BountyBandits.Network
{
    public class NetworkManager
    {
        private const string APPID = "bountyBandits";
        private NetServer server;
        private NetClient client;
        private Game gameref;
        public static String joinString = "localhost";
        private TimedActions timedActions = new TimedActions();
        public NetworkManager(Game gameref)
        {
            this.gameref = gameref;
        }
        public bool isServer()
        {
            return server != null && server.Status != NetPeerStatus.NotRunning;
        }
        public bool isClient()
        {
            return client != null && client.ConnectionStatus != NetConnectionStatus.Disconnected;
        }
        public void startServer()
        {
            NetPeerConfiguration config = new NetPeerConfiguration(APPID);
            config.Port = Const.GameServerPort;

            server = new NetServer(config);
            server.Start();
            Log.write(LogType.NetworkServer, "Server started");
            //if internet game, add sending registration ot master server
            //IPEndPoint masterServerEndpoint = NetUtility.Resolve("localhost", Const.GameServerPort);
        }
        public bool startClient()
        {
            NetPeerConfiguration config = new NetPeerConfiguration(APPID);
            client = new NetClient(config);
            client.Start();
            NetOutgoingMessage hail = client.CreateMessage();
            hail.Write("Hello World *winkz*");
            client.Connect(joinString, Const.GameServerPort, hail);
            Log.write(LogType.NetworkClient, "Client connecting...");

            int msUntilDisconnect = 1000;
            while (client.ConnectionsCount == 0)
                if (msUntilDisconnect-- < 0)
                {
                    Log.write(LogType.NetworkClient, "Client failed to conenct");
                    return false;
                }
                else
                    Thread.Sleep(1);
            Log.write(LogType.NetworkClient, "Client connected");
            return true;
        }
        public void update(GameTime gameTime)
        {
            if (isClient())
            {
                updateClient();
                if (timedActions.isActionReady(gameTime, 100, TimedUpdate.PlayersUpdate))
                    sendPlayersUpdateClient();
            }
            if (isServer())
            {
                updateServer();
                if (timedActions.isActionReady(gameTime, 1500, TimedUpdate.State))
                    sendGameStateUpdate();
                if (timedActions.isActionReady(gameTime, 100, TimedUpdate.PlayersUpdate))
                    sendPlayersUpdateServer();
                if (timedActions.isActionReady(gameTime, 100, TimedUpdate.ObjectsUpdate))
                    sendObjectsUpdate();
            }
        }
        public void updateClient()
        {
            NetIncomingMessage im;
            while ((im = client.ReadMessage()) != null)
            {
                switch (im.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        switch (im.ReadInt32())
                        {
                            case (int)MessageType.GameState:
                                receiveGameStateUpdate(im);
                                break;
                            case (int)MessageType.PlayersUpdate:
                                receivePlayersUpdate(im);
                                break;
                            case (int)MessageType.LevelIndexChange:
                                receiveLevelIndexChange(im);
                                break;
                            case (int)MessageType.PlayerFullUpdateServer:
                                receiveFullPlayersUpdate(im);
                                break;
                            case (int)MessageType.ObjectsFullUpdate:
                                receiveFullObjectsUpdate(im);
                                break;
                            case (int)MessageType.ObjectsUpdate:
                                receiveObjectsUpdate(im);
                                break;
                            default:
                                Log.write(LogType.NetworkClient, "Unknown data message received");
                                break;
                        }
                        break;
                    default:
                        Log.write(LogType.NetworkClient, im.MessageType.ToString() + " received. " + im.LengthBytes + " bytes");
                        break;
                }
            }
        }
        public void updateServer()
        {
            NetIncomingMessage im;
            while ((im = server.ReadMessage()) != null)
            {
                switch (im.MessageType)
                {
                    case NetIncomingMessageType.ConnectionApproval:
                        Log.write(LogType.NetworkServer, "Connection approved from " + im.SenderEndpoint.ToString());
                        sendGameStateUpdate();
                        break;
                    case NetIncomingMessageType.Data:
                        switch (im.ReadInt32())
                        {
                            case (int)MessageType.PlayerFullUpdateClient:
                                receiveFullPlayersUpdate(im);
                                sendFullPlayersUpdateServer();
                                break;
                            case (int)MessageType.LevelIndexChange:
                                receiveIncrementLevelRequest(im);
                                break;
                            case (int)MessageType.PlayersUpdateClient:
                                receivePlayersUpdate(im);
                                break;
                            default:
                                Log.write(LogType.NetworkServer, "Unknown data message received");
                                break;
                        }
                        break;
                    default:
                        Log.write(LogType.NetworkServer, im.MessageType.ToString() + " received. " + im.LengthBytes + " bytes");
                        break;
                }
            }
        }
        public void sendGameStateUpdate()
        {
            if (!gameref.network.isServer() || server.ConnectionsCount < 1 || gameref.currentState.getState() == GameState.Cutscene)
                return;
            NetOutgoingMessage stateUpdate = server.CreateMessage();
            stateUpdate.Write((int)MessageType.GameState);
            stateUpdate.Write((int)gameref.currentState.getState());
            server.SendToAll(stateUpdate, NetDeliveryMethod.ReliableUnordered);

            if (gameref.currentState.getState() == GameState.Gameplay)
                sendFullObjectsUpdate();
        }
        private void receiveGameStateUpdate(NetIncomingMessage im)
        {
            int newState = im.ReadInt32();
            if (gameref.currentState.getState() != GameState.CharacterSelection)
            {
                gameref.currentState.setState((GameState)newState);
                if (gameref.currentState.getState() != GameState.Gameplay)
                    gameref.newLevel();
            }
        }
        public void sendIncrementLevelRequest(bool up)
        {
            if (!gameref.network.isClient())
                return;
            NetOutgoingMessage msg = client.CreateMessage();
            msg.Write((int)MessageType.IncrementLevelRequest);
            msg.Write(up);
            client.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);
        }
        private void receiveIncrementLevelRequest(NetIncomingMessage im)
        {
            if (!gameref.network.isServer())
                return;
            gameref.mapManager.incrementCurrentLevel(im.ReadBoolean());
            sendLevelIndexChange(gameref.mapManager.getCurrentLevelIndex());
        }
        public void sendLevelIndexChange(int newLevelIndex)
        {
            if (!gameref.network.isServer() || server.ConnectionsCount < 1)
                return;
            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write((int)MessageType.LevelIndexChange);
            msg.Write(newLevelIndex);
            server.SendToAll(msg, NetDeliveryMethod.ReliableUnordered);
        }
        private void receiveLevelIndexChange(NetIncomingMessage im)
        {
            if (!gameref.network.isClient())
                return;
            int newLevelIndex = im.ReadInt32();
            while (gameref.mapManager.getCurrentLevelIndex() != newLevelIndex)
                gameref.mapManager.incrementCurrentLevel(gameref.mapManager.getCurrentLevelIndex() < newLevelIndex);
        }
        private void sendPlayersUpdateClient()
        {
            if (!gameref.network.isClient())
                return;
            List<Being> localList = new List<Being>();
            foreach (Being player in gameref.players)
                if (player.isLocal)
                    localList.Add(player);

            NetOutgoingMessage msg = client.CreateMessage();
            msg.Write((int)MessageType.PlayersUpdateClient);
            msg.Write((int)localList.Count);
            foreach (Being player in localList)
                BeingNetworkState.writeState(msg, player);
            client.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);
        }
        public void sendPlayersUpdateServer()
        {
            if (!gameref.network.isServer() || server.ConnectionsCount < 1)
                return;
            NetOutgoingMessage stateUpdate = server.CreateMessage();
            stateUpdate.Write((int)MessageType.PlayersUpdate);
            stateUpdate.Write((int)gameref.players.Count);
            foreach (Being player in gameref.players)
                BeingNetworkState.writeState(stateUpdate, player);
            server.SendToAll(stateUpdate, NetDeliveryMethod.ReliableUnordered);
        }
        private void receivePlayersUpdate(NetIncomingMessage im)
        {
            int count = im.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                BeingNetworkState state = BeingNetworkState.readState(im);
                foreach (Being player in gameref.players)
                    if (!player.isLocal && player.guid == state.guid)
                    {
                        player.body.LinearVelocity = state.velocity;
                        Vector2 difference = state.position - player.body.Position;
                        player.body.Position = player.body.Position + (difference * .1f);
                        player.isFacingLeft = state.isFacingLeft;
                        if (state.depth != player.getDepth())
                        {
                            player.timeOfLastDepthChange = Environment.TickCount;
                            player.setDepth(state.depth);
                        }
                    }
            }
        }
        public void sendFullPlayersUpdateClient()
        {
            List<Being> localList = new List<Being>();
            foreach (Being player in gameref.players)
                if (player.isLocal)
                    localList.Add(player);

            NetOutgoingMessage msg = client.CreateMessage();
            msg.Write((int)MessageType.PlayerFullUpdateClient);
            msg.Write(localList.Count);
            foreach (Being player in localList)
                msg.Write(player.asXML(new XmlDocument().CreateDocumentFragment()).OuterXml);
            client.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);
        }
        private void receiveFullPlayersUpdate(NetIncomingMessage im)
        {
            int count = im.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                String beingXML = im.ReadString();
                Being being = Being.fromXML(XMLUtil.asXML(beingXML), gameref);
                bool found = false;
                foreach (Being player in gameref.players)
                    if (player.guid == being.guid)
                        found = true;
                if(!found)
                    gameref.players.Add(being);
            }
        }
        public void sendFullPlayersUpdateServer()
        {
            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write((int)MessageType.PlayerFullUpdateServer);
            msg.Write(gameref.players.Count);
            foreach (Being player in gameref.players)
                msg.Write(player.asXML(new XmlDocument().CreateDocumentFragment()).OuterXml);
            server.SendToAll(msg, NetDeliveryMethod.ReliableUnordered);
        }
        private void sendFullObjectsUpdate()
        {
            List<GameItem> gameItems = new List<GameItem>();
            foreach (GameItem item in gameref.activeItems)
                if (!(item is DropItem))
                    gameItems.Add(item);

            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write((int)MessageType.ObjectsFullUpdate);
            msg.Write(gameItems.Count);
            foreach (GameItem item in gameItems)
                msg.Write(item.asXML(new XmlDocument().CreateDocumentFragment()).OuterXml);

            /*
            List<DropItem> dropItems = new List<DropItem>();
            foreach (DropItem item in gameref.activeItems)
                if (item is DropItem)
                    dropItems.Add(item);
            msg.Write(dropItems.Count);
            foreach (DropItem item in dropItems)
                msg.Write(item.asXML(new XmlDocument().CreateDocumentFragment()).OuterXml);
            server.SendToAll(msg, NetDeliveryMethod.ReliableUnordered);*/
        }
        private void receiveFullObjectsUpdate(NetIncomingMessage im)
        {
            int count = im.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                String gameItemXML = im.ReadString();
                GameItem item = GameItem.fromXML(XMLUtil.asXML(gameItemXML));
                bool found = false;
                foreach (GameItem activeitem in gameref.activeItems)
                    if (activeitem.guid == item.guid)
                        found = true;
                if (!found)
                    gameref.addGameItem(item);
            }
            /*count = im.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                String gameItemXML = im.ReadString();
                DropItem item = DropItem.fromXML(XMLUtil.asXML(gameItemXML));
                bool found = false;
                foreach (GameItem activeitem in gameref.activeItems)
                    if (activeitem.guid == item.guid)
                        found = true;
                if (!found)
                    gameref.addGameItem(item);
            }*/
        }
        private void sendObjectsUpdate()
        {
            List<GameItem> gameItems = new List<GameItem>();
            foreach (GameItem item in gameref.activeItems)
                if (!item.immovable && !item.body.IsStatic && !(item is DropItem))
                    gameItems.Add(item);

            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write((int)MessageType.ObjectsUpdate);
            msg.Write(gameItems.Count);
            foreach (GameItem item in gameItems)
                GameItemNetworkState.writeState(msg, item);
        }
        private void receiveObjectsUpdate(NetIncomingMessage im)
        {
            int count = im.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                GameItemNetworkState state = GameItemNetworkState.readState(im);
                foreach (GameItem item in gameref.activeItems)
                    if (item.guid == state.guid)
                    {
                        Vector2 positiondifference = state.position - item.body.Position;
                        float rotationDifference = state.rotation - item.body.Rotation;
                        item.body.LinearVelocity = state.velocity;
                        item.body.Position = item.body.Position + (positiondifference * .1f);
                        item.body.Rotation = state.rotation + (rotationDifference * .1f);//this might not be right
                        item.body.AngularVelocity = state.angularVelocity;
                    }
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using System.Net;
using System.Xml;
using Microsoft.Xna.Framework;
using System.Threading;
using BountyBandits.Character;

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
        public void shutdown()
        {
            if (isClient())
                client.Shutdown("kbye");
            else if (isServer())
                server.Shutdown("serverkbye");
        }
        public void update(GameTime gameTime)
        {
            if (isClient())
            {
                updateClient();
                if (timedActions.isActionReady(gameTime, 100, TimedUpdate.BeingsUpdate))
                    sendPlayersUpdateClient();
            }
            if (isServer())
            {
                updateServer();
                if (gameref.network.isServer() && server.ConnectionsCount > 0)
                {
                    if (timedActions.isActionReady(gameTime, 100, TimedUpdate.BeingsUpdate))
                    {
                        sendPlayersUpdateServer();
                        sendEnemiesUpdate();
                    } if (timedActions.isActionReady(gameTime, 100, TimedUpdate.ObjectsUpdate))
                        sendObjectsUpdate();
                }
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
                            case (int)MessageType.BeingAnimationChange:
                                receiveBeingAnimationChange(im);
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
                            case (int)MessageType.NewEnemy:
                                receiveNewEnemy(im);
                                break;
                            case (int)MessageType.EnemiesUpdate:
                                receiveEnemiesUpdate(im);
                                break;
                            case (int)MessageType.BeingNewCombatText:
                                receiveBeingNewCombatText(im);
                                break;
                            case (int)MessageType.BeingNewCurrentHP:
                                receiveBeingNewCurrentHP(im);
                                break;
                            case (int)MessageType.AddXP:
                                receiveAddXP(im);
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
                            case (int)MessageType.BeingAnimationChange:
                                receiveBeingAnimationChange(im);
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
        private void serverSendToAllUnordered(NetOutgoingMessage msg)
        {
            if (gameref.network.isServer() && server.ConnectionsCount > 0)
                try
                {
                    server.SendToAll(msg, NetDeliveryMethod.ReliableUnordered);
                }
                catch (Exception e)
                {
                    Log.write(LogType.NetworkServer, e.Message);
                }
        }
        public void sendGameStateUpdate()
        {
            if (!gameref.network.isServer() || server.ConnectionsCount < 1)
                return;
            NetOutgoingMessage stateUpdate = server.CreateMessage();
            stateUpdate.Write((int)MessageType.GameState);
            stateUpdate.Write((int)gameref.currentState.getState());
            if (gameref.currentState.getState() == GameState.Gameplay)
                stateUpdate.Write(gameref.mapManager.getCurrentLevelIndex());
            serverSendToAllUnordered(stateUpdate);

            if (gameref.currentState.getState() == GameState.Gameplay)
                sendFullObjectsUpdate();
        }
        private void receiveGameStateUpdate(NetIncomingMessage im)
        {
            int newState = im.ReadInt32();
            if (gameref.currentState.getState() != GameState.CharacterSelection)
            {
                if ((GameState)newState == GameState.Gameplay)
                    gameref.mapManager.currentLevelIndex = im.ReadInt32();
                if (gameref.currentState.getState() != GameState.Gameplay && (GameState)newState == GameState.Gameplay)
                    gameref.newLevel();
                else if (gameref.currentState.getState() != GameState.WorldMap && (GameState)newState == GameState.WorldMap)
                    gameref.endLevel(false);
                gameref.currentState.setState((GameState)newState);
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
            serverSendToAllUnordered(msg);
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
            foreach (Being player in gameref.players.Values)
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
            NetOutgoingMessage stateUpdate = server.CreateMessage();
            stateUpdate.Write((int)MessageType.PlayersUpdate);
            stateUpdate.Write((int)gameref.players.Count);
            foreach (Being player in gameref.players.Values)
                BeingNetworkState.writeState(stateUpdate, player);
            serverSendToAllUnordered(stateUpdate);
        }
        private void receivePlayersUpdate(NetIncomingMessage im)
        {
            int count = im.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                BeingNetworkState state = BeingNetworkState.readState(im);
                if (gameref.players.ContainsKey(state.guid))
                {
                    Being player = gameref.players[state.guid];
                    if (!player.isLocal)
                    {
                        player.body.LinearVelocity = state.velocity;
                        Vector2 difference = state.position - player.body.Position;
                        player.body.Position = player.body.Position + (difference * .1f);
                        player.isFacingLeft = state.isFacingLeft;
                        player.CurrentHealth = state.currentHP;
                        player.stunDuration = state.stunDuration;
                        player.body.Tag = state.tag;
                        if (state.depth != player.getDepth())
                        {
                            player.timeOfLastDepthChange = Environment.TickCount;
                            player.setDepth(state.depth);
                            player.isMovingUp = state.isMovingUp;
                        }
                    }
                }
            }
        }
        public void sendFullPlayersUpdateClient()
        {
            List<Being> localList = new List<Being>();
            foreach (Being player in gameref.players.Values)
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
                Being being = Being.fromXML(XMLUtil.asXML(im.ReadString()));
                if(!gameref.players.ContainsKey(being.guid))
                    gameref.players.Add(being.guid,being);
            }
        }
        public void sendFullPlayersUpdateServer()
        {
            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write((int)MessageType.PlayerFullUpdateServer);
            msg.Write(gameref.players.Count);
            foreach (Being player in gameref.players.Values)
                msg.Write(player.asXML(new XmlDocument().CreateDocumentFragment()).OuterXml);
            serverSendToAllUnordered(msg);
        }
        public void sendFullObjectsUpdate()
        {
            if (!isServer() && !isClient())
                return;
            List<GameItem> gameItems = new List<GameItem>();
            foreach (GameItem item in gameref.activeItems.Values)
                if (!(item is DropItem))
                    gameItems.Add(item);

            NetOutgoingMessage msg = isServer() ? server.CreateMessage() : client.CreateMessage();
            msg.Write((int)MessageType.ObjectsFullUpdate);
            msg.Write(gameItems.Count);
            foreach (GameItem item in gameItems)
                msg.Write(item.asXML(new XmlDocument()).OuterXml);

            List<DropItem> dropItems = new List<DropItem>();
            foreach (GameItem item in gameref.activeItems.Values)
                if (item is DropItem)
                    dropItems.Add((DropItem)item);
            msg.Write(dropItems.Count);
            foreach (DropItem item in dropItems)
                msg.Write(item.asXML(new XmlDocument()).OuterXml);
            if (isClient())
                client.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);
            else
                serverSendToAllUnordered(msg);
        }
        private void receiveFullObjectsUpdate(NetIncomingMessage im)
        {
            #region GameItems
            int count = im.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                String gameItemXML = im.ReadString();
                GameItem item = GameItem.fromXML(XMLUtil.asXML(gameItemXML));
                if (!gameref.activeItems.ContainsKey(item.guid))
                    gameref.addGameItem(item);
            }
            #endregion
            // TODO must remove picked up objects and implement updates for dropped items
            #region DropItems
            count = im.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                DropItem item = DropItem.fromXML(XMLUtil.asXML(im.ReadString()));
                if (!gameref.activeItems.ContainsKey(item.guid))
                    gameref.addGameItem(item);
            }
            #endregion
        }
        private void sendObjectsUpdate()
        {
            List<GameItem> gameItems = new List<GameItem>();
            foreach (GameItem item in gameref.activeItems.Values)
                if (!item.immovable && !item.body.IsStatic)
                    gameItems.Add(item);

            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write((int)MessageType.ObjectsUpdate);
            msg.Write(gameItems.Count);
            foreach (GameItem item in gameItems)
                GameItemNetworkState.writeState(msg, item);
            serverSendToAllUnordered(msg);
        }
        private void receiveObjectsUpdate(NetIncomingMessage im)
        {
            int count = im.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                GameItemNetworkState state = GameItemNetworkState.readState(im);
                if (gameref.activeItems.ContainsKey(state.guid))
                {
                    GameItem item = gameref.activeItems[state.guid];
                    Vector2 positiondifference = state.position - item.body.Position;
                    item.body.LinearVelocity = state.velocity;
                    item.body.Position = item.body.Position + (positiondifference * .1f);
                    item.body.Rotation = state.rotation;
                    item.body.AngularVelocity = state.angularVelocity;
                    if (state.tag > 1)
                        item.body.Tag = state.tag;
                }
            }
        }
        public void sendNewEnemy(Being enemy)
        {
            if (!isServer())
                return;
            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write((int)MessageType.NewEnemy);
            msg.Write(enemy.asXML(new XmlDocument().CreateDocumentFragment()).OuterXml);
            serverSendToAllUnordered(msg);
        }
        public void receiveNewEnemy(NetIncomingMessage im)
        {
            Enemy being = Enemy.fromXML(XMLUtil.asXML(im.ReadString()));
            if (!gameref.spawnManager.enemies.ContainsKey(being.guid))
                gameref.spawnManager.enemies.Add(being.guid, being);
        }
        private void sendEnemiesUpdate()
        {
            NetOutgoingMessage stateUpdate = server.CreateMessage();
            stateUpdate.Write((int)MessageType.EnemiesUpdate);
            stateUpdate.Write((int)gameref.spawnManager.enemies.Count);
            foreach (Enemy enemy in gameref.spawnManager.enemies.Values)
                BeingNetworkState.writeState(stateUpdate, enemy);
            serverSendToAllUnordered(stateUpdate);
        }
        private void receiveEnemiesUpdate(NetIncomingMessage im)
        {
            int count = im.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                BeingNetworkState state = BeingNetworkState.readState(im);
                if (gameref.spawnManager.enemies.ContainsKey(state.guid))
                {
                    Enemy enemy = gameref.spawnManager.enemies[state.guid];
                    enemy.body.LinearVelocity = state.velocity;
                    Vector2 difference = state.position - enemy.body.Position;
                    enemy.body.Position = enemy.body.Position + (difference * .1f);
                    enemy.isFacingLeft = state.isFacingLeft;
                    enemy.CurrentHealth = state.currentHP;
                    if (state.depth != enemy.getDepth())
                    {
                        enemy.timeOfLastDepthChange = Environment.TickCount;
                        enemy.setDepth(state.depth);
                    }
                }
            }
        }
        public void sendBeingAnimationChange(Guid beingGuid, String animationName)
        {
            if (!isServer() && !isClient())
                return;
            NetOutgoingMessage msg = isServer() ? server.CreateMessage() : client.CreateMessage();
            msg.Write((int)MessageType.BeingAnimationChange);
            msg.Write(beingGuid.ToString());
            msg.Write(animationName);
            if (isClient())
                client.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);
            else
                serverSendToAllUnordered(msg);
        }
        private void receiveBeingAnimationChange(NetIncomingMessage im)
        {
            Guid guid = Guid.Parse(im.ReadString());
            String animationName = im.ReadString();
            if (gameref.spawnManager.enemies.ContainsKey(guid))
                gameref.spawnManager.enemies[guid].changeAnimation(animationName);
            else if (gameref.players.ContainsKey(guid))
                gameref.players[guid].changeAnimation(animationName);
        }
        public void sendNewCombatText(Guid guid, string text, CombatTextType type)
        {
            if(!isServer())
                return;
            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write((int)MessageType.BeingNewCombatText);
            msg.Write(guid.ToString());
            msg.Write(text);
            msg.Write((byte)type);
            serverSendToAllUnordered(msg);
        }
        public void receiveBeingNewCombatText(NetIncomingMessage im)
        {
            Guid guid = Guid.Parse(im.ReadString());
            String text = im.ReadString();
            CombatTextType type = (CombatTextType)im.ReadByte();
            Being toUpdate = null;
            if (gameref.spawnManager.enemies.ContainsKey(guid))
                toUpdate = gameref.spawnManager.enemies[guid];
            else if (gameref.players.ContainsKey(guid))
                toUpdate = gameref.players[guid];
            if(toUpdate != null && !toUpdate.isLocal)
                toUpdate.combatText.add(guid, text, type);
        }
        public void sendBeingCurrentHP(Guid guid, float currentHP)
        {
            if (!isServer())
                return;
            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write((int)MessageType.BeingNewCurrentHP);
            msg.Write(guid.ToString());
            msg.Write(currentHP);
            serverSendToAllUnordered(msg);
        }
        private void receiveBeingNewCurrentHP(NetIncomingMessage im)
        {
            Guid guid = Guid.Parse(im.ReadString());
            float currentHP = im.ReadFloat();
            if (gameref.spawnManager.enemies.ContainsKey(guid))
                gameref.spawnManager.enemies[guid].CurrentHealth = currentHP;
            else if (gameref.players.ContainsKey(guid))
                gameref.players[guid].CurrentHealth = currentHP;
        }
        public void sendAddXP(int addXP)
        {
            if (!isServer())
                return;
            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write((int)MessageType.AddXP);
            msg.Write(addXP);
            serverSendToAllUnordered(msg);
        }
        private void receiveAddXP(NetIncomingMessage im)
        {
            int addXP = im.ReadInt32();
            foreach (Being player in gameref.players.Values)
                player.giveXP(addXP);
        }
    }
}

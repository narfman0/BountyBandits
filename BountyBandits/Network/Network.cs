using System;
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
                updateClient();
            if (isServer())
            {
                updateServer();
                if (timedActions.isActionReady(gameTime, 1500, ServerTimedUpdate.State))
                    sendGameStateUpdate();
                if (timedActions.isActionReady(gameTime, 100, ServerTimedUpdate.PlayersUpdates))
                    sendServerPlayersUpdate();
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
                                if(gameref.currentState.getState() != GameState.CharacterSelection)
                                    gameref.currentState.setState((GameState)im.ReadInt32());
                                break;
                            case (int)MessageType.PlayersUpdate:
                                receivePlayersUpdate(im);
                                break;
                            case (int)MessageType.LevelIndexChange:
                                receiveLevelIndexChange(im);
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
                            case (int)MessageType.InitialSendCharacter:
                                int count = im.ReadInt32();
                                for (int i = 0; i < count; i++)
                                {
                                    String beingXML = im.ReadString();
                                    Being being = Being.fromXML(XMLUtil.asXML(beingXML), gameref);
                                    gameref.players.Add(being);
                                }
                                break;
                            case (int)MessageType.LevelIndexChange:
                                receiveIncrementLevelRequest(im);
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
            if (!gameref.network.isServer() || server.ConnectionsCount < 1)
                return;
            NetOutgoingMessage stateUpdate = server.CreateMessage();
            stateUpdate.Write((int)MessageType.GameState);
            stateUpdate.Write((int)gameref.currentState.getState());
            server.SendToAll(stateUpdate, NetDeliveryMethod.ReliableUnordered);
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
        public void receiveIncrementLevelRequest(NetIncomingMessage im)
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
        public void receiveLevelIndexChange(NetIncomingMessage im)
        {
            if (!gameref.network.isClient())
                return;
            int newLevelIndex = im.ReadInt32();
            while (gameref.mapManager.getCurrentLevelIndex() != newLevelIndex)
                gameref.mapManager.incrementCurrentLevel(gameref.mapManager.getCurrentLevelIndex() < newLevelIndex);
        }
        public void sendServerPlayersUpdate()
        {
            if (!gameref.network.isServer() || server.ConnectionsCount < 1)
                return;
            NetOutgoingMessage stateUpdate = server.CreateMessage();
            stateUpdate.Write((int)MessageType.PlayersUpdate);
            stateUpdate.Write((int)gameref.players.Count);
            foreach (Being player in gameref.players)
                BeingNetworkState.writeBeingState(stateUpdate, player);
            server.SendToAll(stateUpdate, NetDeliveryMethod.ReliableUnordered);
        }
        public void sendClientPlayersUpdate()
        {
            NetOutgoingMessage initialCharmsg = client.CreateMessage();
            initialCharmsg.Write((int)MessageType.InitialSendCharacter);
            initialCharmsg.Write(gameref.players.Count);
            foreach (Being player in gameref.players)
            {
                String playerStr = player.asXML(new XmlDocument().CreateDocumentFragment()).InnerXml;
                initialCharmsg.Write(playerStr);
            }
            client.SendMessage(initialCharmsg, NetDeliveryMethod.ReliableUnordered);
        }
        public void receivePlayersUpdate(NetIncomingMessage im)
        {
            int count = im.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                BeingNetworkState state = BeingNetworkState.readBeingState(im);
                foreach (Being player in gameref.players)
                    if (player.guid == state.guid)
                    {
                        player.body.LinearVelocity = state.velocity;
                        Vector2 difference = state.position - player.body.Position;
                        player.body.Position = player.body.Position + difference * .1f;
                    }
            }
        }
    }
}

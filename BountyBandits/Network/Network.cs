using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using System.Net;

namespace BountyBandits.Network
{
    public class NetworkManager
    {
        private NetServer server;
        private NetClient client;
        public static String joinString = "jrob.no-ip.org";

        public void startServer()
        {

            NetPeerConfiguration config = new NetPeerConfiguration("bountyBanditsServer");
            config.Port = Const.GameServerPort;

            server = new NetServer(config);
            server.Start();
            //if internet game, add sending registration ot master server
            //IPEndPoint masterServerEndpoint = NetUtility.Resolve("localhost", Const.GameServerPort);
        }

        /// <summary>
        /// Connect using joinString
        /// </summary>
        public void startClient(){
            IPEndPoint point = new IPEndPoint(NetUtility.Resolve(joinString), Const.GameServerPort);
            NetPeerConfiguration config = new NetPeerConfiguration("bountyBanditsClient");
            config.SetMessageTypeEnabled(NetIncomingMessageType.UnconnectedData, true);
            config.Port = Const.GameServerPort;
            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);
            config.EnableMessageType(NetIncomingMessageType.NatIntroductionSuccess);
            client = new NetClient(config);
            client.Start();
            client.Connect(point);
        }
    }
}

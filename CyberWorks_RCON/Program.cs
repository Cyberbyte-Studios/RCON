using System;
using System.Configuration;
using System.Reflection;
using System.Net;
using BattleNET;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

namespace CWRCON
{
    internal class Program
    {
        static Socket s;
        static Boolean cmdSent = false;
        private static void Main(string[] args)
        {
            Console.WriteLine(
                "BattleNET v1.3.3 - BattlEye Library and Client\n\n" +
                "Copyright (C) 2015 by it's authors.\n" +
                "Some rights reserved. See license.txt, authors.txt.\n"
            );

            BattlEyeLoginCredentials loginCredentials;

            loginCredentials = GetLoginCredentials();

            Console.Title = string.Format("CyberWorks RCON Client Using BattleNETv1.3.3 - {0}:{1}", loginCredentials.Host, loginCredentials.Port);

            BattlEyeClient b = new BattlEyeClient(loginCredentials);
            b.BattlEyeMessageReceived += BattlEyeMessageReceived;
            b.BattlEyeConnected += BattlEyeConnected;
            b.BattlEyeDisconnected += BattlEyeDisconnected;
            b.ReconnectOnPacketLoss = true;
            b.Connect();

            try {
                IPAddress ipAd = IPAddress.Parse(ConfigurationManager.AppSettings["CW_HOST"]);
                TcpListener listener = new TcpListener(ipAd, int.Parse(ConfigurationManager.AppSettings["CW_PORT"]));
                listener.Start();

                Console.WriteLine("CW Server is running at port {0}...", ConfigurationManager.AppSettings["CW_PORT"]);
                Console.WriteLine("CW Server End point is  :" + listener.LocalEndpoint);
                Console.WriteLine("Waiting for a connection.....");
 
                while (true) {
                    s = listener.AcceptSocket();
                    Console.WriteLine("Connection accepted from " + s.RemoteEndPoint);

                    byte[] bb = new byte[100];
                    int k = s.Receive(bb);
                    Console.WriteLine("Recieved...");
                    var cmd = Encoding.Default.GetString(bb);
                    Console.WriteLine("Command: {0}", cmd);

                    if (cmd == "exit" || cmd == "logout") {
                        break;
                    }

                    if (b.Connected) {
                        b.SendCommand(cmd);
                        cmdSent = true;
                    } else {
                        Environment.Exit(0);
                    }
                }

                b.Disconnect();
                s.Close();
                listener.Stop();
            }
            catch (Exception ex){

            }
        }

        private static void BattlEyeConnected(BattlEyeConnectEventArgs args)
        {
            //if (args.ConnectionResult == BattlEyeConnectionResult.Success) { /* Connected successfully */ }
            //if (args.ConnectionResult == BattlEyeConnectionResult.InvalidLogin) { /* Connection failed, invalid login details */ }
            //if (args.ConnectionResult == BattlEyeConnectionResult.ConnectionFailed) { /* Connection failed, host unreachable */ }

            Console.WriteLine(args.Message);
        }

        private static void BattlEyeDisconnected(BattlEyeDisconnectEventArgs args)
        {
            //if (args.DisconnectionType == BattlEyeDisconnectionType.ConnectionLost) { /* Connection lost (timeout), if ReconnectOnPacketLoss is set to true it will reconnect */ }
            //if (args.DisconnectionType == BattlEyeDisconnectionType.SocketException) { /* Something went terribly wrong... */ }
            //if (args.DisconnectionType == BattlEyeDisconnectionType.Manual) { /* Disconnected by implementing application, that would be you */ }

            Console.WriteLine(args.Message);
        }

        private static void BattlEyeMessageReceived(BattlEyeMessageEventArgs args)
        {
            if (cmdSent) {
                ASCIIEncoding asen = new ASCIIEncoding();
                s.Send(asen.GetBytes(args.Message));
                Console.WriteLine("\nSent Msg");
                cmdSent = false;
            }
            Console.WriteLine(args.Message);
        }

        private static BattlEyeLoginCredentials GetLoginCredentials() {
            IPAddress host = null;
            int port = 0;
            string password = "";

            try
            {
                IPAddress ip = Dns.GetHostAddresses(ConfigurationManager.AppSettings["RCON_HOST"])[0];
                host = ip;
            }
            catch { /* try again */ }

            port = int.Parse(ConfigurationManager.AppSettings["RCON_PORT"]);

            password = ConfigurationManager.AppSettings["RCON_PASS"];

            var loginCredentials = new BattlEyeLoginCredentials
            {
                Host = host,
                Port = port,
                Password = password,
            };

            return loginCredentials;
        }
    }
}

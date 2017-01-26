using ConsoleApp1;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MultiServer
{
    class Program
    {
        private static readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly List<Socket> clientSockets = new List<Socket>();
        private const int BUFFER_SIZE = 2048;
        private const int PORT = 11000;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];
        static List<Fighter> sockets = new List<Fighter>();
        public static void Main()
        {
            Console.Title = "Server";
            SetupServer();
            Console.ReadLine(); // When we press enter close everything
            CloseAllSockets();
        }

        private static void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            serverSocket.Listen(0);
            serverSocket.BeginAccept(AcceptCallback, null);
            Console.WriteLine("Server setup complete");
        }

        /// <summary>
        /// Close all connected client (we do not need to shutdown the server socket as its connections
        /// are already closed with the clients).
        /// </summary>
        private static void CloseAllSockets()
        {
            foreach (Socket socket in clientSockets)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            serverSocket.Close();
        }

        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException) // I cannot seem to avoid this (on exit when properly closing sockets)
            {
                return;
            }
                     
            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            Fighter f = new Fighter();
            Fighter.fighters++;
            f.id = Fighter.fighters;
            f.socket = socket; 
            sockets.Add(f);
            int i = 1;
            foreach(Fighter fi in sockets)
            {
                Console.WriteLine(i);
                i++;
            }
            Console.WriteLine("Client connected, waiting for request...");
            serverSocket.BeginAccept(AcceptCallback, null);
        }

        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int received;
            
            try
            {
                received = current.EndReceive(AR);                
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                current.Close();
                clientSockets.Remove(current);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);
            Console.WriteLine("Received Text: " + text);

            if (text.Equals("Charmander") || text.Equals("Squirtle") || text.Equals("Bulbasaur"))
            {
                foreach(Fighter f in sockets)
                {
                    if (f.socket.Equals(current))
                    {
                        f.pokemon = text;
                        f.ready = true;
                    }
                }
            }

            if (sockets.TrueForAll(x => x.ready))
            {
                Console.WriteLine("Ready");
            }

            
            int i = 1;
            foreach (Fighter s in sockets)
            {
                
                i++;
                byte[] b = new byte[512];
                string msg = "hola";
                int x = 0;
                foreach (byte by in msg)
                {
                    b[x] = by;
                    x++;
                }
                b[x] = Encoding.UTF8.GetBytes("$")[0];
                s.socket.Send(b);
                Console.WriteLine("sended to {0} {1}", i, Encoding.UTF8.GetString(b));
            }

            current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
        }
    }
}
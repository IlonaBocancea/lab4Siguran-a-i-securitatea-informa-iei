using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace ServerApp
{
    public class ServerObject
    {
        static TcpListener tcpListener; // serverul pentru ascultare
        List<ClientObject> clients = new List<ClientObject>(); // toate conexiunile
        CaesarCipher cipher = new CaesarCipher();

        protected internal void AddConnection(ClientObject clientObject)
        {
            clients.Add(clientObject);
        }
        protected internal void RemoveConnection(string id)
        {
            // obtinem prin id conexiunea inchisa
            ClientObject client = clients.FirstOrDefault(c => c.Id == id);
            // si stergem aceasta din lista conexiunilor
            if (client != null)
                clients.Remove(client);
        }
        // ascultarea conexiunilor de intrare
        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                Console.WriteLine("Server run. Asteptarea conexiunilor...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        // translarea mesajului clientilor conectati
        protected internal void BroadcastMessage(string message, string id)
        {
            string codingMessage = cipher.Encrypt(message, 3);
            byte[] data = Encoding.Unicode.GetBytes(codingMessage);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id != id) // daca id unui client este diferit de id transmitatorului
                {
                    clients[i].Stream.Write(data, 0, data.Length); // transmiterea datelor
                }
            }
        }
        // deconectarea tuturor clientilor
        protected internal void Disconnect()
        {
            tcpListener.Stop(); //stop server

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); //deconectarea clientului
            }
            Environment.Exit(0); //terminarea procesului
        }
    }
}
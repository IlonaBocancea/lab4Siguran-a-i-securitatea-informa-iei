using System;
using System.Net.Sockets;
using System.Text;

namespace ServerApp
{
    public class ClientObject
    {
        CaesarCipher cipher = new CaesarCipher();
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        string userName;
        TcpClient client;
        ServerObject server; // obiectul server

        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
        }

        public void Process()
        {
            try
            {
                Stream = client.GetStream();
                // obtinem numele utilizatorului
                string message = GetMessage();
                userName = message;

                message = userName + " a intrat in chat";
                // trimitem mesajul despre intrare in chat tuturor utilizatorilor conectati
                server.BroadcastMessage(message, this.Id);
                Console.WriteLine(message);
                // intr-un ciclu infinit obtinem mesajele de la client
                while (true)
                {
                    try
                    {
                        message = GetMessage();
                        message = String.Format("{0}: {1}", userName, message);
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, this.Id);
                    }
                    catch
                    {
                        message = String.Format("{0}: a iesit din chat", userName);
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, this.Id);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                // in cazul iesirii din ciclu inchidem resurse
                server.RemoveConnection(this.Id);
                Close();
            }
        }

        // citirea mesajului de intrare si transformare a acestuia in text
        private string GetMessage()
        {
            byte[] data = new byte[64]; // un bufer pentru datele obtinute
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return cipher.Decrypt(builder.ToString(), 3);
        }

        // inchiderea conexiunii
        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }
    }
}
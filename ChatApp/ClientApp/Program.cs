using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;

namespace ClientApp
{
    public class CaesarCipher
    {
        //alfabetul englez
        const string alfabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private string CodeEncode(string text, int k)
        {
            //adaugam in alfabet literele mici
            var fullAlfabet = alfabet + alfabet.ToLower();
            var letterQty = fullAlfabet.Length;
            var retVal = "";
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                var index = fullAlfabet.IndexOf(c);
                if (index < 0)
                {
                    //daca caracterul nu e gasit, atunci il adaugam fara schimbari
                    retVal += c.ToString();
                }
                else
                {
                    var codeIndex = (letterQty + index + k) % letterQty;
                    retVal += fullAlfabet[codeIndex];
                }
            }

            return retVal;
        }

        //cifrarea textului
        public string Encrypt(string plainMessage, int key)
            => CodeEncode(plainMessage, key);

        //descifrarea textului
        public string Decrypt(string encryptedMessage, int key)
            => CodeEncode(encryptedMessage, -key);
    }
    class Program
    {
        static string userName;
        private const string host = "127.0.0.1";
        private const int port = 8888;
        static TcpClient client;
        static NetworkStream stream;
        static CaesarCipher cipher = new CaesarCipher();
        static void Main(string[] args)
        {
            Console.Write("Introduceti nume: ");
            userName = Console.ReadLine();
            client = new TcpClient();
            try
            {
                client.Connect(host, port); //conectarea clientului
                stream = client.GetStream(); // obtinem flux

                string message = userName;
                string codingMessage = cipher.Encrypt(message, 3);
                byte[] data = Encoding.Unicode.GetBytes(codingMessage);
                stream.Write(data, 0, data.Length);

                // lansam un nou flux pentru receptionarea datelor
                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start(); //start fluxului
                Console.WriteLine("Bine ati venit, {0}", userName);
                SendMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Disconnect();
            }
        }
        // trimiterea mesajelor
        static void SendMessage()
        {
            Console.WriteLine("Introduceti mesajul: ");

            while (true)
            {
                string message = Console.ReadLine();
                string codingMessage = cipher.Encrypt(message, 3);
                byte[] data = Encoding.Unicode.GetBytes(codingMessage);
                stream.Write(data, 0, data.Length);
            }
        }
        // receptionarea mesajelor
        static void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64]; // buferul pentru datele receptionate
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = cipher.Decrypt(builder.ToString(), 3);
                    Console.WriteLine(message);//afisarea mesajului
                }
                catch
                {
                    Console.WriteLine("Conexiunea a esuat!");
                    Console.ReadLine();
                    Disconnect();
                }
            }
        }

        static void Disconnect()
        {
            if (stream != null)
                stream.Close();//deconectarea fluxului
            if (client != null)
                client.Close();//deconectarea clientului
            Environment.Exit(0); //inchiderea procesului
        }
    }
}
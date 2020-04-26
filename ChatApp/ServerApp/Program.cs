using System;
using System.Threading;

namespace ServerApp
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
        static ServerObject server; // serverul
        static Thread listenThread; // fluxului pentru ascultare
        static void Main(string[] args)
        {
            try
            {
                server = new ServerObject();
                listenThread = new Thread(new ThreadStart(server.Listen));
                listenThread.Start(); //startul fluxului
            }
            catch (Exception ex)
            {
                server.Disconnect();
                Console.WriteLine(ex.Message);
            }
        }
    }
}
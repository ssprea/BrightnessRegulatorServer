namespace ServerBrightnnes
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    class Program
    {
        /// <summary>
        /// Classe Main asincrona per l'esecuzione di tutto il server
        /// </summary>
        static async Task Main(string[] args)
        {
            PhysicalMonitorController control = new PhysicalMonitorController();
            List<int> values = new List<int>();
            List<int> values2 = new List<int>();

            int port = 45743; // Porta del server
            TcpListener server = new TcpListener(IPAddress.Any, port);
            NetworkStream stream;

            server.Start();
            Console.WriteLine($"Server in ascolto sulla porta {port}...");

            while (true)
            {
                var client = await server.AcceptTcpClientAsync();
                Console.WriteLine("Client connesso!\n");
                stream = client.GetStream();

                try
                {
                    // Invia i valori iniziali di luminosità e contrasto
                    values = RetriveSettings(control);
                    SendSettings(values, stream);

                    // Ciclo per ricevere ed elaborare dati dal client
                    while (client.Connected)
                    {
                        values2 = await ReceiveSettings(stream);
                        ApplySettings(control, values2);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Errore nella gestione del client.\n");
                }
            }

        }


        /// <summary>
        /// Prende le informazioni base sui valori luminosità e contrasto dal PhysicalMonitorBrightnessController
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        static List<int> RetriveSettings(PhysicalMonitorController controller)
        {
            List<int> values = new List<int>();
            values.Add(controller.GetBrightness());
            values.Add(controller.GetContrast());
            return values;
        }

        /// <summary>
        /// ???
        /// </summary>
        /// <param name="valuesToSend"></param>
        /// <param name="clientStream"></param>
        static void SendSettings(List<int> valuesToSend, NetworkStream clientStream)
        {
            // Serializzazione in JSON
            string json = JsonSerializer.Serialize(valuesToSend);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(json);

            // Invio della lunghezza del messaggio e dei dati
            clientStream.Write(BitConverter.GetBytes(data.Length), 0, 4); // 4 byte per la lunghezza
            clientStream.Write(data, 0, data.Length);

            Console.WriteLine("Dati inviati al client.\n");
        }


        /// <summary>
        /// ??? 
        /// </summary>
        /// <param name="valuesToReceive"></param>
        /// <param name="clientStream"></param>
        /// <returns></returns>
        static async Task<List<int>> ReceiveSettings(NetworkStream clientStream)
        {
            // Leggi la lunghezza del messaggio
            byte[] lengthBuffer = new byte[4];
            await clientStream.ReadAsync(lengthBuffer, 0, 4);
            int dataLength = BitConverter.ToInt32(lengthBuffer, 0);

            // Leggi i dati
            byte[] data = new byte[dataLength];
            await clientStream.ReadAsync(data, 0, dataLength);
            string jsonString = Encoding.UTF8.GetString(data);

            // Deserializza la lista di interi
            return JsonSerializer.Deserialize<List<int>>(jsonString);
        }



        /// <summary>
        /// ???
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="valuesToApply"></param>
        static void ApplySettings(PhysicalMonitorController controller, List<int> valuesToApply)
        {
            controller.SetBrightness((uint)valuesToApply.First());
            controller.SetContrast((uint)valuesToApply.Last());
        }
    }

}

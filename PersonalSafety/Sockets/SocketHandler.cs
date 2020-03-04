using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using Newtonsoft.Json;

namespace PersonalSafety.Sockets
{
    public static class SocketHandler
    {

        static TcpListener server = null;

        static FunctionLauncher fl = new FunctionLauncher();

        public static void Start(object obj)
        {

            int port = (int)obj;
            IPAddress localAddr = IPAddress.Parse("0.0.0.0");
            server = new TcpListener(localAddr, port);
            server.Start();
            GlobalVar.Set("networkstreams", new List<NetworkStream>());
            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for a new client connection...");
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");
                    Thread t = new Thread(new ParameterizedThreadStart(HandleDevice));
                    t.Start(client);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                server.Stop();
            }
        }

        public static void HandleDevice(object obj)
        {
            TcpClient client = (TcpClient)obj;
            var stream = client.GetStream();
            GlobalVar.Get<List<NetworkStream>>("networkstreams", new List<NetworkStream>()).Add(stream); //Add to list of connected clients
            string imei = string.Empty;
            string data = null;
            byte[] bytes = new byte[1024];
            int i;
            try
            {
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    string hex = BitConverter.ToString(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, i);
                    Console.WriteLine("{1}: Received: {0}", data, Thread.CurrentThread.ManagedThreadId);
                    Console.WriteLine("Parsing JSON ..");
                    Console.WriteLine("JSON to parse: " + data);

                    SocketMessage message = JsonConvert.DeserializeObject<SocketMessage>(data);

                    Console.WriteLine("Parsed json message type: " + message.type);

                    fl.LaunchFunctionByName(message.type, message.parameters);

                    Console.WriteLine("Replying back to all clients..");

                    BroadcastMessageToAllConnectedClients(data);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.ToString());
                client.Close();
            }
        }

        public static void BroadcastMessageToAllConnectedClients(string message)
        {

            byte[] encodedMessage = Encoding.ASCII.GetBytes(message);

            List<NetworkStream> clientStreams = GlobalVar.Get<List<NetworkStream>>("networkstreams");

            int iter = 0;

            foreach(NetworkStream ns in clientStreams)
            {

                try
                {

                    ns.Write(encodedMessage, 0, encodedMessage.Length);
                    iter++;
                    
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Couldn't send message to client. Exception: " + ex.ToString());
                }

            }

            Console.WriteLine("Sent to " + iter + " clients.");

        }
    }

    public class SocketMessage
    {

        public string type; //name of function to call

        public Dictionary<string, string> parameters; //key is parameter name, value is parameter value

    }

    public class FunctionLauncher
    {

        public void LaunchFunctionByName(string functionName, params Dictionary<string, string>[] parameters)
        {

            Type thisType = this.GetType();
            MethodInfo theMethod = thisType.GetMethod(functionName);
            theMethod.Invoke(this, parameters);

        }

        public void SHARELOCATION(Dictionary<string, string> TESTPARAM)
        {

            foreach (KeyValuePair<string, string> kvp in TESTPARAM)
                Console.WriteLine("Key is: " + kvp.Key + ", Value is: " + kvp.Value);

        }

    }
}
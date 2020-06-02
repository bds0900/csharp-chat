using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    class Program
    {
        static int PORT = 7000;
        static List<ClientInfo> clients;

        static void Main(string[] args)
        {
            /*Server server = new Server();
            server.Run().Wait();*/

            StartListening();
        }
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public static void StartListening()
        {
            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            // running the listener is "host.contoso.com".  
            string server = Dns.GetHostName();
            Console.WriteLine("Using current host: " + server);

            IPHostEntry heserver = Dns.GetHostEntry(server);
            List<IPAddress> serverIP = new List<IPAddress>();
            Console.WriteLine("Select your server IP");
            int i = 1;
            foreach (IPAddress curAdd in heserver.AddressList)
            {
                if (curAdd.AddressFamily == AddressFamily.InterNetwork)
                {
                    Console.WriteLine(i.ToString() + ". Address: " + curAdd.ToString());
                    serverIP.Add(curAdd);
                    i++;
                }
            }
            int select = Convert.ToInt32(Console.ReadLine()) - 1;


            /*IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];*/
            IPEndPoint localEndPoint = new IPEndPoint(serverIP[select], PORT);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            clients = new List<ClientInfo>();

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            Console.WriteLine("{0} is joining...", handler.RemoteEndPoint);
            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = handler;

            // accept한 소켓을 리스트에 넣어서 보관하자
            //clients.Add(handler);

            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);

        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket.
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.  
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read
                // more data.  
                content = state.sb.ToString();
                if (content.IndexOf("<SOM>") > -1)
                {
                    string id = content.Substring(0, content.IndexOf("<SOM>"));
                    var remote = handler.RemoteEndPoint.ToString();
                    Console.WriteLine("add client");
                    clients.Add(new ClientInfo { client = handler, IP = remote, ID = id, UserName = id });
                    
                    state.sb.Clear();
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                }
                else if (content.IndexOf("<EOM>") > -1)
                {
                    //만약 EOM이 있으면 클라이언트는 더이상 메세지를 보내지 않겠다는 의미, socket을 close한다
                    Console.WriteLine("{0} exit", handler.RemoteEndPoint);
                    foreach (ClientInfo client in clients)
                    {
                        if(client.client== handler)
                        {
                            clients.Remove(client);
                            break;
                        }
                    }
                    
                    //Disable both sending and receiving on this Socket
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
                else if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the
                    // client. Display it on the console.  
                    Console.WriteLine("Read {0} bytes from socket {1}", content.Length, handler.RemoteEndPoint);
                    Console.WriteLine("Data : {0}", content);

                    // Echo the data back to the client.  
                    // Send(handler, content);

                    Console.WriteLine("broad casting to all clients");
                    // 모든 handler에게 broadcasting하자
                    string user = "";
                    foreach (ClientInfo client in clients)
                    {
                        if(client.client==handler)
                        {
                            user = client.UserName;
                        }
                    }
                    foreach (ClientInfo client in clients)
                    {
                        Console.WriteLine("send to {0}", client.client.RemoteEndPoint);
                        Send(client.client, user + " : "+ content.Substring(0, content.IndexOf("<EOF>")));
                    }

                    // 메세지를 clear하고
                    state.sb.Clear();
                    // 한번 읽음이 끝나면 다시 receive해서 메세지를 받자
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                }
                else
                {
                    // Not all data received. Get more.  
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }           

        }

        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client {1}.", bytesSent, handler.RemoteEndPoint);

                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

    public class StateObject
    {
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 24;//8192
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }
    public class ClientInfo
    {
        public Socket client { set; get; }
        public string IP { set; get; }
        public string ID { set; get; }
        public string UserName { set; get; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Chat
{
    /*class Client
    {
        private static readonly SynchronizationContext _syncContext;
        private static TextBlock _chatBx;
        private static Socket sock;
        private static int MAX_SIZE = 8192;

        // ManualResetEvent instances signal completion.  
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);


        public Client(string ip, int port,TextBlock chatBx)
        {
            _chatBx = chatBx;
            _syncContext = SynchronizationContext.Current;
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // (2) 서버에 연결
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ip), port);
            sock.BeginConnect(ep, new AsyncCallback(ConnectCallback), sock);
            connectDone.WaitOne();
            //sock.Connect(ep);
        }
        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.  
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private static void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public async Task Send(string message)
        {
            byte[] buff = Encoding.UTF8.GetBytes(message);
            // (3) 서버에 데이타 전송
            //sock.Send(buff, SocketFlags.None);
            await Task.Factory.FromAsync(
                            sock.BeginSend(buff, 0, buff.Length, SocketFlags.None, null, sock),
                            sock.EndSend);

        }
        public async Task Receive()
        {
            var buff = new byte[MAX_SIZE];
            int received = 0;
            while(true)
            {
                received = await Task.Factory.FromAsync<int>(
                            sock.BeginReceive(buff, 0, buff.Length, SocketFlags.None, null, sock),
                            sock.EndReceive);

                //sock.BeginReceive(buff, 0, buff.Length, SocketFlags.None, new AsyncCallback(Receive), sock);
                _syncContext.Post((object state) => _chatBx.Text = Encoding.UTF8.GetString(buff, 0, buff.Length), _syncContext);
            }
            

        }
        public void Start()
        {
            var buff = new byte[MAX_SIZE];
            //sock.BeginReceive(buff, 0, buff.Length, SocketFlags.None, new AsyncCallback(Receive), null);
            _syncContext.Post((object state) => _chatBx.Text = "Start program", _syncContext);
            Thread receivingThread = new Thread(async () => await Receive());
            //Thread sendingThread = new Thread(async () => await Send());

        }
        private static void Receive(Socket client)
        {
            try
            {
                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.  
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.  
                    if (state.sb.Length > 1)
                    {
                        //response = state.sb.ToString();
                        _syncContext.Post((object state) => _chatBx.Text = state.sb.ToString(), _syncContext);
                    }
                    // Signal that all bytes have been received.  
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public void End()
        {

            try
            {
                sock.Shutdown(SocketShutdown.Both);
            }
            finally
            {
                sock.Close();
            }
            
           
        }
    }
*/

}

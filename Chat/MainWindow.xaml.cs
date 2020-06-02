using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Chat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static SynchronizationContext syncContext= SynchronizationContext.Current;
        private static Socket client;
        private static int MAX_SIZE = 8192;

        // ManualResetEvent instances signal completion.  
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);


        public MainWindow()
        {
            InitializeComponent();
            sendBtn.IsEnabled = false;
            Closing += new CancelEventHandler(OnWindowClosing);

        }

        private void sendBtn_Click(object sender, RoutedEventArgs e)
        {
            //Client.Send(new TextRange(msgBx.Document.ContentStart, msgBx.Document.ContentEnd).Text).Wait();
            Send(client, new TextRange(msgBx.Document.ContentStart, msgBx.Document.ContentEnd).Text);
            sendDone.WaitOne();

        }

        private void connect_Click(object sender, RoutedEventArgs e)
        {
            if(idBx.Text=="")
            {
                MessageBox.Show("Your ID");
            }
            else if(ipBx.Text=="")
            {
                MessageBox.Show("Server IP");
            }
            else if(portBx.Text=="")
            {
                MessageBox.Show("Server Port Num");
            }
            else
            {
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ipBx.Text), Convert.ToInt32(portBx.Text));
                client.BeginConnect(ep, new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne();

                connectBtn.IsEnabled = false;
                sendBtn.IsEnabled = true;
                idBx.IsEnabled = false;
                ipBx.IsEnabled = false;
                portBx.IsEnabled = false;

                Send(client, idBx.Text+"<SOM>");
                sendDone.WaitOne();
            }
            
        }
        private void ConnectCallback(IAsyncResult ar)
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

                // connection done. start receive
                while(true)
                {
                    Receive(client);
                    receiveDone.WaitOne();
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
  
        }
        private void Receive(Socket client)
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

        private void ReceiveCallback(IAsyncResult ar)
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
                    var response = state.sb.ToString();
                    syncContext.Post((object state) => chatBx.Text = response, syncContext);
                    // Get the rest of the data.  
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.  
                    if (state.sb.Length > 1)
                    {
                        var response = state.sb.ToString();
                        
                        syncContext.Post((object state) => chatBx.Text = response, syncContext);
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

        private static void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data+ "<EOF>");

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

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {

            if (client != null)
            {
                // Handle closing logic, set e.Cancel as needed
                try
                {
                    // end of message 를 보낸다
                    byte[] byteData = Encoding.ASCII.GetBytes("<EOM>");
                    client.BeginSend(byteData, 0, byteData.Length, 0,
                        new AsyncCallback(SendCallback), client);
                }
                catch(SocketException ex)
                {
                    
                }
                finally
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }
            }
                
        }
    }



    public class StateObject
    {
        // Client socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 256;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }
}

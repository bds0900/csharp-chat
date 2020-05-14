using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Server
    {
        Socket sock;
        List<Socket> clients;
        static int PORT = 7000;
        static int MAX_SIZE = 8192;
        public Server()
        {
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
                    Console.WriteLine(i.ToString()+". Address: " + curAdd.ToString());
                    serverIP.Add(curAdd);
                    i++;
                }
            }
            int select = Convert.ToInt32(Console.ReadLine()) - 1;



            //소켓만들기, 일반소켓이나 비동기 소켓이나 똑같음
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            IPEndPoint ep = new IPEndPoint(serverIP[select], PORT);
            sock.Bind(ep);
        }
        public async Task Run()
        {
            clients = new List<Socket>();
            sock.Listen(10);

            while(true)
            {
                //accpet client
                Socket clientSock = await Task.Factory.FromAsync(sock.BeginAccept, sock.EndAccept, null);
                clients.Add(clientSock);

                //handle socket
                Thread client = new Thread(async () => await SocketHandlerAsync(clientSock));
                client.Start();
                

                if (clients.Count == 0)
                {
                    break;
                }
            }


        }
        public async Task SocketHandlerAsync(Socket clientSock)
        {
            while(true)
            {
                //receive msg
                //int nCount= await ReceiveMsg(clientSock);
                //await SendMsg(nCount);
                var buff = new byte[MAX_SIZE];
                int nCount = await Task.Factory.FromAsync<int>(
                            clientSock.BeginReceive(buff, 0, buff.Length, SocketFlags.None, null, clientSock),
                            clientSock.EndReceive);
                Console.WriteLine("receive message: " + Encoding.ASCII.GetString(buff, 0, nCount));

                if (nCount > 0)
                {
                    string msg = Encoding.ASCII.GetString(buff, 0, nCount);
                    

                    // braodcasting
                    foreach (var client in clients)
                    {
                        await Task.Factory.FromAsync(
                            client.BeginSend(buff, 0, buff.Length, SocketFlags.None, null, client),
                            client.EndSend);
                        Console.WriteLine("send message: " + msg);
                    }

                }
                //client가 없으면 핸들러를 종료한다
                if (clients.Count == 0)
                {
                    break;
                }



            }

        }
        public void ClientManager()
        {

        }



        public async Task<int> ReceiveMsg(Socket clientSock)
        {
            var buff = new byte[MAX_SIZE];
            int nCount = await Task.Factory.FromAsync<int>(
                        clientSock.BeginReceive(buff, 0, buff.Length, SocketFlags.None, null, clientSock),
                        clientSock.EndReceive);
            Console.WriteLine("receive message: "+Encoding.ASCII.GetString(buff, 0, nCount));
            return nCount;
        }
        public async Task SendMsg(int nCount)
        {
            var buff = new byte[MAX_SIZE];
            //send msg to everyone
            if (nCount > 0)
            {
                string msg = Encoding.ASCII.GetString(buff, 0, nCount);
                Console.WriteLine(msg);

                // braodcasting
                foreach (var client in clients)
                {
                    await Task.Factory.FromAsync(
                        client.BeginSend(buff, 0, buff.Length, SocketFlags.None, null, client),
                        client.EndSend);
                }

            }
        }


    }
}

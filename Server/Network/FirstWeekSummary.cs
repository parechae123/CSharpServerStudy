using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Net;
using Google.Protobuf.Protocol;
using Google.Protobuf;
using ProtoBuf.Meta;
using System.Threading.Channels;
using Grpc.Core;
using Grpc.Net.Client;

namespace CSharpServerStudy.Server.Network
{
    
    internal class FirstWeekSummary
    {

        private static readonly GrpcChannel Channel = GrpcChannel.ForAddress("http://127.0.0.1:50051");

        // 정적 클라이언트 인스턴스
        private static readonly ServiceTest.ServiceTestClient Client = new ServiceTest.ServiceTestClient(Channel);


        private int _port;
        private TcpListener _listener;
        private GetRequest _request;
        public TcpListener Listener
        {
            get { return _listener; }
        }
        public FirstWeekSummary(int port)
        {
            _port = port;
            _listener = new TcpListener(IPAddress.Any, _port);
        }

        public void Start()
        {   
            
            Listener.Start();
            Listener.BeginAcceptTcpClient(ConnectionSuccess, null);

        }

        private void ConnectionSuccess(IAsyncResult result)
        {
            TcpClient client = Listener.EndAcceptTcpClient(result);
            //다음 연결을 위한 재귀호출
            Listener.BeginAcceptTcpClient(ConnectionSuccess, null);

            Console.WriteLine(client.Client.RemoteEndPoint+"와 연결되었습니다.");
            HandleClient(client);
        }

        public void HandleClient(TcpClient client)
        {
            byte[] buffer = new byte[1024];
            NetworkStream stream = client.GetStream();
            try
            {
                while (client.Connected)
                {
                    int bufferRead = stream.Read(buffer, 0, buffer.Length);
                    string msg = Encoding.UTF8.GetString(buffer, 0, bufferRead);
                    Console.WriteLine($"{msg}");
                    User tempUser = new User() { UserID = msg, Token = "dd", Ip = "00" };
                    Console.WriteLine($"{tempUser.Ip + tempUser.UserID + tempUser.Token}");
                    //tempUser.WriteTo(stream);

                    tempUser.WriteDelimitedTo(stream);
                    //output.Flush();
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"클라이언트 연결 종료 : {ex}");
                
            }
            catch (SocketException ex)
            { 
                Console.WriteLine("소켓 예외 발생 : "+ex );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Execption오류 : {ex}");
            }
            finally
            {
                stream.Close();
                stream.Dispose();
            }
            //client.Close();
        }
    }
}



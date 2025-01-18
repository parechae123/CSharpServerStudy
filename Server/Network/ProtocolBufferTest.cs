using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Protocol;

namespace CSharpServerStudy.Server.Network
{
    public class ProtocolBufferTest
    {
        private readonly int _port;
        
        public ProtocolBufferTest(int port)
        {
            _port = port;
        }

        public async Task Start()
        {
            TcpListener listener = new TcpListener(IPAddress.Any,_port);
            listener.Start();
            Console.WriteLine($"포트번호 {_port}에서 실행 중");
            
            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                Console.WriteLine($"클라이언트 연결 됨{client.Client.RemoteEndPoint}");
                _ = Task.Run(() =>ClientConnection(client));
            }
        }

        private async Task ClientConnection(TcpClient client)
        {
            try
            {
                var stream = client.GetStream();
                var buffer = new byte[1024];
                while(true)
                {
                    byte[] send = Encoding.UTF8.GetBytes("이름을 입력해주세요");
                    stream.Write(send, 0, send.Length);
                    int byteRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    User user = new User { UserID = Encoding.UTF8.GetString(buffer, 0, byteRead), Token = string.Empty, Ip = client.Client.RemoteEndPoint.ToString() ?? string.Empty };
                    Console.WriteLine($"유저이름{user.UserID}토큰{user.Token}아이피{user.Ip}");
                    user.WriteTo(stream);
                    stream.Flush();
                }

            }
            catch (Exception)
            {

                throw;
            }
            finally 
            { 
                //client.Close();
            }
        }

    }
}

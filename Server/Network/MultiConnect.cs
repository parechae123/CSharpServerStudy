using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CSharpServerStudy.Server.Network
{
    internal class MultiConnect
    {
        private readonly int _port;
        private Dictionary<string,NetworkStream> users = new Dictionary<string, NetworkStream>();
        public MultiConnect(int port)
        {
            _port = port;
        }

        public async Task StartAsync()
        {
            var listener = new TcpListener(IPAddress.Any, _port);
            listener.Start();
            Console.WriteLine($"서버가 포트 {_port}에서 실행 중입니다.");

            while (true)
            {
                // 비동기적으로 클라이언트 접속 대기
                var client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("클라이언트 연결됨!");

                // 클라이언트 처리 작업을 백그라운드에서 실행
                //_= : 반환값을 무시(저장안)한다는걸 명시적으로 표현
                _ = Task.Run(() => HandleClientAsync(client));
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            //try 도중 오류발생 => catch => finally 실행
            //오류 x try => finally
            try
            {
                var stream = client.GetStream();
                var buffer = new byte[1024];
                string userName = string.Empty;
                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    // 받은 데이터 처리
                    userName = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    if (users.ContainsKey(userName))
                    {
                        _ = stream.WriteAsync(new byte[1]{0}, 0, 1);
                    }
                    else
                    {
                        users.Add(userName,stream);
                        _ = stream.WriteAsync(new byte[1] { 1 }, 0, 1);
                        break;
                    }
                }

                // 클라이언트와의 통신 루프
                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    // 받은 데이터 처리
                    var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"{userName} : {message}");

                    // Echo back (받은 데이터를 다시 클라이언트로 전송)
                    var response = Encoding.UTF8.GetBytes($"{userName} : {message}");
                    foreach ( var item in users )
                    {
                        if (item.Key != userName)
                        {
                            await item.Value.WriteAsync(response, 0, response.Length);
                        }
                        else continue;
                    }
                    if (message[0] == '.')
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"오류: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("클라이언트 연결 종료");
                client.Close();
            }
        }
    }
}

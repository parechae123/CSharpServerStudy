using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CSharpServer.Network
{
    public class Server
    {
        private readonly int _port;
        //생성자, 서버 생성 시 포트를 기입하여 포트를 설정
        public Server(int port)
        {
            _port = port;
        }

        //서버 실행 및 클라이언트 callback을 처리하고 있는 함수
        public void Start()
        {
            //TCP리스너를 현재 ip와 생성자에서 지정된 port로 생성
            var listner = new TcpListener(IPAddress.Any, _port);
            //들어오는 접속 요청을 리스닝해줌
            listner.Start();
            //단순한 커스텀 메세지 함수
            ServerMSG($"서버가 실행되었습니다.");
            ServerMSG($"Listening on port{_port}");
            //프로그램이 실행되고 있는 동안(program의 main함수가 종료되기 전까지 계속 받아옴)
            while(true)
            {
                //대기중인 접속요청들을 수락합니다.
                var client = listner.AcceptTcpClient();
                ServerMSG($"Listening on port{_port}");
                //소켓 스트림을 반환하는 함수,현재 연결되어있는 클라이언트들의 소켓 스트림을 반환한다
                //* 스트림이란? 데이터를 순차적으로 읽거나 쓸 수 있는 방식
                //* 소켓 스트림이란? 네트워크를 통해 데이터를 송수신하는 방식
                var stream = client.GetStream();
                //앞선 스트림 처리가 지연되었을때 그 이전의 스트림(입력)을 buffer의 배열에 넣음 즉 buffer.length = 스트림 저장 갯수
                var buffer = new byte[10];
                //스트림 데이터를 읽어오는 함수, buffer 배열에 저장하고 0번째 배열부터 buffer.length 까지 읽어온다는 뜻
                //데이터는 buffer에 쓰여지고 해당 함수는 최종적으로 buffer에 쓰여진 데이터 갯수를 int 값으로 리턴한다
                var bytesRead = stream.Read( buffer, 0, buffer.Length );

                //읽어온 스트림 데이터를 UTF8 형식으로 string 변환해주는 함수, 매개변수는 
                var message = Encoding.UTF8.GetString( buffer, 0, bytesRead );
                ServerMSG($"Recieved : {message}" );
                //클라이언트를 종료한다
                client.Close();
            }
        }

        public void ServerMSG(string MSG)
        {
            Console.WriteLine($"[Server] : {MSG}");
        }
    }
}
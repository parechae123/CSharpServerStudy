using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CSharpServer.Network
{
    public class Server
    {
        private readonly int _port;
        //������, ���� ���� �� ��Ʈ�� �����Ͽ� ��Ʈ�� ����
        public Server(int port)
        {
            _port = port;
        }

        //���� ���� �� Ŭ���̾�Ʈ callback�� ó���ϰ� �ִ� �Լ�
        public void Start()
        {
            //TCP�����ʸ� ���� ip�� �����ڿ��� ������ port�� ����
            var listner = new TcpListener(IPAddress.Any, _port);
            //������ ���� ��û�� ����������
            listner.Start();
            //�ܼ��� Ŀ���� �޼��� �Լ�
            ServerMSG($"������ ����Ǿ����ϴ�.");
            ServerMSG($"Listening on port{_port}");
            //���α׷��� ����ǰ� �ִ� ����(program�� main�Լ��� ����Ǳ� ������ ��� �޾ƿ�)
            while(true)
            {
                //������� ���ӿ�û���� �����մϴ�.
                var client = listner.AcceptTcpClient();
                ServerMSG($"Listening on port{_port}");
                //���� ��Ʈ���� ��ȯ�ϴ� �Լ�,���� ����Ǿ��ִ� Ŭ���̾�Ʈ���� ���� ��Ʈ���� ��ȯ�Ѵ�
                //* ��Ʈ���̶�? �����͸� ���������� �аų� �� �� �ִ� ���
                //* ���� ��Ʈ���̶�? ��Ʈ��ũ�� ���� �����͸� �ۼ����ϴ� ���
                var stream = client.GetStream();
                //�ռ� ��Ʈ�� ó���� �����Ǿ����� �� ������ ��Ʈ��(�Է�)�� buffer�� �迭�� ���� �� buffer.length = ��Ʈ�� ���� ����
                var buffer = new byte[10];
                //��Ʈ�� �����͸� �о���� �Լ�, buffer �迭�� �����ϰ� 0��° �迭���� buffer.length ���� �о�´ٴ� ��
                //�����ʹ� buffer�� �������� �ش� �Լ��� ���������� buffer�� ������ ������ ������ int ������ �����Ѵ�
                var bytesRead = stream.Read( buffer, 0, buffer.Length );

                //�о�� ��Ʈ�� �����͸� UTF8 �������� string ��ȯ���ִ� �Լ�, �Ű������� 
                var message = Encoding.UTF8.GetString( buffer, 0, bytesRead );
                ServerMSG($"Recieved : {message}" );
                //Ŭ���̾�Ʈ�� �����Ѵ�
                client.Close();
            }
        }

        public void ServerMSG(string MSG)
        {
            Console.WriteLine($"[Server] : {MSG}");
        }
    }
}
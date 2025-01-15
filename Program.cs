// See https://aka.ms/new-console-template for more information
using CSharpServer.Network;
using CSharpServerStudy.Server.Network;

//Console.WriteLine(클래스 선언을 따로 하지 않아도 구문은 실행된다 신기하당)

//프로그램 메인 클래스, 하위 폴더에서 네트워크와 로직,파이프라인을 작업하고 여기서 최종적으로 합친다
class Program
{
    //프로그램 실행 시 무조건 실행되는 Main 함수, 프로그래머스 문제 풀때 있는 Main 그거임
    static async Task Main()
    {
        //var server = new MultiConnect(7777);
        //await server.StartAsync();
        //var server = new Server(7777);
        //server.ServerMSG("서버 실행중...");
        var server = new ProtocolBufferTest(7777);
        await server.Start();
    }
}
using CSharpServerStudy.Server.Handle;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSharpServerStudy.Server.Network
{
    //ORM은 게임 서버에서 사용하지 않음,속도떄문
    internal class DBConnectedServer
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        DBConnector con;
        public void Start()
        {
            DBConnect();
            ServerON();
        }
        public void ServerON()
        {
            // DBConnectedServer 시작 메시지 출력
            Console.WriteLine("DBConnectedServer Start");

            // 1️ Kestrel 서버 설정 (웹 서버를 수동으로 구성)
            builder.WebHost.ConfigureKestrel(options =>
            {
                // 5000번 포트에서 HTTP/2 프로토콜을 사용하여 요청을 받을 수 있도록 설정
                options.ListenLocalhost(5000, listenOptions =>
                {
                    listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
                });
            });

            // 2️ 로깅(Logging) 설정
            builder.Logging.ClearProviders();    // 기존 로깅 제공자(Providers) 제거
            builder.Logging.AddConsole();        // 콘솔 로그 출력 추가
            builder.Logging.SetMinimumLevel(LogLevel.Debug); // 최소 로그 레벨을 Debug로 설정하여 상세 로그 출력

            // 3️ gRPC 서비스 등록
            builder.Services.AddGrpc(); // gRPC 관련 서비스들을 DI(Dependency Injection) 컨테이너에 추가

            // 4️ 애플리케이션 빌드
            var app = builder.Build();

            // 5️ 요청 라우팅 설정
            app.UseRouting(); // 미들웨어를 사용하여 라우팅을 활성화

            // 6️ gRPC 엔드포인트 매핑
            app.UseEndpoints(endpoints =>
            {
                try
                {
                    // Google.Protobuf.Protocol.CustomService라는 gRPC 서비스를 엔드포인트에 매핑
                    endpoints.MapGrpcService<Google.Protobuf.Protocol.CustomService>();
                }
                catch (Exception ex)
                {
                    // gRPC 엔드포인트 매핑 과정에서 예외 발생 시 콘솔에 출력
                    Console.WriteLine($"엔드포인트 오류 : {ex.Message}");
                    throw; // 예외를 다시 던져서 애플리케이션이 알맞게 종료되도록 처리
                }
            });

            // 7️ 기본 요청 처리 (루트 URL 요청 시 응답)
            app.MapGet("/", () => "gRPC 서버가 실행 중입니다. gRPC 클라이언트를 사용해 요청하세요.");

            // 8️ 모든 요청을 처리하는 예외적인 핸들러 추가
            app.MapWhen(context => true, appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    // 콘솔에 요청 정보 출력 (context 객체 자체를 출력하는 것은 적절하지 않을 수 있음)
                    Console.WriteLine(context.Request.Path);

                    // 404 응답을 보냄 (요청이 매핑되지 않은 경우)
                    await context.Response.WriteAsync("Not Found");
                });
            });

            // 9️ 서버 실행
            app.Run(); // Kestrel 서버 실행 -> 이후부터 클라이언트의 요청을 받을 수 있음
        }
        public void DBConnect()
        {
            string[] temp = new string[] { "dbIP","port","dbName","userID","password" };
            Queue<string> serverInfo = ReadJsonConfig("ServerInfo", "DBLocation",new Queue<string>(temp));
            con = new DBConnector(serverInfo.Dequeue(), serverInfo.Dequeue(), serverInfo.Dequeue(), serverInfo.Dequeue(), serverInfo.Dequeue());
            con.DBConnect();
            con.DBDisConnect();
        }
        public Queue<string> ReadJsonConfig(string fileName,string objectName, Queue<string> instanceNames)
        {
            string jsonPath = Path.Combine(Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName, $"{fileName}.json");
            string jsonString =  File.ReadAllText(jsonPath);

            var jsonOBJ = JObject.Parse(jsonString);
            var jsonInfo = jsonOBJ[objectName];
            Queue<string> datas = new Queue<string>();
            while (instanceNames.Count > 0)
            {
                if (jsonInfo[instanceNames.Peek()] == null)
                {
                    Console.WriteLine($"{instanceNames.Peek()}해당 인스턴스가 존재하지 않습니다.");
                    break;
                }
                datas.Enqueue(jsonInfo[instanceNames.Dequeue()].ToString());
            }

            return datas;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using Grpc.AspNetCore.Server;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using System.Net;

namespace CSharpServerStudy.Server.Network
{
    internal class ChannelServer
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        public void Start()
        {

            // 1. gRPC 서비스 등록
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(5000, listenOptions =>
                {
                    listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2; // HTTP/2 설정
                });
            });
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Debug);

            builder.Services.AddGrpc();
            var app = builder.Build();
            // 2. gRPC 엔드포인트 매핑
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                try
                {
                    endpoints.MapGrpcService<Google.Protobuf.Protocol.CustomService>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"엔드포인트 오류 : {ex.Message}" );
                    throw;
                }
            });
            // 3. 기본 HTTP 요청 차단 (선택적)
            app.MapGet("/", () => "gRPC 서버가 실행 중입니다. gRPC 클라이언트를 사용해 요청하세요.");
            app.MapWhen(context => true, appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    Console.WriteLine(context);
                    await context.Response.WriteAsync("Not Found");
                });
            });
            DBConnecting();
            app.Run();
        }

        public void DBConnecting()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string connectionString = "Server=localhost,3306;Database=csharpserverstudydb;User Id=Server;Password=0000;";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    Console.WriteLine("연결 성공");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("서버의 TLS 버전 : "+ServicePointManager.SecurityProtocol);
                Console.WriteLine("서버의 TLS 버전과 대조혹은 인바운드 아웃바운드 확인 필요");
                Console.WriteLine($"DB 연결 에러 : {ex.Message}");
                Console.WriteLine("");
                throw;
            }
        }


    }
}
namespace Google.Protobuf.Protocol
{

    public class CustomService : ServiceTest.ServiceTestBase
    {
        public override Task<GetRequest> firstEvent(GetRequest request, ServerCallContext context)
        {
            Console.WriteLine("firstEvent 호출됨");
            Console.WriteLine($"클라이언트 요청 데이터: {request.RequestWord}");

            // 응답 설정
            var response = new GetRequest
            {
                RequestWord = $"서버 응답: {request.RequestWord}"
            };
            return Task.FromResult(response);
        }

        public override Task<KeyNumberReq> secondEvent(KeyNumberReq request, ServerCallContext context)
        {
            Console.WriteLine("secondEvent 호출됨");
            Console.WriteLine($"클라이언트 요청 데이터: {request.RequestWord}, {request.RequestNumb}");

            // 응답 설정
            var response = new KeyNumberReq
            {
                RequestWord = $"서버 응답: {request.RequestWord}",
                RequestNumb = request.RequestNumb + 1
            };
            return Task.FromResult(response);
        }
    }
}

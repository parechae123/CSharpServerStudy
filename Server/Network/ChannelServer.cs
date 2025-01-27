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
using System.Data.Common;
using MySql.Data.MySqlClient;

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
            string connectionString = "Server=127.0.0.1;Port=3306;Database=csharpserverstudydb;User ID=Server;Password=0000;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    Console.WriteLine("연결 성공");

                    string query = "SELECT * FROM user";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    List<(string,string,string)> data = new List<(string, string, string)>();
                    while (reader.Read())
                    {
                        (string,string,string) tempData = (reader["index"].ToString(), reader["id"].ToString(), reader["password"].ToString());
                        data.Add(tempData);
                    }

                    reader.Close();
                    // 데이터 처리
                    foreach (var item in data)
                    {
                        Console.WriteLine($"index : {item.Item1}, id : {item.Item2}, password : {item.Item3 }");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DB 연결 에러: {ex.Message}");
                    throw;
                }
                finally
                {
                    connection.Close();
                    Console.WriteLine("DB 연결 종료");
                }
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

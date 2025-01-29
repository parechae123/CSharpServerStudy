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
using CSharpServerStudy.Server.Handle;

namespace CSharpServerStudy.Server.Network
{
    internal class ChannelServer
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        DBConnector con;
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
            if (con == null)
            {
                con = new DBConnector("127.0.0.1", "3306", "csharpserverstudydb", "Server", "0000");
            }
            using (MySqlConnection conn = new MySqlConnection(con.connectingString))
            {

                try
                {
                    conn.Open();
                    Console.WriteLine("DB 연결 성공");
                    con.conn = conn;
                    DataRequest<(string, string, string)>("user", "id", "213", (isSuccess, data) =>
                    {

                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DB 연결 에러: {ex.Message}");
                    throw;
                }
                finally
                {
                    con.DBDisConnect();
                    Console.WriteLine("DB 연결 종료");
                }
            }
        }

        public void DataRequest<T>(string requestTable,string columnName,string dataName, Action<bool, T> action)
        {
            string query = $"SELECT * FROM {requestTable}";
            Console.WriteLine(query);
            //string query = $"SELECT * FROM {requestTable} WHERE {columnName} = @{dataName}";
            //string query = $"SELECT * FROM {requestTable} WHERE {columnName} = @userId";
            using (MySqlCommand command = new MySqlCommand(query, con.conn))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    try
                    {
                        Console.WriteLine($"데이터 존재 : " + reader.HasRows);
                        Console.WriteLine($"행의 길이 : " + reader.FieldCount);
                        while (reader.Read())
                        {
                            //for문으로 fieldcount 돌리고 그걸 list나 linkedList로 만들어서 파싱해주면 어떰?
                            Console.WriteLine($"{reader[0]},{reader[1]},{reader[2]}");
                        }
                        Console.WriteLine($"리더  : " + reader.IsClosed);
                        /*for (int i = 0; i < reader.Depth; i++)
                        {
                            reader.
                        }*/
                        action.Invoke(true, default);

                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine(ex.Message);

                        action.Invoke(false, default);
                        throw;
                    }
                    finally
                    {
                        
                        
                    }
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using Grpc.AspNetCore.Server;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpServerStudy.Server.Network
{
    internal class ChannelServer
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        public void Start()
        {

            // 1. gRPC 서비스 등록
            builder.Services.AddGrpc();

            var app = builder.Build();

            // 2. gRPC 엔드포인트 매핑
            app.MapGrpcService<CustomService>();

            // 3. 기본 HTTP 요청 차단 (선택적)
            app.MapGet("/", () => "gRPC 서버가 실행 중입니다. gRPC 클라이언트를 사용해 요청하세요.");

            app.Run();
        }
    }
}

public class CustomService : ServiceTest.ServiceTestBase
{
    public override Task<GetRequest> firstEvent(GetRequest request, ServerCallContext context)
    {
        Console.WriteLine($"클라이언트 {context.RequestHeaders.FirstOrDefault(x => x.Key == "x-forwarded-for")?.Value ?? context.Peer}가 요청을 보냈습니다.");
        return base.firstEvent(request, context);
    }
}

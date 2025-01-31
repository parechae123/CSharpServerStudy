using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using Grpc.Core;

namespace CSharpServerStudy.Server.Handle
{
    public class UserInfo
    {
        string id;
        public string ID{get{return id;} }
        TcpClient client;
        public TcpClient Client { get { return client; } }
        public UserInfo(string id,TcpClient client)
        {
            this.id = id;
            this.client = client;
        }
    }
    public class UserInfoRequest : DBRequest.DBRequestBase
    {
        public override Task<GetUserDB> GetUserInfo(GetRequest request, ServerCallContext context)
        {
            GetUserDB response = new GetUserDB()
            {
                
            };
            return Task.FromResult(response);
        }
    }
}

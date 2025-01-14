using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
}

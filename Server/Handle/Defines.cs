using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using Grpc.Core;
using Newtonsoft.Json.Linq;
using System.Security.AccessControl;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MySqlX.XDevAPI.Common;
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
        string GetServerPath
        {
            get
            {
                if (Serverpath == string.Empty)
                {
                    Console.WriteLine("서버패스 생성"); 
                    string jsonPath = Path.Combine(Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName, $"ServerInfo.json");
                    string jsonString = File.ReadAllText(jsonPath);

                    var jsonOBJ = JObject.Parse(jsonString);
                    var jsonInfo = jsonOBJ["ServerString"];
                    Serverpath = jsonInfo["path"].ToString();
                }

                return Serverpath;
            }
        }
        string Serverpath = string.Empty;
        MySqlConnection con;
        MySqlConnection Con
        {
            get
            {
                if (con == null)
                {
                    con = new MySqlConnection(GetServerPath);
                }
                return con;
            }
        }
        public override Task<GetUserDB> GetUserInfo(GetRequest request, ServerCallContext context)
        {
            Con.Open();
            string query = "SELECT * FROM `user` WHERE id = @id";
            Console.WriteLine(query);
            //string query = $"SELECT * FROM {requestTable} WHERE {columnName} = @{dataName}";
            //string query = $"SELECT * FROM {requestTable} WHERE {columnName} = @userId";
            GetUserDB response = new GetUserDB()
            {
                Index = -1,
                Id = "null",
                Pw = "null"
            };
            using (MySqlCommand command = new MySqlCommand(query, Con))
            {
                command.Parameters.AddWithValue("@id", request.RequestWord);
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
                        response.Index = reader.GetInt32(0);
                        response.Id = reader.GetString(1);
                        response.Pw = reader.GetString(2);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        throw;
                    }
                }
            }
            con.Close();
            return Task.FromResult(response);
        }
        public override Task<TryResults> DeleteUser(IDSet request, ServerCallContext context)
        {
            Con.Open();
            string query = "DELETE FROM `user` WHERE id = @id AND password = @password";
            Console.WriteLine(query);
            //string query = $"SELECT * FROM {requestTable} WHERE {columnName} = @{dataName}";
            //string query = $"SELECT * FROM {requestTable} WHERE {columnName} = @userId";
            TryResults response = new TryResults()
            {
                Result = false,
                ErrorText = "아이디가 존재하지 않거나 ,비밀번호가 일치하지 않습니다."
            };
            using (MySqlCommand command = new MySqlCommand(query, Con))
            {
                command.Parameters.AddWithValue("@id", request.Id);
                command.Parameters.AddWithValue("@password", request.Password);
                command.ExecuteNonQuery();
                response.Result = true;
                response.ErrorText = "삭제 완료";
            }
            con.Close();
            return Task.FromResult(response);
        }
        public override Task<TryResults> RegistUser(IDSet request, ServerCallContext context)
        {
            Con.Open();
            string query = "INSERT INTO user (id,password) VALUES (@id,@password)";
            Console.WriteLine(query);
            //string query = $"SELECT * FROM {requestTable} WHERE {columnName} = @{dataName}";
            //string query = $"SELECT * FROM {requestTable} WHERE {columnName} = @userId";
            TryResults response = new TryResults()
            {
                Result = false,
                ErrorText = "등록 실패."
            };
            using (MySqlCommand command = new MySqlCommand(query, Con))
            {
                command.Parameters.AddWithValue("@id", request.Id);
                command.Parameters.AddWithValue("@password", request.Password);
                command.ExecuteNonQuery();
                response.Result = true;
                response.ErrorText = "등록 완료";
            }
            con.Close();
            return Task.FromResult(response);
        }
        public override async Task Chatting(IAsyncStreamReader<ChatMessage> requestStream,IServerStreamWriter<ChatMessage> responseStream,ServerCallContext context)
        {
            // 클라이언트가 보내는 메시지를 수신하면서 반복 처리
            while (await requestStream.MoveNext())
            {
                var incomingMessage = requestStream.Current;
                Console.WriteLine($"[Received] {incomingMessage.UserName}: {incomingMessage.ChattingText}");

                // 예시: 받은 메시지를 에코(Echo)하여 클라이언트에게 다시 보냄
                await responseStream.WriteAsync(new ChatMessage
                {
                    UserName = incomingMessage.UserName,
                    ChattingText = incomingMessage.ChattingText ,
                    Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                });
                
            }
        }
    }
}

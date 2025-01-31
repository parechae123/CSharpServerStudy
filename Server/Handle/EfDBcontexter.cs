using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace CSharpServerStudy.Server.Handle
{
    internal class EfDBcontexter : DbContext
    {
        private readonly IConfiguration _config;
        //DBSet은 "반드시!" db의 이름과 동일해야 매핑이됨
        public DbSet<user> user{ get; set; }

        public EfDBcontexter(IConfiguration configuration)
        {
            _config = configuration;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(
                _config.GetConnectionString("EfDBcontexter"),
                new MySqlServerVersion(new Version(8, 0, 33))
            );
        }
    }
    public class user
    {
        public int index { get; set; }
        public string id { get; set; }
        public string Password { get; set; }
    }
}

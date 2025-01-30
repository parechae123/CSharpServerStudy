using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpServerStudy.Server.Handle
{
    internal class EfDBcontexter : DbContext
    {

        public DbSet<user> users{ get; set; }

        public EfDBcontexter() : base("name=EfDBcontexter")
        {
            
        }
    }

    public class user
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int index { get; set; }
        public string id { get; set; }
        public string Password { get; set; }
    }
}

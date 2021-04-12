using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace ConditionalAccess.Models
{
    public class Enrolldb
    {
        SqlConnection con = new SqlConnection("Data Source=(localdb)//MSSQLLocalDB;Integrated Security = True; Connect Timeout = 30; Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        
        public void SaveRecord(Enroll enroll)
        {
            SqlCommand com = new SqlCommand("dbo.enroll", con);
        }
    }
}

using Qrakhen.Struqt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qrakhen.Struqt.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Database db = new Database("struqt", "Data Source=localhost;Initial Catalog=struqt;Integrated Security=SSPI;Trusted_Connection=true;MultipleActiveResultSets=true");

            db.register(typeof(TestModel));
            
            var sql = new Query.Select("myTable")
                    .limit(10)
                    .sort("id", System.Data.SqlClient.SortOrder.Ascending)
                    .addArgument("myTable", "table123")
                    .where(
                        new Where.Equals("id", 1)
                        .and(new Where.Equals("name", "stefan"))
                        .and(new Where.Equals("i", 3.242f)));

            //Console.WriteLine(sql.build());

            /*TestModel m = new TestModel {
                enabled = true,
                hiddenFromDb = "yes",
                name = "dave",
                dt = DateTime.Now,
                guid = Guid.NewGuid()
            };*/

            var m = TestModel.select<TestModel>(new Where.Equals("id", 2));

            m[0].name = "King of da hill!";
            m[0].store();

            Console.WriteLine(sql.build());
            Console.ReadLine();
        }
    }
}

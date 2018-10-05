﻿using Qrakhen.Struqt.Models;
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
            db.register(typeof(TestType));

            TestModel tm = new TestModel {
                name = "dave",
                enabled = true,
                type_id = 5
            };

            TestType tt = new TestType {
                id = 5,
                name = "5er-Typ",
                price = 49.95m,
                sort = TestType.Sort.Smoo
            };

            tt.store();

            tm.store();

            Console.ReadLine();
        }
    }
}

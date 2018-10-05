using Qrakhen.Struqt.Models;
using System;

namespace Qrakhen.Struqt.CLI
{
    [TableName("test_model")]
    public class TestModel : Model
    {
        [Primary]
        [AutoIncrement]
        [Column]
        public int id;

        [Column]
        public Guid guid;
        
        [Null(true)]
        [Column]
        public string name;

        [Column("alive")]
        public bool enabled;

        [Column("dt")]
        public DateTime dt;

        public string hiddenFromDb;
    }
}

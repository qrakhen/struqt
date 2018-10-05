using Qrakhen.Struqt.ExtendedTypes;
using Qrakhen.Struqt.Models;
using System;

namespace Qrakhen.Struqt.CLI
{
    [TableName("test_model")]
    public class TestModel : Model
    {
        [Primary]
        [AutoIncrement]
        public int id;

        [Column]
        public Guid guid;
        
        [Null(true)]
        [Column]
        public string name;

        [Column("alive")]
        public bool enabled;

        [Column("dt")]
        public NDateTime dt;

        [Column]
        [Reference(typeof(TestModel), "id", "type_container")]
        public int type_id;

        public TestType type_container;
    }

    [TableName("test_type")]
    public class TestType : Model
    {
        public enum Sort
        {
            Cool = 0,
            Smoo = 1,
            Whoo = 2,
            Foou = 3
        }

        [Primary]
        [AutoIncrement]
        public int id;

        [Column]
        public string name;

        [Column]
        public decimal price;

        [Column]
        public Sort sort;
    }
}

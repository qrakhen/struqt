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
            Database db = new Database("db_value", "Data Source=localhost;Initial Catalog=db_value;Integrated Security=SSPI;Trusted_Connection=true;MultipleActiveResultSets=true");

            db.register(typeof(Item));

            foreach (var l in Enum.GetNames(typeof(LootType))) {
                if (l == "XP" || l == "Item") continue;
                Item item = new Item {
                    i_drop_chance = 20,
                    i_name_id = "res_" + l,
                    i_itemtype = Item.ItemType.Resource,
                    i_max_count = 1000,
                    i_price = -1,
                    i_slottype = Item.SlotType.None
                };
                item.store();
            }
        }


        public static string getResourceDbName(LootType lootType)
        {
            return "res_" + Enum.GetName(typeof(LootType), lootType);
        }

        public enum LootType
        {
            XP = 1,
            Wood = 2,
            Scroll = 3,
            ManaBubble = 4,
            TerraToken = 5,
            Coupon = 6,
            ManaPotion = 7,
            SeedsValue = 8,
            SeedsFire = 9,
            SeedsWater = 10,
            SeedsEarth = 11,
            SeedsAir = 12,
            WoodFire = 13,
            WoodWater = 14,
            WoodEarth = 15,
            WoodAir = 16,
            ValueKeyFragmentGold = 17,
            ValueKeyGold = 18,
            ValueKeyFragmentSilver = 19,
            ValueKeySilver = 20,
            Item = 21
        }

        [Serializable()]
        [CacheTable(true)]
        [TableName("tbl_item")]
        public class Item : Model
        {
            [Primary]
            public int i_id;

            [Column]
            [NotNull]
            public SlotType i_slottype;

            [Column]
            [NotNull]
            public ItemType i_itemtype;

            [Column]
            [NotNull]
            public string i_name_id;

            [Column]
            [NotNull]
            public int i_price;

            [Column]
            public string i_description_id;

            [Column]
            [NotNull]
            public int i_drop_chance;

            [Column]
            public int i_max_count;

            public enum ItemType
            {
                Cosmetic = 0,
                Resource = 10
            }

            public enum SlotType
            {
                None = -1,
                Face = 0,
                Hair = 1,
                Diadem = 2,
                Body = 3
            }

        }
    }
}

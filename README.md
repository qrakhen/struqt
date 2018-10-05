# Struqt

#### A simple yet powerful database model system for .NET

## Example Usage:
### Defining Models:
```
using Qrakhen.Struqt.Models;

[TableName("player")]
class Player : Model
{
    [Primary] // mark field as primary.
    [AutoIncrement]
    [Column] // mark field as model column.
    private int id;

    [Column("player_name")] // defines the column name.
    [Null(true)] // nullable
    public string playerName;

    // this field will be ignored due to no [Column] attribute
    public int ignoredInDb;

    [Column]
    private DateTime dt;

    [Column]
    public Guid guid;
}
```
And that's it!
Yes, really. All you need to do is register your model, and you're good to go!

### Using Models
```
Database db = new Database("myDatabase", "connectionString");

// register the model to a database, so it knows where it belongs to.
// this will also automatically create the table if it does not exist.
db.register(typeof(Player));

// create a new instance of the Player model
Player p = new Player {
    playerName = "qrakhen",
    dt = DateTime.Now,
    guid = Guid.NewGuid()
};

// store the instance. automatically inserts when primary key is undefined,
// and updates when there's a primary key set.
p.store();

// retrieve the player again by using the model's built in select function
Player p = Player.select<Player>(new Where.Equals("playerName", "qrakhen"))[0];
```
So much for the basic usage.
It's easy, isn't it?

### Using Queries and Complex Selects
This feature isn't fully done yet, but will follow shorty.
Here are some tricks that work already:
```
var select = new Query.Select("player");
select
    .limit(12)
    .sort("name", SortOrder.Descending)
    .where(
    	new Where.Equals("active", true)
	.and(Where.LargerThan("age", 18))
	.and(new Condition.Or(
	    new Where.Equals("name", "dave"),
	    new Where.Null("first_login"))
    ));

var result = db.select<SomeModel>(
    select, 
    new delegate {
        // select<T> requires a delegate, which provides a RowReader.
        // the row reader makes reading from the stream easy,
        // and this callback is executed for every found row.
        // all of this is done automatically by the abstract model,
        // but there's always a corner case for special needs.
        var obj = new SomeModel();
        obj.name = RowReader.readString("name");
        return obj;
    });
```

### Credits
This Library was solely made using .NET's System[.Data] and that's it.
Copying, modifying, and redistributing is permitted.

#### Author
David 'Qrakhen' Neumaier, qrakhen@gmail.com | http://qrakhen.net/






using System.Data;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace OrgChart.Core;

public class Db(SqliteConnection connection) : IDisposable
{
    public static Db Open()
    {
        var userAppDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (string.Empty.Equals(userAppDataDir)) throw new Exception("Base app data folder does not exist.");

        var dir = new DirectoryInfo(userAppDataDir).CreateSubdirectory("merviche.orgchart");
        var dbfilepath = Path.Combine(dir.FullName, "merviche.orgchart.db");

        var b = new SqliteConnectionStringBuilder();
        b.Mode = SqliteOpenMode.ReadWriteCreate;
        b.DataSource = dbfilepath;

        var connection = new SqliteConnection(b.ConnectionString);
        var db = new Db(connection);
        connection.Open();
        db.Init();
        return db;
    }

    public void Init()
    {
        using var create = connection.CreateCommand();
        create.CommandText = """
                             create table if not exists peoplehr_json
                             (
                                 id      integer not null
                                     constraint peoplehr_json_pk
                                         primary key autoincrement
                                 ,data    jsonb   not null
                                 ,created integer not null
                             )
                             """;

        create.ExecuteNonQuery();
    }

    public PersonRoot Latest()
    {
        using var select = connection.CreateCommand();
        select.CommandText = """
                             select data
                             from peoplehr_json
                             order by id desc
                             limit 1
                             """;

        using var reader = select.ExecuteReader(CommandBehavior.SingleRow);
        if (!reader.Read()) throw new Exception("Empty result set.");
        var stream = reader.GetStream(0);
        var root = JsonSerializer.Deserialize(stream, PersonJsonContext.Default.PersonRoot);
        if (root is null) throw new JsonException("Latest db record deserialized to a c# null.");
        return root;
    }

    public void Save(PersonRoot root)
    {
        var json = JsonSerializer.Serialize(root, PersonJsonContext.Default.PersonRoot);

        using var add = connection.CreateCommand();
        add.CommandText = "insert into peoplehr_json (data, created) values ($data, $now)";
        add.Parameters.AddWithValue("$data", json);
        add.Parameters.AddWithValue("$now", TimeProvider.System.GetUtcNow().ToUnixTimeSeconds());
        var count = add.ExecuteNonQuery();
        Debug.Assert(count == 1, "Should've inserted one row.");
    }

    public void Dispose()
    {
        connection.Dispose();
    }
}

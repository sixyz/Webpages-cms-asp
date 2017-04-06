using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using WebMatrix.Data;

/// <summary>
/// Summary description for BlogPostRepository
/// </summary>
public class TagRepository                                                  //Innehåller liknande kod som PostRepository. Har kommenterat där.
{
    private static readonly string connectionString = "DefaultConnection";
	public TagRepository()
	{
		
	}

    public static dynamic Get(int id)
    {
        using(var db = Database.Open(connectionString))
        {
            var sql = "SELECT * FROM Tags WHERE ID = @0";

            return db.QuerySingle(sql, id);
        }
    }

    public static dynamic Get(string friendlyName)
    {
        using (var db = Database.Open(connectionString))
        {
            var sql = "SELECT * FROM Tags WHERE UrlFriendlyName = @0";

            return db.QuerySingle(sql, friendlyName);
        }
    }

    public static IEnumerable<dynamic> GetAll(string orderBy = null, string where = null)
    {
        using(var db = Database.Open(connectionString))
        {
            var sql = "SELECT * FROM Tags";

            if(!string.IsNullOrEmpty(where))
            {
                sql += " WHERE " + where;
            }

            //if(!string.IsNullOrEmpty(orderBy))
            //{
            //    sql += " ORDER BY " + orderBy;
            //}

            return db.Query(sql);
        }
    }

    public static void Add(string name, string friendlyName)
    {
        using (var db = Database.Open("DefaultConnection"))
        {
            var sql = "INSERT INTO Tags (Name, UrlFriendlyName)" +
 "              VALUES (@0, @1)";

            db.Execute(sql, name, friendlyName);

        }
    }

    public static void Edit(int id, string name, string friendlyName)
    {
        using (var db = Database.Open("DefaultConnection"))
        {
            var sql = "UPDATE Tags SET Name = @0, UrlFriendlyName = @1 " +
                      "WHERE Id = @2";

            db.Execute(sql, name, friendlyName, id);

        }
    }

    public static void Remove(string friendlyName)
    {
        using(var db = Database.Open(connectionString))
        {
            var sql = "DELETE FROM Tags WHERE UrlFriendlyName = @0";

            db.Execute(sql, friendlyName);
        }
    }
}
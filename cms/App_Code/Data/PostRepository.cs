using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using WebMatrix.Data;

/// <summary>
/// Summary description for PostRepository
/// </summary>
public class PostRepository             //Funktioner som hanterar data från databasen, blir renare kod. 
                                        //Andra Repository.cs filer har i stort sett samma kod, fast med lite olika sql queries.
{
    private static readonly string _connectionString = "DefaultConnection";
	public PostRepository()
	{
		
	}

    public static dynamic Get(int id)//Hämta post'er med hjälp av id
    {
        var sql = "select p.*, t.Id as TagId, t.Name as TagName, t.UrlFriendlyName as TagUrlFriendlyName from Posts p " +
                      "left JOIN PostsTagsMapping m on p.Id = m.PostId " +
                      "left join Tags t on t.Id = m.TagId " +
                      "WHERE p.Id = @0";

        var result = DoGet(sql, id);

        return result.Any() ? result.First() : null;
    }

    private static IEnumerable<dynamic> DoGet(string sql, params object[] values)
    {
        using (var db = Database.Open(_connectionString))
        {
            var posts = new List<dynamic>();
            var results = db.Query(sql, values);

            //Loopa igenom resultat från databasen.
            foreach (dynamic result in results)
            {
                dynamic post = posts.SingleOrDefault(p => p.Id == result.Id);

                if (post == null)
                {
                    post = CreatePostObject(result);

                    posts.Add(post);
                }

                if (result.TagId == null)
                {
                    continue;
                }

                dynamic tag = new ExpandoObject();

                tag.Id = result.TagId;
                tag.Name = result.TagName;
                tag.UrlFriendlyName = result.TagUrlFriendlyName;

                post.Tags.Add(tag);
            }
            
            return posts.ToArray();
        }
    }
        

        

    public static dynamic Get(string slug)//Hämta post'er med hjälp av slug'en
    {
        

            var sql = "select p.*, t.Id as TagId, t.Name as TagName, t.UrlFriendlyName as TagUrlFriendlyName from Posts p " +
                      "left JOIN PostsTagsMapping m on p.Id = m.PostId " +
                      "left join Tags t on t.Id = m.TagId " +
                      "WHERE Slug = @0";

            var result =  DoGet(sql, slug);//hämta resultat i functionen DoGet()

            return result.Any() ? result.First() : null;
        
    }

    public static IEnumerable<dynamic> GetAll(string orderBy = null)
    {
        var posts = new List<dynamic>();
        
            var sql = "select p.*, t.Id as TagId, t.Name as TagName, t.UrlFriendlyName as TagUrlFriendlyName from Posts p " +  
                    "left JOIN PostsTagsMapping m on p.Id = m.PostId " + 
                    "left join Tags t on t.Id = m.TagId";

            if(!string.IsNullOrEmpty(orderBy))
            {
                sql += " ORDER BY " + orderBy;
            }

            return DoGet(sql);


    }

    //function som sparar en post. 
    public static void Add(string title, string content, string slug, DateTime? datePublished, int authorId, IEnumerable<int> tags)
    {
        using (var db = Database.Open("DefaultConnection"))
        {
            var sql = "INSERT INTO Posts (Title, Content, DatePublished, AuthorId, Slug)" +
 "              VALUES (@0, @1, @2, @3, @4)";

            db.Execute(sql, title, content, datePublished, authorId, slug);

            var post = db.QuerySingle("SELECT * FROM Posts WHERE Slug = @0", slug);

            AddTags(post.Id, tags, db);
        }
    }

    public static void AddTags(int postId, IEnumerable<int> tags, Database db)
    {
        if (!tags.Any())
        {
            return;
        }

        var sql = "INSERT INTO PostsTagsMapping (PostId, TagId) VALUES (@0, @1)";

        foreach(var tag in tags)
        {
            db.Execute(sql, postId, tag);
        }
                  
    }

    private static void DeleteTags(int id, Database db)
    {
        var sql = "DELETE FROM PostsTagsMapping WHERE PostId = @0";

        db.Execute(sql, id);
    }

    public static void Edit(int id, string title, string content, string slug, DateTime? datePublished, int authorId, IEnumerable<int> tags)
    {
        using (var db = Database.Open("DefaultConnection"))
        {
            var sql = "UPDATE Posts SET Title = @0, Content = @1, DatePublished = @2, AuthorId = @3, Slug = @4 " +
                      "WHERE Id = @5";

            db.Execute(sql, title, content, datePublished, authorId, slug, id);

            DeleteTags(id, db);//Tar bort tag

            AddTags(id, tags, db);//lägger till tag
        }
    }

    

    public static void Remove(string slug)//ta bort post med hjälp av en slug
    {
        using(var db = Database.Open(_connectionString))
        {
            var sql = "SELECT * FROM Posts WHERE Slug = @0";

            var post = db.QuerySingle(sql, slug);

            if(post == null)
            {
                return;
            }

            DeleteTags(post.Id, db); //tar också bort taggen med functionen DeleteTags

            sql = "DELETE FROM Posts WHERE Id = @0";

            db.Execute(sql, post.Id);
        }
    }

    private static dynamic CreatePostObject(dynamic obj)
    {
        dynamic post = new ExpandoObject();

        post.Id = obj.Id;
        post.Title = obj.Title;
        post.Content = obj.Content;
        post.DateCreated = obj.DateCreated;
        post.DatePublished = obj.DatePublished;
        post.AuthorId = obj.AuthorId;
        post.Slug = obj.Slug;
        post.Tags = new List<dynamic>();


        return post;
    }
}
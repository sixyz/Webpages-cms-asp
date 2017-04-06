using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.WebPages;
using WebMatrix.Data;

/// <summary>
/// Summary description for Post
/// </summary>
public class Post
{
	private static WebPageRenderingBase Page
    {
        get { return WebPageContext.Current.Page; }
    }

    public static string Mode
    {//Hämtar vilket mode man är i (create, edit, delete, new)
        get
        {
            if (Page.UrlData.Any())
            {
                return Page.UrlData[0].ToLower();
            }
            return string.Empty;
        }
    }

    public static string Slug
    {
        get {
            if(Mode == "test")
            {
                return Page.UrlData[1];
            }
            if(Mode == "edit")
            {
                return Page.UrlData[1]; //localhost:1111/admin/post/edit/post-title - returnerar då post-title.
            }
            return string.Empty;
        }
        
    }

    public static dynamic Current
    {
        get
        {       //hämta resultat med hjälp av Slug från databasen.
                var result = PostRepository.Get(Slug);

                return result ?? CreatePostObject();//result null? annars CreatePostObject.
        }
    }

    private static dynamic CreatePostObject()
    {
        dynamic obj = new ExpandoObject();

        obj.Id = 0;
        obj.Title = string.Empty;
        obj.Content = string.Empty;
        obj.DateCreated = DateTime.Now;
        obj.DatePublished = null;
        obj.Slug = string.Empty;
        obj.AuthorId = null;
        obj.Tags = new List<dynamic>();

        return obj;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.SessionState;
using WebMatrix.Data;

/// <summary>
/// Summary description for PostHandler
/// </summary>
public class PostHandler : IHttpHandler, IReadOnlySessionState
{
	

    public bool IsReusable
    {
        get { return false; }
    }

    public void ProcessRequest(HttpContext context)
    {
        AntiForgery.Validate();

        if(!WebUser.IsAuthenticated)
        {
            throw new HttpException(401, "You must login to do that");
        }

        if (!WebUser.HasRole(UserRoles.Admin) && !WebUser.HasRole(UserRoles.Editor) && !WebUser.HasRole(UserRoles.Author))
        {
            throw new HttpException(401, "You do not have permission to do that");
        }

        

        var mode = context.Request.Form["Mode"];
        var title = context.Request.Form["postTitle"];
        var content = context.Request.Form["postContent"];
        var slug = context.Request.Form["postSlug"];
        var id = context.Request.Form["postId"];
        var datePublished = context.Request.Form["postDatePublished"];
        var authorId = context.Request.Form["postAuthorId"];
        var postTags = context.Request.Form["postTags"];
        IEnumerable<int> tags = new int[] { };

        if(!string.IsNullOrEmpty(postTags))
        {
            //För att man ska kunna lägga till mer än en tagg för en post.
            tags = postTags.Split(',').Select(v => Convert.ToInt32(v));
        }

        if ((mode == "edit" || mode == "delete" && WebUser.HasRole(UserRoles.Author)))
        {
            if (WebUser.UserId != Convert.ToInt32(authorId))
            {
                throw new HttpException(401, "You do not have permission to do that");
            }
        }

        //Slug är ett url-friendlyname
        //Om vi inte fyllt i slug manuellt så skapar den en slug med hjälp av titeln på post'en.
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = CreateSlug(title);
        }

        //Går in i en function beroende på vilket mode man är i.
        if(mode == "edit")
        {
            EditPost(Convert.ToInt32(id), title, content, slug, datePublished, Convert.ToInt32(authorId), tags);
        }
        else if (mode == "new")
        {
            CreatePost(title, content, slug, datePublished, WebUser.UserId, tags);
        }
        else if (mode == "delete")
        {
            DeletePost(slug);
        }
                   
            context.Response.Redirect("~/admin/post/");
        
    }

    private static void DeletePost(string slug)
    {
        PostRepository.Remove(slug); //Delete post med inparameter slug
    }

    private static void CreatePost(string title, string content, string slug, string datePublished, int authorId, IEnumerable<int> tags)//Skapa post.
    {
        var result = PostRepository.Get(slug);
        DateTime? published = null;

        if (result != null)//kolla i databasen om det redan finns en slug med samma namn.
        {
            throw new HttpException(409, "Slug is already in use");
        }

        if(!string.IsNullOrWhiteSpace(datePublished))
        {
            published = DateTime.Parse(datePublished); 
        }

        PostRepository.Add(title, content, slug, published, authorId, tags);//Går in i en function Add() med inparametrar. sparas sedan i databasen.
    }

    private static void EditPost(int id, string title, string content, string slug, string datePublished, int authorId, IEnumerable<int> tags)
    {
        var result = PostRepository.Get(id);//hämta resultat med inparameter id.
        DateTime? published = null;

        if (result == null)//om result är null så hittade vi ingen post i databasen med det id't.
        {
            throw new HttpException(404, "Post does not exist");
        }

        if (!string.IsNullOrWhiteSpace(datePublished))
        {
            published = DateTime.Parse(datePublished);
        }

        PostRepository.Edit(id, title, content, slug, published, authorId, tags);//Går in i funktionen Edit() med inparametrar.
    }

    private static string CreateSlug(string title)
    {
        title = title.ToLowerInvariant().Replace(" ", "-");
        title = Regex.Replace(title, @"[^0-9a-z-]", string.Empty);

        return title;
    }
}
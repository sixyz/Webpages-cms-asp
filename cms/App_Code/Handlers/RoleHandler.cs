using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.SessionState;
using WebMatrix.Data;

/// <summary>
/// Summary description for RoleHandler
/// </summary>                                                      //Innehåller liknande kod som PostHandler.cs. Har kommenterat där.
public class RoleHandler : IHttpHandler, IReadOnlySessionState
{
	

    public bool IsReusable
    {
        get { return false; }
    }

    public void ProcessRequest(HttpContext context)
    {
        AntiForgery.Validate();

        if (!WebUser.IsAuthenticated)
        {
            throw new HttpException(401, "You must login to do this");
        }

        if (!WebUser.HasRole(UserRoles.Admin))
        {
            throw new HttpException(401, "You do not have permission to do this");
        }


        var mode = context.Request.Form["Mode"];
        var name = context.Request.Form["roleName"];
        var id = context.Request.Form["roleId"];

        if(mode == "edit")
        {
            EditRole(Convert.ToInt32(id), name);
        }
        else if (mode == "new")
        {
            CreateRole(name);
        }
        else if (mode == "delete")
        {
            DeleteRole(name);
        }
            
            context.Response.Redirect("~/admin/role/");
    }

    private static void DeleteTag(string friendlyName)
    {
        TagRepository.Remove(friendlyName);
    }

    private static void CreateRole(string name)
    {
        var result = RoleRepository.Get(name);
        

        if (result != null)
        {
            throw new HttpException(409, "Role is already in use");
        }

        RoleRepository.Add(name);
    }

    private static void EditRole(int id, string name)
    {
        var result = RoleRepository.Get(id);
        

        if (result == null)
        {
            throw new HttpException(404, "Role does not exist");
        }

        RoleRepository.Edit(id, name);
    }

    private static void DeleteRole(string name)
    {
        RoleRepository.Remove(name);
    }
}
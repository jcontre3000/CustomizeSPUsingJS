﻿using Microsoft.SharePoint.Client;
using System;
using System.Linq;
using System.Text;

namespace CustomizeSPUsingJSWeb
{
  public partial class Default : System.Web.UI.Page
  {
    protected void Page_PreInit(object sender, EventArgs e)
    {
      Uri redirectUrl;
      switch (SharePointContextProvider.CheckRedirectionStatus(Context, out redirectUrl))
      {
        case RedirectionStatus.Ok:
          return;
        case RedirectionStatus.ShouldRedirect:
          Response.Redirect(redirectUrl.AbsoluteUri, endResponse: true);
          break;
        case RedirectionStatus.CanNotRedirect:
          Response.Write("An error occurred while processing your request.");
          Response.End();
          break;
      }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
     
    }

    protected void addCustomization_Click(object sender, EventArgs e)
    {
      var spContext = SharePointContextProvider.Current.GetSharePointContext(Context);

      using (var ctx = spContext.CreateUserClientContextForSPHost())
      {
        AddJsLink(ctx, ctx.Web);
      }
    }

public void AddJsLink(ClientContext ctx, Web web)
{
  string scenarioUrl = String.Format("{0}://{1}:{2}/Scripts", this.Request.Url.Scheme,
                                      this.Request.Url.DnsSafeHost, this.Request.Url.Port);
  string revision = Guid.NewGuid().ToString().Replace("-", "");
  string jsLink = string.Format("{0}/{1}?rev={2}", scenarioUrl, "customization.js", revision);

  StringBuilder scripts = new StringBuilder(@"
            var headID = document.getElementsByTagName('head')[0]; 
            var");

  scripts.AppendFormat(@"
            newScript = document.createElement('script');
            newScript.type = 'text/javascript';
            newScript.src = '{0}';
            headID.appendChild(newScript);", jsLink);
  string scriptBlock = scripts.ToString();

  var existingActions = web.UserCustomActions;
  ctx.Load(existingActions);
  ctx.ExecuteQuery();
  var actions = existingActions.ToArray();
  foreach (var action in actions)
  {
    if (action.Description == "customization" &&
        action.Location == "ScriptLink")
    {
      action.DeleteObject();
      ctx.ExecuteQuery();
    }
  }

  var newAction = existingActions.Add();
  newAction.Description = "customization";
  newAction.Location = "ScriptLink";

  newAction.ScriptBlock = scriptBlock;
  newAction.Update();
  ctx.Load(web, s => s.UserCustomActions);
  ctx.ExecuteQuery();
}

    protected void removeCustomization_Click(object sender, EventArgs e)
    {
      var spContext = SharePointContextProvider.Current.GetSharePointContext(Context);

      using (var ctx = spContext.CreateUserClientContextForSPHost())
      {
        DeleteJsLink(ctx, ctx.Web);
      }
    }

    public void DeleteJsLink(ClientContext ctx, Web web)
    {
      var existingActions = web.UserCustomActions;
      ctx.Load(existingActions);
      ctx.ExecuteQuery();
      var actions = existingActions.ToArray();
      foreach (var action in actions)
      {
        if (action.Description == "customization" &&
            action.Location == "ScriptLink")
        {
          action.DeleteObject();
          ctx.ExecuteQuery();
        }
      }

    }
  }
}
<%@ Page Language="C#" %>
<script runat="server">

    public bool IsLoggedIn()
    {
        return Sitecore.Context.User.Identity.IsAuthenticated && Sitecore.Web.Authentication.TicketManager.IsCurrentTicketValid() && !Sitecore.Security.Authentication.AuthenticationHelper.IsAuthenticationTicketExpired();
    }

</script>
<%
    var username = Request.QueryString["u"] ?? string.Empty;
    var password = Request.QueryString["p"] ?? string.Empty;
    var redirect = Request.QueryString["r"] ?? string.Empty;

    var isLoggedIn = false;
    try
    {
        isLoggedIn = IsLoggedIn();
    }
    catch (MissingMethodException)
    {
        isLoggedIn = Sitecore.Context.User.Identity.IsAuthenticated;
    }

    if (isLoggedIn)
    {
        Sitecore.Web.WebUtil.Redirect(redirect);
        return;
    }

    var args1 = new Sitecore.Pipelines.LoggingIn.LoggingInArgs();
    args1.Username = username;
    args1.Password = password;
    args1.StartUrl = redirect;

    Sitecore.Pipelines.Pipeline.Start("loggingin", args1);
    if (!args1.Success)
    {
        Sitecore.Web.WebUtil.Redirect(redirect);
        return;
    }

    var args2 = new Sitecore.Pipelines.LoggedIn.LoggedInArgs();
    args2.Username = username;
    args2.StartUrl = redirect;
    args2.Persist = false;

    Sitecore.Pipelines.Pipeline.Start("loggedin", args2);

    Sitecore.Web.WebUtil.Redirect(redirect);
%>
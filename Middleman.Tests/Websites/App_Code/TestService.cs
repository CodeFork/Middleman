using System.Web.Services;

[WebService(Namespace = "http://localhost/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class TestService : WebService
{
    [WebMethod]
    public string SimpleMethod()
    {
        return "Hello World";
    }
}
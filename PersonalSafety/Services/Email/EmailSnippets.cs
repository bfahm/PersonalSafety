using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace PersonalSafety.Services.Email
{
    public static class EmailSnippets
    {
        public static string BuildFooter(string brand)
        {
            StringBuilder footer = new StringBuilder("<b> Take care. Be Safe. </b>");
            footer.AppendLine($"<p> {brand} Team </p>");

            return footer.ToString();
        }

        public static string BuildHi(string email)
        {
            return $"<h5>Hi {email}, </h5>";
        }

        public static NameValueCollection BuildQuery(string email, string token)
        {
            var queryStrings = HttpUtility.ParseQueryString(string.Empty);
            queryStrings["email"] = email;
            queryStrings["token"] = token;

            return queryStrings;
        }

        public static NameValueCollection BuildQuery(string oldEmail, string newEmail, string token)
        {
            var queryStrings = HttpUtility.ParseQueryString(string.Empty);
            queryStrings["oldEmail"] = oldEmail;
            queryStrings["newEmail"] = newEmail;
            queryStrings["token"] = token;

            return queryStrings;
        }
    }
}

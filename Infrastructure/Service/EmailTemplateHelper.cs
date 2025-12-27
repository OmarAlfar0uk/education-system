namespace EduocationSystem.Infrastructure.Service
{
    public static class EmailTemplateHelper
    {
        public static string LoadTemplate(string templateName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Emails", templateName);

            return File.ReadAllText(path);
        }

        public static string ReplaceTokens(string html, Dictionary<string, string> tokens)
        {
            foreach (var t in tokens)
                html = html.Replace($"{{{{{t.Key}}}}}", t.Value);

            return html;
        }
    }

}

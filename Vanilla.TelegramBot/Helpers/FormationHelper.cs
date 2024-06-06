using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Vanilla.TelegramBot.Helpers
{
    public static class FormationHelper
    {
        public static List<string> Links(string text)
        {
            var links = text.Split(",");
            foreach (var link in links)
            {
                //if (!Uri.IsWellFormedUriString(link, UriKind.Absolute))
                Uri? outUri = new Uri(link);
                var validateUri = Uri.TryCreate(link, UriKind.Absolute, out outUri);

                bool isShemeValid = outUri.Scheme == "https" || outUri.Scheme == "http" ? true : false;
                bool isHostValid = outUri.Host.Split(".").Length > 1 && outUri  .Host.Split(".").Last() != "." ? true : false;

                

                if (!validateUri || !isShemeValid || !isHostValid)
                {
                    throw new ValidationException(String.Format("Link <i>{0}</i> isn`t correct. Try again", link));
                }
            }

            var Links = new List<string>();
            Links.AddRange(links);
            return Links;
        }
    }
}

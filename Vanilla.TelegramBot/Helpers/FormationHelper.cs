using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Vanilla.TelegramBot.Helpers
{
    public static class FormationHelper
    {
        public static List<string> Links(string text)
        {
            var stringLinks = text.Split(",");
            var Links = new List<string>();

            foreach (var link in stringLinks)
            {
                string url = new string(link.Where(c => !char.IsWhiteSpace(c)).ToArray()); ;
                //if (!Uri.IsWellFormedUriString(link, UriKind.Absolute))
                /*Uri? outUri = new Uri(link);
                var validateUri = Uri.TryCreate(link, UriKind.Absolute, out outUri);

                bool isShemeValid = outUri.Scheme == "https" || outUri.Scheme == "http" ? true : false;
                bool isHostValid = outUri.Host.Split(".").Length > 1 && outUri  .Host.Split(".").Last() != "." ? true : false;

                

                if (!validateUri || !isShemeValid || !isHostValid)
                {
                    throw new ValidationException(String.Format("Link <i>{0}</i> isn`t correct. Try again", link));
                }*/

                if (!ValidateUrl(url))
                {
                    throw new ValidationException(String.Format("Link <i>{0}</i> isn`t correct. Try again", link));
                }

                Regex isHaveShema = new Regex(@"^https?:\/\/");
                if (!isHaveShema.Match(url).Success)
                {
                    url = "https://" + url;
                }

                Uri? outUri = new Uri(url);
                var validateUri = Uri.TryCreate(url, UriKind.Absolute, out outUri);

                bool isShemeValid = outUri.Scheme == "https" || outUri.Scheme == "http" ? true : false;

                if (!validateUri || !isShemeValid)
                {
                    throw new ValidationException(String.Format("Link <i>{0}</i> isn`t correct. Try again", link));
                }

                Links.Add(url);

            }

            //Links.AddRange(stringLinks);
            return Links;
        }

        public static bool ValidateUrl(string value)
        {
            value = value.Trim();
            if (value == "") return false;

            //Regex pattern = new Regex(@"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$");
            //Regex pattern = new Regex(@"^(https?:\/\/)?(www\.)?[a-zA-Z0-9-]+(\.[a-zA-Z]{2,})(\/[a-zA-Z0-9-._~:/?#[\]@!$&'()*+,;=%]*)?$");
            Regex pattern = new Regex(@"^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$");
            Match match = pattern.Match(value);
            if (match.Success == false) return false;
            return true;
        }
    }
}

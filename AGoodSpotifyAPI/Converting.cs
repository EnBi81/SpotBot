using AGoodSpotifyAPI.JsonSchema;
using AGoodSpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace AGoodSpotifyAPI
{
    internal class Converting
    {
        public static AuthScopes[] GetAuthScopes(string scope)
        {
            if (scope is null) return new AuthScopes[0];

            List<string> scopes = scope.Split(' ').ToList();
            List<AuthScopes> list = new List<AuthScopes>(); 
            for (int i = 0; i < scopes.Count; i++)
            {
                string text = scopes[i];

                string[] splitted = text.Split('-');

                for (int j = 0; j < splitted.Length; j++)
                {
                    splitted[j] = splitted[j][0..1].ToUpper() + splitted[j][1..];
                }

                text = string.Join('_', splitted);

                list.Add(Enum.Parse<AuthScopes>(text));
            }

            return list.ToArray();
        }

        public static async Task<List<T>> GetPagingItems<T>(PagingJSON<T> paging, string token)
        {
            if (paging is null) throw new ArgumentNullException("paging");
            if (paging.Items is null)
            {
                var res = await new WebHelper<PagingJSON<T>>(token, paging.Href, Method.Get).GetResultAsync();
                if (res.IsError) throw new ArgumentNullException();

                paging = res.Result;
                if (paging is null || paging.Items is null) throw new ArgumentNullException();
            }
            
            var list = new List<T>();
            do
            {
                var items = paging.Items;

                if (!(items is null))
                {
                    list.AddRange(items);
                }

                if (paging.Next is null) break;
                paging = (await new WebHelper<PagingJSON<T>>(token, paging.Next, Method.Get).GetResultAsync()).Result;
            } while (true);

            return list;
        }

        public static Markets[] StringToMarkets(string[] markets)
        {
            var seged = new List<Markets>();

            foreach (var i in markets)
            {
                if (Enum.TryParse(i, out Markets result)) seged.Add(result);
            }

            return seged.ToArray();
        }             

        public static ReleaseDatePrecision StringToRDP(string text)
        {
            text = text[0..1].ToUpper() + text[1..];

            return Enum.Parse<ReleaseDatePrecision>(text);
        }

        public static AlbumType StringToAlbumType(string text)
        {
            text = text[0..1].ToUpper() + text[1..];

            return Enum.Parse<AlbumType>(text);
        }

        public static List<List<T>> Cutting<T>(List<T> list, int cutting = 50)
        {
            if (cutting < 1) throw new ArgumentOutOfRangeException("cutting", "can't be less than one");
            if (list is null) throw new ArgumentNullException();

            if (list.Count == 0) return new List<List<T>>();

            if (list.Count < cutting) return new List<List<T>>() { list };


            var seged = new List<List<T>>();

            for (int i = 0; i < list.Count; i++)
            {
                if (i % cutting == 0) seged.Add(new List<T>());

                seged[^1].Add(list[i]);
            }

            return seged;
            
        }

        public static string RangeConvert(TimeRange? range)
        {
            if (range is null) throw new ArgumentNullException();

            return range switch { TimeRange.LongTerm => "long_term", TimeRange.MediumTerm => "medium_term", TimeRange.ShortTerm => "short_term", _ => throw new NotImplementedException() };
        }
    }
}

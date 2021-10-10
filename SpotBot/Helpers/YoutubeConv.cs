using AGoodSpotifyAPI.Classes;
using SpotBot.Spotify;
using System.Linq;
using System.Threading.Tasks;

namespace SpotBot.Helpers
{
    public class YoutubeConv
    {
        public string[] Authors { get; set; }
        public string Title { get; set; }
        private string RawTitle { get; set; }


        public YoutubeConv(string title)
            => TryParse2(title);

        private void TryParse(string title, bool firstArtist = true)
        {
            RawTitle = title;
            if (title.Contains(" ft")) title = title.Substring(0, title.IndexOf(" ft"));
            if (title.ToLower().Contains("official")) title = title.Substring(0, title.ToLower().IndexOf("official"));
            
            if (title.Contains("-"))
            {
                var datas = title.Split("-");
                if (!firstArtist) (datas[0], datas[1]) = (datas[1], datas[0]);

                var author = datas[0].Trim();
                if(!firstArtist)
                {
                    if (author.Contains("("))
                        author = author.Substring(0, author.IndexOf("("));
                    if (author.Contains("["))
                        author = author.Substring(0, author.IndexOf("["));
                }


                Title = datas[1];

                if (author.Contains(","))
                    Authors = author.Split(",");
                else if (author.Contains(" x ")) Authors = author.Split(" x ");
                else if (author.Contains(" X ")) Authors = author.Split(" X ");
                else if (author.Contains(" & ")) Authors = author.Split(" & ");
                else Authors = new[] { author };

                for (int i = 0; i < Authors.Length; i++)
                {
                    Authors[i] = Authors[i].Trim();
                }

            }
            else
                (Title, Authors) = (title, null);



            if (Title.Contains("("))
                Title = Title.Substring(0, Title.IndexOf("("));
            if (Title.Contains("["))
                Title = Title.Substring(0, Title.IndexOf("["));
            if (Title.Contains(" Feat")) Title = Title.Substring(0, Title.IndexOf(" Feat"));

            Title = Title.Trim();
        }


        private void TryParse2(string rawTitle, bool firstArtist = true)
        {
            RawTitle = rawTitle;

            string title, author;

            if (rawTitle.Contains("-"))
            {
                var helper = rawTitle.Split("-");

                (author, title) = firstArtist ? (helper[0], helper[1]) : (helper[1], helper[0]);
            }
            else (author, title) = (null, rawTitle);

            CutParentheses(ref title);
            CutFeat(ref title);
            Title = title;

            if (!(author is null))
            {
                CutParentheses(ref author);
                CutFeat(ref author);
                Authors = SplitArtists(author);
            }
            else Authors = null;
            
        }

        private static void CutFeat(ref string text)
        {
            var feats = new[] {" Feat", " feat", " FEAT", "featured", "FEATURED", "Featured", "official", "Official", " ft", " FT", " Ft"  };

            foreach (var f in feats)
            {
                if (text.Contains(f)) text = text[0..text.IndexOf(f)];
            }
            text = text.Trim();
        }
        private static void CutParentheses(ref string text)
        {
            if (text.Contains("(")) text = text.Substring(0, text.IndexOf("("));
            if (text.Contains("[")) text = text.Substring(0, text.IndexOf("["));
            text = text.Trim();
        }
        private static string[] SplitArtists(string a)
        {
            string[] artists = new[] { a };
            var splitters = new[] { ",", " x ", " X ", " and ", " & ", " + " };

            foreach (var s in splitters)
            {
                if(a.Contains(s))
                {
                    artists = a.Split(s);
                    break;
                }
            }

            for (int i = 0; i < artists.Length; i++)
            {
                artists[i] = artists[i].Trim();
            }

            return artists;
        }



        public async Task<Track> GetSpotifyTrack()
        {
            var query = Title;
            if (!(Authors is null) && Authors.Any()) query += " artist:" + Authors.First();
            var token = await SpotClient.GetTokenAsync();
            var res = await Search.SearchTrack(query: query,token: token);

            if (res.Any()) return res.First();



            TryParse2(RawTitle, false);
            query = Title;
            if (!(Authors is null) && Authors.Any()) query += " artist:" + Authors.First();
            res = await Search.SearchTrack(query: query, token);

            if (res.Any()) return res.First();


            return null;
        }

    }
}

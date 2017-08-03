using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;

namespace SteamFilter
{
	public class SteamFilter
	{

		public static bool useSteamAPI => false;
		public static string SteamAPIKey => "";

		public static bool tryGetSteam64ID (string identifier, out string result)
		{
			result = "";
			try
			{

				if (identifier == null)
					return false;

				Regex rg = new Regex (@"steamcommunity.com/id/(\w+)");
				var match = rg.Match (identifier);
				if (match.Success)
				{
					// http://steamcommunity.com/id/{id} Format
					if (useSteamAPI)
					{
						WebClient client = new WebClient ();
						var url = String.Format (
								"http://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key={0}&vanityurl={1}",
								SteamAPIKey,
								match.Groups [1].Value);
						var response = client.DownloadString (url);
						
						JObject json = JObject.Parse (response);
						if (json ["response"] ["success"].ToString () == "1")
						{
							result = json ["response"] ["steamid"].ToString ();
							client.Dispose ();
							return true;
						}
						else
						{
							client.Dispose ();
							return false;
						}
					}
					else
					{
						try
						{
							XmlTextReader reader = new XmlTextReader (String.Format ("http://steamcommunity.com/id/{0}?xml=1", match.Groups [1].Value));

							while (reader.Read ())
							{
								if (reader.Name != "steamID64")
									continue;
								result = reader.ReadInnerXml ();
								break;
							}
							reader.Close ();
							return true;
						}
						catch (WebException ex)
						{
							return false;
						}
					}
				}

				rg = new Regex (@"steamcommunity.com/profiles/([0-9]+)");
				match = rg.Match (identifier);
				if (match.Success)
				{
					// http://steamcommunity.com/profiles/{id} Format
					result = match.Groups [1].Value;
					return true;
				}

				rg = new Regex (@"STEAM_0:\d:\d{8}");
				match = rg.Match (identifier);
				if (match.Success)
				{
					// STEAM_0:0:id
					var splits = identifier.Split (':');
					long id = Convert.ToInt64 (splits [2]) * 2;
					id += Convert.ToInt32 (splits [1]);
					id += 76561197960265728;

					result = id.ToString ();
					return true;
				}

				rg = new Regex (@"steamcommunity.com/tradeoffer/new/?\?partner=(\d+)");
				match = rg.Match (identifier);
				if (match.Success)
				{
					// https://steamcommunity.com/tradeoffer/new/?partner={id}&token={token}
					var id = Convert.ToDecimal (match.Groups [1].Value) + Convert.ToDecimal ("76561197960265728");

					result = id.ToString ();
					return true;
				}

				rg = new Regex (@"\[U:\d:\d+\]");
				match = rg.Match (identifier);
				if (match.Success)
				{
					// [U:1:id]
					var splits = identifier.Trim ('[', ']').Split (':');
					var id = Convert.ToDecimal (splits [2]) + Convert.ToDecimal ("76561197960265728");

					result = id.ToString ();
					return true;
				}


				if (identifier.Length == 17)
				{
					result = identifier;
					return true;
				}

			}
			return false;
		}
	}
}

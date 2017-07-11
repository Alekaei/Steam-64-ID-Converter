public static bool tryGetSteam64ID (string identifier, out string result)
{
	result = "";

	Regex rg = new Regex (@"(?:http|https)://steamcommunity.com/id/.*");
	var match = rg.Match (identifier);
	if (match.Success)
	{
		// http://steamcommunity.com/id/{id} Format

		XmlTextReader reader = new XmlTextReader (String.Format ("http://steamcommunity.com/id/{0}?xml=1", identifier.Split ('/') [4]));

		while (reader.Read ())
		{
			if (reader.Name != "steamID64")
				continue;

			result = reader.Value;
			break;
		}
		return true;
	}

	rg = new Regex (@"(?:http|https)://steamcommunity.com/profiles/.*");
	match = rg.Match (identifier);
	if (match.Success)
	{
		// http://steamcommunity.com/profiles/{id} Format
		var splits = identifier.Split ('/');
		result = splits [4];
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

	rg = new Regex (@"(?:https|http)://steamcommunity.com/tradeoffer/new/?\?partner=\d+");
	match = rg.Match (identifier);
	if (match.Success)
	{
		// https://steamcommunity.com/tradeoffer/new/?partner={id}&token={token}
		var id = Convert.ToDecimal (match.Groups [0].Value) + Convert.ToDecimal ("76561197960265728");

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

	return false;
}

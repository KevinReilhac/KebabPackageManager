/**
################################################################################
#          File: ConfigReader.cs                                               #
#          File Created: Thursday, 26th May 2022 7:16:49 pm                    #
#          Author: Kévin Reilhac (kevin.reilhac.pro@gmail.com)                 #
################################################################################
**/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kebab.PackageManager.Models;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Net.Http;

namespace Kebab.PackageManager
{
	public class KPMConfigReader
	{
		public static string PASTBIN_FILE_ID = "SD5A7gKZ";

		public async static Task<KPMConfig> GetConfig()
		{
			string url = string.Format("https://pastebin.com/raw/" + PASTBIN_FILE_ID);
			HttpClient client = new HttpClient();

			string textConfig = await client.GetStringAsync(url);
			return (JsonUtility.FromJson<KPMConfig>(textConfig));
		}
	}
}
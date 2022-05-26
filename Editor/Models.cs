/**
################################################################################
#          File: Models.cs                                                     #
#          File Created: Thursday, 26th May 2022 7:06:35 pm                    #
#          Author: Kévin Reilhac (kevin.reilhac.pro@gmail.com)                 #
################################################################################
**/

using System.Collections.Generic;

namespace Kebab.PackageManager.Models
{
	[System.Serializable]
	public class KPMConfig
	{
		public string github_username;
		public List<KPMModule> modules;
	}

	[System.Serializable]
	public class KPMModule
	{
		public string name;
		public string description;
		public string package_id;
		public string[] dependencies;
	}
}

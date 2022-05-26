using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using Kebab.PackageManager.Models;

namespace Kebab.PackageManager
{
	public class KPM_Window : EditorWindow
	{
		private static KPMConfig config = null;
		private static Vector2 scrollValue = Vector2.zero;
		private static List<string> installedPackages = null;

		private static ListRequest packagesListRequest = null;
		private static AddAndRemoveRequest packagesAddRemoveRequest = null;

		[MenuItem("Tools/Kebab Package Manager")]
		private static void ShowWindow()
		{
			var window = GetWindow<KPM_Window>();
			window.titleContent = new GUIContent("Kebab Package Manager");
			window.Show();
		}

		private void OnEnable()
		{
			if (config == null)
				UpdateConfig();
		}

		private void OnGUI()
		{
			if (packagesAddRemoveRequest != null && !packagesAddRemoveRequest.IsCompleted)
			{
				GUILayout.Label("Process...");
				return;
			}
			if (config == null)
			{
				GUILayout.Label("Loading config files...");
				return;
			}
			if (GUILayout.Button(EditorGUIUtility.IconContent("Refresh")))
				UpdateConfig();
			scrollValue = GUILayout.BeginScrollView(scrollValue);
			foreach (KPMModule module in config.modules)
			{
				EditorGUILayout.Space();
				DrawModuleCard(module);
				GUILine();
			}
			GUILayout.EndScrollView();
		}

		private void DrawModuleCard(KPMModule module)
		{
			bool isInstalled = IsModuleInstalled(module);

			GUILayout.Label(module.name + (isInstalled ? " (INSTALLED)" : ""), EditorStyles.largeLabel);
			GUILayout.Label(module.description);
			if (module.dependencies == null || module.dependencies.Length > 0)
				GUILayout.Label("Dependencies: " + string.Join(',', module.dependencies), EditorStyles.miniLabel);
			EditorGUILayout.Space();
			if (isInstalled)
			{
				if (GUILayout.Button("Uninstall"))
					UninstallPackage(module);
			}
			else
			{
				if (GUILayout.Button("Install"))
					InstallPackage(module);
			}
		}

		private bool IsModuleInstalled(KPMModule module)
		{
			if (installedPackages == null)
				return (false);
			return (installedPackages.Contains(module.package_id));
		}

		private void InstallPackage(KPMModule module)
		{
			List<string> sshUrls = new List<string>();

			sshUrls.Add(GetGitSSHUrl(module.name));
			sshUrls.AddRange(GetAllDependencies(module).Select((d) => GetGitSSHUrl(d)));

			packagesAddRemoveRequest = Client.AddAndRemove(sshUrls.ToArray());
			EditorApplication.update += AddAndRemoveRequestUpdate;
		}

		private string GetGitSSHUrl(string moduleName)
		{
			return (string.Format("git@github.com:{0}/{1}.git", config.github_username, moduleName));
		}

		private List<string> GetAllDependencies(KPMModule module)
		{
			List<string> dependencies = new List<string>();

			if (module.dependencies == null)
				return (dependencies);

			foreach (string dependency in module.dependencies)
			{
				dependencies.Add(dependency);
				KPMModule dependency_module = config.modules.Find((m) => m.name == dependency);

				dependencies.AddRange(GetAllDependencies(dependency_module));
			}

			return dependencies.Distinct().ToList();
		}

		private void UninstallPackage(KPMModule module)
		{
			List<string> packagesIds = new List<string>();

			packagesIds.Add(module.package_id);

			if (EditorUtility.DisplayDialog("Uninstall module", "Uninstall dependencies too ?", "yes", "no"))
			{
				List<string> dependenciesName = GetAllDependencies(module);
				List<string> dependenciesPackageNames = new List<string>();

				foreach (string dependencyName in dependenciesName)
				{
					KPMModule dependencyModule = config.modules.Find((m) => m.name == dependencyName);

					dependenciesPackageNames.Add(dependencyModule.package_id);
				}

				packagesIds.AddRange(dependenciesPackageNames);
			}

			packagesAddRemoveRequest = Client.AddAndRemove(packagesToRemove: packagesIds.ToArray());
			EditorApplication.update += AddAndRemoveRequestUpdate;
		}

		private void AddAndRemoveRequestUpdate()
		{
			if (packagesAddRemoveRequest == null)
				return;
			if (packagesAddRemoveRequest.IsCompleted)
			{
				if (packagesAddRemoveRequest.Status == StatusCode.Success)
				{
					Debug.Log("Success");
				}
				else if (packagesAddRemoveRequest.Status >= StatusCode.Failure)
					Debug.LogError(packagesListRequest.Error.message);

				EditorApplication.update -= AddAndRemoveRequestUpdate;
			}
		}

		private async void UpdateConfig()
		{
			config = null;

			GetPackageList();
			config = await KPMConfigReader.GetConfig();
			Repaint();
		}

		private void GetPackageList()
		{
			packagesListRequest = Client.List();
			EditorApplication.update += GetPackageListProgress;
		}

		private static void GetPackageListProgress()
		{
			if (packagesListRequest == null)
				return;
			if (packagesListRequest.IsCompleted)
			{
				if (packagesListRequest.Status == StatusCode.Success)
					installedPackages = packagesListRequest.Result.Select((p) => p.packageId.Split("@")[0]).ToList();
				else if (packagesListRequest.Status >= StatusCode.Failure)
					Debug.Log(packagesListRequest.Error.message);

				EditorApplication.update -= GetPackageListProgress;
			}
		}

		private void GUILine(int i_height = 1)
		{
			Rect rect = EditorGUILayout.GetControlRect(false, i_height);
			rect.height = i_height;

			EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
		}

	}
}
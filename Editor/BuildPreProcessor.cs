using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace com.ktgame.analytics.tracker.editor
{
	public class BuildPreProcessor : IPreprocessBuildWithReport
	{
		public int callbackOrder => 1;

		private const string PackageName = "com.ktgame.analytics.tracker";

		public void OnPreprocessBuild(BuildReport report)
		{
			var pluginPath = Path.Combine(Application.dataPath, $"Plugins/Ktgame/Settings/{PackageName}");
			if (!Directory.Exists(pluginPath))
			{
				Directory.CreateDirectory(pluginPath);
			}

			if (AssetDatabase.IsValidFolder($"Packages/{PackageName}"))
			{
				AssetDatabase.CopyAsset($"Packages/{PackageName}/Runtime/link.xml", $"Assets/Plugins/Unimob/Settings/{PackageName}/link.xml");
			}
		}
	}
}

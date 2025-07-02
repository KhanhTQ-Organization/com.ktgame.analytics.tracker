using UnityEditor;
using com.ktgame.analytics.tracker.adjust;
using com.ktgame.analytics.tracker.firebase;

namespace com.ktgame.analytics.tracker.editor
{
    public class PackageInstaller
    {
            private const string PackageName = "com.ktgame.analytics.tracker";

            [MenuItem("Ktgame/Services/Settings/Analytics/Firebase")]
            private static void SelectionSettingsFirebase()
            {
                Selection.activeObject = FirebaseAnalyticServiceSettings.Instance;
            }
			
            [MenuItem("Ktgame/Services/Settings/Analytics/Adjust")]
            private static void SelectionSettingsAdjust()
            {
                Selection.activeObject = AdjustAnalyticServiceSettings.Instance;
            }
    }
}

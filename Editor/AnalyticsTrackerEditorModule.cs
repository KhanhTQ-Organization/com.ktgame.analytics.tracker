using com.ktgame.core.editor;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

#if FIREBASE_ANALYTICS
using com.ktgame.analytics.tracker.firebase;
#endif

#if ADJUST_ANALYTICS
using com.ktgame.analytics.tracker.adjust;
#endif

namespace com.ktgame.analytics.tracker.editor
{
    [InitializeOnLoad]
    public class AnalyticsTrackerEditorModule : IEditorDirtyHandler, IMenuTreeExtension
    {
        static AnalyticsTrackerEditorModule()
        {
            var module = new AnalyticsTrackerEditorModule();
            EditorDirtyRegistry.Register(module);
            MenuTreeExtensionRegistry.Register(module);
        }

        public void SetDirty()
        {
#if FIREBASE_ANALYTICS
            var firebase = FirebaseAnalyticServiceSettings.Instance;
            if (firebase != null)
            {
                EditorUtility.SetDirty(firebase);
            }

#endif

#if ADJUST_ANALYTICS
            var adjust = AdjustAnalyticServiceSettings.Instance;
            if (adjust != null)
            {
                EditorUtility.SetDirty(adjust);
            }
#endif
        }

        public void BuildMenu(OdinMenuTree tree)
        {
#if FIREBASE_ANALYTICS && !ADJUST_ANALYTICS
            tree.Add("Tracker Analytics",new AnalyticsTrackerEditor(KTWindow.Setting),KTEditor.GetIconComponent("firebase"));
#endif

#if ADJUST_ANALYTICS && !FIREBASE_ANALYTICS
            tree.Add("Tracker Analytics",new AnalyticsTrackerEditor(KTWindow.Setting),KTEditor.GetIconComponent("adjust"));
#endif

#if FIREBASE_ANALYTICS && ADJUST_ANALYTICS
            tree.Add("Tracker Analytics",new AnalyticsTrackerEditor(KTWindow.Setting),KTEditor.GetIconComponent("analytics"));
#endif
        }
    }
}
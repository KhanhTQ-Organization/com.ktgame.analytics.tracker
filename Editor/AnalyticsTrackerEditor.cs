using System.Collections.Generic;
using com.ktgame.core.editor;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

#if FIREBASE_ANALYTICS
using com.ktgame.analytics.tracker.firebase;
#endif

#if ADJUST_ANALYTICS
using com.ktgame.analytics.tracker.adjust;
#endif

namespace com.ktgame.analytics.tracker.editor
{
    public class AnalyticsTrackerEditor
    {
        [Title("Tracking Configuration", "Manage analytics events and user properties.", TitleAlignments.Centered)]
        [InfoBox("Add events and user properties below, then press Generate to update the code in Assets/Scripts/Generated. Names must follow Firebase naming rules.", InfoMessageType.Info)]
        [PropertyOrder(-10)]
        [ShowInInspector, HideLabel, DisplayAsString(false)]
        private string _header = "";

        private KTSettingSO _setting;

#if FIREBASE_ANALYTICS
        private FirebaseAnalyticServiceSettings _firebaseSettings;
#endif

#if ADJUST_ANALYTICS
        private AdjustAnalyticServiceSettings _adjustSettings;
#endif

        public AnalyticsTrackerEditor(KTSettingSO setting)
        {
            _setting = setting;

#if FIREBASE_ANALYTICS
            _firebaseSettings = FirebaseAnalyticServiceSettings.Instance;
#endif

#if ADJUST_ANALYTICS
            _adjustSettings = AdjustAnalyticServiceSettings.Instance;
#endif
        }

        #region Inspector GUI

        [PropertyOrder(-1)]
        [OnInspectorGUI]
        private void OnInspectorGUI()
        {
            if (GUI.changed)
            {
#if FIREBASE_ANALYTICS
                EditorUtility.SetDirty(_firebaseSettings);
#endif

#if ADJUST_ANALYTICS
                EditorUtility.SetDirty(_adjustSettings);
#endif
                AssetDatabase.SaveAssets();
            }
        }

        #endregion

#if FIREBASE_ANALYTICS

        [PropertySpace(10)]
        [LabelText("Log Events To Console (QA)")]
        [ShowInInspector]
        public bool FirebaseLogEventsToConsole
        {
            get => _firebaseSettings.LogEventsToConsole;
            set => _firebaseSettings.LogEventsToConsole = value;
        }

        [PropertySpace(20)]
        [Title("Firebase Events", Bold = true)]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
        [ShowInInspector]
        public List<com.ktgame.analytics.tracker.firebase.EventData.Event> FirebaseEvents
        {
            get => _firebaseSettings.EventData.Events;
            set => _firebaseSettings.EventData.Events = value;
        }

        [PropertySpace(10)]
        [Title("Firebase User Properties", Bold = true)]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
        [ShowInInspector]
        public List<UserPropertyData.UserProperties> FirebaseUserProperties
        {
            get => _firebaseSettings.UserPropertyData.Properties;
            set => _firebaseSettings.UserPropertyData.Properties = value;
        }

        [Button("Generate Firebase Event Code")]
        private void GenerateFirebaseEvent()
        {
            _firebaseSettings.GenerateEventEditor();
        }

        [Button("Generate Firebase User Property Code")]
        private void GenerateFirebaseUserProperty()
        {
            _firebaseSettings.GenerateUserProperty();
        }

#endif


#if ADJUST_ANALYTICS

        [PropertySpace(20)]
        [Title("Adjust Events", Bold = true)]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
        [ShowInInspector]
        public List<com.ktgame.analytics.tracker.adjust.EventData.Event> AdjustEvents
        {
            get => _adjustSettings.EventData.Events;
            set => _adjustSettings.EventData.Events = value;
        }

        [Button("Generate Adjust Event Code")]
        private void GenerateAdjustEvent()
        {
            _adjustSettings.GenerateEventEditor();
        }

#endif
    }
}
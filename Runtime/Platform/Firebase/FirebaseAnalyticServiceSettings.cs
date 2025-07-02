using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using com.ktgame.core;

#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
#endif

namespace com.ktgame.analytics.tracker.firebase
{
    public enum ValueType
    {
        Int,
        Long,
        Float,
        Double,
        String
    }

    [Serializable]
    public struct EventData
    {
        [Serializable]
        public struct Param
        {
            [SerializeField] private ValueType type;
            [SerializeField] private string key;

            public ValueType Type => type;

            public string Key => key;
        }

        [Serializable]
        public struct Event
        {
            [SerializeField] [FoldoutGroup("$name", expanded: false)]
            private string name;

            [SerializeField] [FoldoutGroup("$name", expanded: false)]
            private List<Param> @params;

            public string Name => name;

            public List<Param> Params => @params;
        }

        [SerializeField] private List<Event> events;

        public List<Event> Events => events;
    }

    [Serializable]
    public struct UserPropertyData
    {
        [Serializable]
        public struct UserProperties
        {
            [SerializeField] private string name;
            public string Name => name;
        }

        [SerializeField] private List<UserProperties> properties;

        public List<UserProperties> Properties => properties;
    }

    public class FirebaseAnalyticServiceSettings : ServiceSettingsSingleton<FirebaseAnalyticServiceSettings>
    {
        public override string PackageName => GetType().Namespace;

        [SerializeField] private EventData eventData;

        [SerializeField] private UserPropertyData userPropertyData;

        public EventData EventData => eventData;

#if UNITY_EDITOR
        [Button("Generate Event")]
        private void GenerateEvent()
        {
            if (eventData.Events.Count <= 0) return;
            var builder = new StringBuilder();
            builder.Append("using com.ktgame.analytics.tracker;").Append("\n").Append("\n");
            builder.AppendFormat("namespace {0}", PackageName).Append("\n");
            builder.Append("{").Append("\n");
            foreach (var @event in eventData.Events)
            {
                builder.Append("\t").AppendFormat("public struct FirebaseTracking_{0} : IEventData ", @event.Name).Append("\n");
                builder.Append("\t").Append("{").Append("\n");
                builder.Append("\t\t").AppendFormat("private const string EventName = \"{0}\"", @event.Name).Append(";").Append("\n");
                var paramString = "";
                for (var i = 0; i < @event.Params.Count; i++)
                {
                    var param = @event.Params[i];
                    switch (param.Type)
                    {
                        case ValueType.Int:
                            paramString += $"int {param.Key.Trim()}";
                            builder.Append("\t\t").AppendFormat("public int {0} ", param.Key.Trim()).Append("{ get; }").Append("\n");
                            break;
                        case ValueType.Long:
                            paramString += $"long {param.Key.Trim()}";
                            builder.Append("\t\t").AppendFormat("public long {0} ", param.Key.Trim()).Append("{ get; }").Append("\n");
                            break;
                        case ValueType.Float:
                            paramString += $"float {param.Key.Trim()}";
                            builder.Append("\t\t").AppendFormat("public float {0} ", param.Key.Trim()).Append("{ get; }").Append("\n");
                            break;
                        case ValueType.Double:
                            paramString += $"double {param.Key.Trim()}";
                            builder.Append("\t\t").AppendFormat("public double {0} ", param.Key.Trim()).Append("{ get; }").Append("\n");
                            break;
                        case ValueType.String:
                            paramString += $"string {param.Key.Trim()}";
                            builder.Append("\t\t").AppendFormat("public string {0} ", param.Key.Trim()).Append("{ get; }").Append("\n");
                            break;
                        default:
                            paramString += $"string {param.Key.Trim()}";
                            builder.Append("\t\t").AppendFormat("public string {0} ", param.Key.Trim()).Append("{ get; }").Append("\n");
                            break;
                    }

                    if (i < @event.Params.Count - 1) paramString += ", ";
                }

                if (@event.Params.Count > 0)
                {
                    builder.Append("\n");
                    builder.Append("\t\t").AppendFormat("public FirebaseTracking_{0}", @event.Name).Append("(").Append(paramString).Append(")").Append("\n");
                    builder.Append("\t\t").Append("{").Append("\n");
                    foreach (var param in @event.Params)
                    {
                        builder.Append("\t\t\t").AppendFormat("this.{0} = {1}", param.Key, param.Key).Append(";").Append("\n");
                    }

                    builder.Append("\t\t").Append("}").Append("\n");
                }

                builder.Append("\t").Append("}").Append("\n");
                builder.Append("\n");
            }

            builder.Append("}");
            var fileText = builder.ToString();
            
            var saveFolderPath = Path.Combine(Application.dataPath, "Scripts/Generated");
            var saveFilePath = Path.Combine(saveFolderPath, "FirebaseTrackingGenerate.cs");

            if (!Directory.Exists(saveFolderPath))
            {
                Directory.CreateDirectory(saveFolderPath);
            }

            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
            }

            if (File.Exists(saveFilePath + ".meta"))
            {
                File.Delete(saveFilePath + ".meta");
            }

            File.WriteAllText(saveFilePath, fileText, Encoding.UTF8);
            AssetDatabase.ImportAsset(saveFilePath);
            AssetDatabase.Refresh();
        }

        [Button("Generate User Property")]
        private void GenerateUserProperty()
        {
            var builder = new StringBuilder();
            builder.Append("using com.ktgame.analytics.tracker;").Append("\n").Append("\n");
            builder.AppendFormat("namespace {0}", PackageName).Append("\n");
            builder.Append("{").Append("\n");
            foreach (var property in userPropertyData.Properties)
            {
                builder.Append("\t").AppendFormat("public struct FirebaseUserProperty_{0} : IUserPropertyData ", property.Name).Append("\n");
                builder.Append("\t").Append("{").Append("\n");
                builder.Append("\t\t").AppendFormat("private const string PropertyName = \"{0}\"", property.Name).Append(";").Append("\n");
                builder.Append("\t\t").Append("public string Value ").Append("{ get; }").Append("\n");
                builder.Append("\n");
                builder.Append("\t\t").AppendFormat("public FirebaseUserProperty_{0}", property.Name).Append("(").Append("string value").Append(")")
                    .Append("\n");
                builder.Append("\t\t").Append("{").Append("\n");
                builder.Append("\t\t\t").Append("this.Value = value").Append(";").Append("\n");
                builder.Append("\t\t").Append("}").Append("\n");
                builder.Append("\t").Append("}").Append("\n");
                builder.Append("\n");
            }

            builder.Append("}");
            var fileText = builder.ToString();
            
            var saveFolderPath = Path.Combine(Application.dataPath, "Scripts/Generated");
            var saveFilePath = Path.Combine(saveFolderPath, "FirebaseUserPropertyGenerate.cs");

            if (!Directory.Exists(saveFolderPath))
            {
                Directory.CreateDirectory(saveFolderPath);
            }

            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
            }

            if (File.Exists(saveFilePath + ".meta"))
            {
                File.Delete(saveFilePath + ".meta");
            }

            File.WriteAllText(saveFilePath, fileText, Encoding.UTF8);
            AssetDatabase.ImportAsset(saveFilePath);
            AssetDatabase.Refresh();
        }
#endif
    }
}
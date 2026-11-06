#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace com.ktgame.analytics.tracker
{
	internal static class TrackingCodeGenerator
	{
		internal const string GeneratedFolder = "Assets/Scripts/Generated";

		internal struct GeneratedParam
		{
			public string TypeName;
			public string Name;
		}

		private static readonly HashSet<string> CSharpKeywords = new HashSet<string>
		{
			"abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const",
			"continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern",
			"false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface",
			"internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override",
			"params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
			"sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof",
			"uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
		};
		
		private static readonly HashSet<string> ReservedEventMembers = new HashSet<string>
		{
			"EventName", "GetEventName", "WriteParams"
		};
		
		internal static string Identifier(string name)
		{
			return CSharpKeywords.Contains(name) ? "@" + name : name;
		}

		internal static string EscapeLiteral(string value)
		{
			return (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
		}
		
		internal static bool ValidateEvent(string context, string structName, IList<GeneratedParam> parameters, ISet<string> usedStructNames)
		{
			var ok = true;

			if (!usedStructNames.Add(structName))
			{
				Debug.LogError($"[Analytics] {context}: struct name \"{structName}\" duplicates another event (after removing invalid characters).");
				ok = false;
			}

			var usedNames = new HashSet<string>();
			foreach (var param in parameters)
			{
				if (!usedNames.Add(param.Name))
				{
					Debug.LogError($"[Analytics] {context}: duplicate param \"{param.Name}\".");
					ok = false;
				}

				if (ReservedEventMembers.Contains(param.Name) || param.Name == structName)
				{
					Debug.LogError($"[Analytics] {context}: param \"{param.Name}\" conflicts with a reserved member of the generated struct, please rename it.");
					ok = false;
				}
			}

			return ok;
		}

		internal static void AppendEventStruct(StringBuilder builder, string structName, string eventName, IList<GeneratedParam> parameters)
		{
			builder.AppendLine("\t[UnityEngine.Scripting.Preserve]");
			builder.AppendLine($"\tpublic struct {structName} : IEventDataWriter");
			builder.AppendLine("\t{");
			builder.AppendLine($"\t\tprivate const string EventName = \"{EscapeLiteral(eventName)}\";");

			foreach (var param in parameters)
			{
				builder.AppendLine($"\t\tpublic {param.TypeName} {Identifier(param.Name)} {{ get; }}");
			}

			if (parameters.Count > 0)
			{
				var arguments = new List<string>();
				foreach (var param in parameters)
				{
					arguments.Add($"{param.TypeName} {Identifier(param.Name)}");
				}

				builder.AppendLine();
				builder.AppendLine($"\t\tpublic {structName}({string.Join(", ", arguments)})");
				builder.AppendLine("\t\t{");
				foreach (var param in parameters)
				{
					var id = Identifier(param.Name);
					builder.AppendLine($"\t\t\tthis.{id} = {id};");
				}

				builder.AppendLine("\t\t}");
			}

			builder.AppendLine();
			builder.AppendLine("\t\tpublic string GetEventName() => EventName;");
			builder.AppendLine();
			builder.AppendLine("\t\tpublic void WriteParams(IEvent analyticEvent)");
			builder.AppendLine("\t\t{");
			foreach (var param in parameters)
			{
				builder.AppendLine($"\t\t\t{WriteStatement(param)}");
			}

			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			builder.AppendLine();
		}
		
		private static string WriteStatement(GeneratedParam param)
		{
			var key = EscapeLiteral(param.Name);
			var value = "this." + Identifier(param.Name);

			switch (param.TypeName)
			{
				case "int":
					return $"analyticEvent.AddIntParam(\"{key}\", {value});";
				case "long":
					return $"analyticEvent.AddLongParam(\"{key}\", {value});";
				case "float":
					return $"analyticEvent.AddFloatParam(\"{key}\", {value});";
				case "double":
					return $"analyticEvent.AddDoubleParam(\"{key}\", {value});";
				case "bool":
					return $"analyticEvent.AddStringParam(\"{key}\", {value} ? \"true\" : \"false\");";
				default:
					return $"if ({value} != null) analyticEvent.AddStringParam(\"{key}\", {value});";
			}
		}

		internal static void AppendUserPropertyStruct(StringBuilder builder, string structName, string propertyName)
		{
			builder.AppendLine("\t[UnityEngine.Scripting.Preserve]");
			builder.AppendLine($"\tpublic struct {structName} : IUserPropertyDataWriter");
			builder.AppendLine("\t{");
			builder.AppendLine($"\t\tprivate const string PropertyName = \"{EscapeLiteral(propertyName)}\";");
			builder.AppendLine("\t\tpublic string Value { get; }");
			builder.AppendLine();
			builder.AppendLine($"\t\tpublic {structName}(string value)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tthis.Value = value;");
			builder.AppendLine("\t\t}");
			builder.AppendLine();
			builder.AppendLine("\t\tpublic string GetPropertyName() => PropertyName;");
			builder.AppendLine();
			builder.AppendLine("\t\tpublic string GetPropertyValue() => Value;");
			builder.AppendLine("\t}");
			builder.AppendLine();
		}

		internal static void SaveToFile(string fileName, string content)
		{
			var assetPath = $"{GeneratedFolder}/{fileName}";
			var fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
			var folder = Path.GetDirectoryName(fullPath);

			var folderCreated = false;
			if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
				folderCreated = true;
			}

			if (File.Exists(fullPath) && File.ReadAllText(fullPath) == content)
			{
				Debug.Log($"[Analytics] {assetPath} is unchanged.");
				return;
			}

			File.WriteAllText(fullPath, content, new UTF8Encoding(true));
			if (folderCreated)
			{
				AssetDatabase.Refresh();
			}
			else
			{
				AssetDatabase.ImportAsset(assetPath);
			}

			Debug.Log($"<color=green>[Analytics] Generated {assetPath}</color>");
		}
	}
}
#endif

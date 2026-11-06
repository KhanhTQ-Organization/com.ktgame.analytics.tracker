using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace com.ktgame.analytics.tracker.firebase
{
	public static class FirebaseNameRules
	{
		public const int MaxNameLength = 40;
		public const int MaxParams = 25;
		public const int MaxStringValueLength = 100;

		private static readonly Regex NamePattern = new Regex("^[A-Za-z][A-Za-z0-9_]*$");
		private static readonly string[] ReservedPrefixes = { "firebase_", "google_", "ga_" };
		
		private static readonly HashSet<string> ReservedEvents = new HashSet<string>
		{
			"ad_activeview", "ad_click", "ad_exposure", "ad_query", "ad_reward", "adunit_exposure",
			"app_background", "app_clear_data", "app_exception", "app_remove", "app_store_refund",
			"app_store_subscription_cancel", "app_store_subscription_convert", "app_store_subscription_renew",
			"app_update", "app_upgrade", "dynamic_link_app_open", "dynamic_link_app_update",
			"dynamic_link_first_open", "error", "first_open", "first_visit", "in_app_purchase",
			"notification_dismiss", "notification_foreground", "notification_open", "notification_receive",
			"os_update", "session_start", "session_start_with_rollout", "user_engagement"
		};
		
		public static string ValidateEventName(string name)
		{
			var error = ValidateName(name);
			if (error != null)
			{
				return error;
			}

			return ReservedEvents.Contains(name) ? "is a reserved Firebase event name" : null;
		}
		
		public static string ValidateParamName(string name)
		{
			return ValidateName(name);
		}

		private static string ValidateName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return "is null or empty";
			}

			if (name.Length > MaxNameLength)
			{
				return $"is {name.Length} characters long (max {MaxNameLength})";
			}

			if (!NamePattern.IsMatch(name))
			{
				return "must start with a letter and contain only letters, digits and underscores";
			}

			foreach (var prefix in ReservedPrefixes)
			{
				if (name.StartsWith(prefix))
				{
					return $"must not start with \"{prefix}\"";
				}
			}

			return null;
		}
	}
}

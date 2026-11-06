using System;
using System.Collections.Generic;
using com.ktgame.analytics.tracker;
using Cysharp.Threading.Tasks;
using com.ktgame.core;
using UnityEngine;

#if FIREBASE_ANALYTICS
using Firebase;
using Firebase.Analytics;
using com.ktgame.core.di;
using com.ktgame.services.firebase;
#endif

namespace com.ktgame.analytics.tracker.firebase
{
	[Service(typeof(IFirebaseAnalyticService))]
	public class FirebaseAnalyticService : MonoBehaviour, IFirebaseAnalyticService
	{
		private const int MaxPendingActions = 200;
		private const float FirebaseWaitTimeoutSeconds = 10f;

		public int Priority => 1;
		public bool Initialized { get; set; }
		private AnalyticTracker _tracker;
		private ITrackingProvider _provider;

		private readonly Queue<Action> _pending = new Queue<Action>();
		private bool _pendingOverflowWarned;

		public async UniTask OnInitialize(IArchitecture architecture)
		{
			var settings = FirebaseAnalyticServiceSettings.Instance;
			if (settings != null)
			{
				AnalyticTracker.LogEnabled |= settings.LogEventsToConsole;
			}

#if FIREBASE_ANALYTICS
			var firebaseService = architecture.GetService<IFirebaseService>();
			if (firebaseService == null)
			{
				Debug.LogError("[Analytics] IFirebaseService not found. Events will only be logged to the Console (not sent to Firebase).");
				SetupTracker(new NullTrackingProvider(true));
				Initialized = true;
				return;
			}

			if (!firebaseService.Initialized)
			{
				var deadline = Time.realtimeSinceStartup + FirebaseWaitTimeoutSeconds;
				await UniTask.WaitUntil(() => firebaseService.Initialized || Time.realtimeSinceStartup >= deadline,
					cancellationToken: this.GetCancellationTokenOnDestroy());
			}

			if (firebaseService.Initialized)
			{
				SetupTracker(new FirebaseTrackingProvider());
			}
			else
			{
				Debug.LogWarning($"[Analytics] Firebase not ready after {FirebaseWaitTimeoutSeconds}s, continuing startup. Events are queued until Firebase is ready.");
				SetupTrackerWhenFirebaseReady(firebaseService).Forget();
			}
#else
			SetupTracker(new NullTrackingProvider(true));
			await UniTask.CompletedTask;
#endif

			Initialized = true;
		}

		private void SetupTracker(ITrackingProvider provider)
		{
			_provider = provider;
			_tracker = new AnalyticTracker(provider);
			FlushPending();
		}

#if FIREBASE_ANALYTICS
		private async UniTaskVoid SetupTrackerWhenFirebaseReady(IFirebaseService firebaseService)
		{
			try
			{
				await UniTask.WaitUntil(() => firebaseService.Initialized, cancellationToken: this.GetCancellationTokenOnDestroy());
			}
			catch (OperationCanceledException)
			{
				return;
			}

			SetupTracker(new FirebaseTrackingProvider());
			Debug.Log("[Analytics] Firebase is ready, queued events have been sent.");
		}
#endif

		public void SetUserId(string id)
		{
			if (!IsReadyOrQueue(() => SetUserId(id)))
			{
				return;
			}

			try
			{
				_provider.SetUserId(id);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		public void SetUserProperty(IUserPropertyData userPropertyData)
		{
			if (!IsReadyOrQueue(() => SetUserProperty(userPropertyData)))
			{
				return;
			}

			_tracker.SetUserProperty(userPropertyData);
		}

		public void LogEvent(IEventData eventData)
		{
			if (!IsReadyOrQueue(() => LogEvent(eventData)))
			{
				return;
			}

			_tracker.LogEvent(eventData);
		}

		public void LogEvent(string eventName, params AnalyticParam[] parameters)
		{
			if (!IsReadyOrQueue(() => LogEvent(eventName, parameters)))
			{
				return;
			}

			_tracker.LogEvent(eventName, parameters);
		}

		public void SetConsent(bool analyticsStorage, bool adStorage, bool adUserData, bool adPersonalization)
		{
			if (!IsReadyOrQueue(() => SetConsent(analyticsStorage, adStorage, adUserData, adPersonalization)))
			{
				return;
			}

#if FIREBASE_ANALYTICS
			try
			{
				FirebaseAnalytics.SetConsent(new Dictionary<ConsentType, ConsentStatus>
				{
					{ ConsentType.AnalyticsStorage, analyticsStorage ? ConsentStatus.Granted : ConsentStatus.Denied },
					{ ConsentType.AdStorage, adStorage ? ConsentStatus.Granted : ConsentStatus.Denied },
					{ ConsentType.AdUserData, adUserData ? ConsentStatus.Granted : ConsentStatus.Denied },
					{ ConsentType.AdPersonalization, adPersonalization ? ConsentStatus.Granted : ConsentStatus.Denied },
				});
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
#endif

			if (AnalyticTracker.LogEnabled)
			{
				Debug.Log($"[Analytics] Consent analytics={analyticsStorage}, ad_storage={adStorage}, ad_user_data={adUserData}, ad_personalization={adPersonalization}");
			}
		}
		
		private bool IsReadyOrQueue(Action action)
		{
			if (_tracker != null)
			{
				return true;
			}

			if (_pending.Count >= MaxPendingActions)
			{
				if (!_pendingOverflowWarned)
				{
					_pendingOverflowWarned = true;
					Debug.LogWarning($"[Analytics] Event queue is full ({MaxPendingActions}). Further events will be dropped until Firebase is ready.");
				}

				return false;
			}

			_pending.Enqueue(action);
			return false;
		}

		private void FlushPending()
		{
			while (_pending.Count > 0)
			{
				var action = _pending.Dequeue();
				try
				{
					action();
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}
		}
	}
}

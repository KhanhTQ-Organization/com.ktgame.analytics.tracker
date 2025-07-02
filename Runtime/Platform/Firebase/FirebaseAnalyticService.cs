using com.ktgame.analytics.tracker;
using Cysharp.Threading.Tasks;
using com.ktgame.core;
using UnityEngine;

#if FIREBASE_ANALYTICS
using Firebase;
using com.ktgame.core.di;
using com.ktgame.services.firebase;
using com.ktgame.analytics.provider.firebase;
using com.ktgame.services.device;
#endif

namespace com.ktgame.analytics.tracker.firebase
{
	[Service(typeof(IFirebaseAnalyticService))]
	public class FirebaseAnalyticService : MonoBehaviour, IFirebaseAnalyticService
	{
		public int Priority => 1;
		public bool Initialized { get; set; }

#if FIREBASE_ANALYTICS
		[Inject] private readonly IDeviceService _deviceService;
#endif

		private IAnalyticTracker _tracker;
		private ITrackingProvider _provider;

		public async UniTask OnInitialize(IArchitecture architecture)
		{
#if FIREBASE_ANALYTICS
            var firebaseService = architecture.GetService<IFirebaseService>();
            await UniTask.WaitUntil(() => firebaseService.Initialized);
            
            var externalDeviceId = string.Empty;
            if (_deviceService != null)
            {
                externalDeviceId = _deviceService.ID;
            }
            
            _provider = new FirebaseTrackingProvider();
            _provider.SetUserId(externalDeviceId);
#else
			_provider = new NullTrackingProvider();
#endif

			_tracker = new AnalyticTracker(_provider);
			Initialized = true;
		}

		public void SetUserId(string id)
		{
			_provider?.SetUserId(id);
		}

		public void SetUserProperty(IUserPropertyData userPropertyData)
		{
			_tracker?.SetUserProperty(userPropertyData);
		}

		public void LogEvent(IEventData eventData)
		{
			_tracker?.LogEvent(eventData);
		}
	}
}

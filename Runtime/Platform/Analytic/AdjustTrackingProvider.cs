using System;
using com.ktgame.analytics.tracker;

#if ADJUST_ANALYTICS
using AdjustSdk;
#endif

namespace com.ktgame.analytics.tracker.adjust
{
    public class AdjustTrackingProvider : ITrackingProvider
    {
        public bool IsReady { get; private set; }

#if ADJUST_ANALYTICS
        private readonly AdjustConfig _config;
#endif

#if ADJUST_ANALYTICS
        private AdjustTrackingProvider(Builder builder)
        {
            var config = new AdjustConfig(builder.AppToken, builder.Environment);

            config.LogLevel = builder.LogLevel;
            config.IsSendingInBackgroundEnabled = builder.SendInBackground;
            config.IsDeferredDeeplinkOpeningEnabled = builder.LaunchDeferredDeeplink;

            if (!string.IsNullOrEmpty(builder.ExternalDeviceId))
                config.ExternalDeviceId = builder.ExternalDeviceId;

            if (builder.DeferredDeeplinkCallback != null)
                config.DeferredDeeplinkDelegate = builder.DeferredDeeplinkCallback;

            _config = config;

            Adjust.InitSdk(_config);

            IsReady = true;
        }
#else
        private AdjustTrackingProvider()
        {
            IsReady = false;
        }
#endif

        public ITrackingProvider SetUserId(string id)
        {
#if ADJUST_ANALYTICS
            Adjust.AddGlobalCallbackParameter("user_id", id);
#endif
            return this;
        }

        public ITrackingProvider SetUserProperty(string id, string value)
        {
#if ADJUST_ANALYTICS
            Adjust.AddGlobalCallbackParameter(id, value);
#endif
            return this;
        }

        public IEvent NewEvent(string eventId)
        {
#if ADJUST_ANALYTICS
            return new AdjustEventWrapper(eventId);
#else
            return null;
#endif
        }

        public class Builder
        {
#if ADJUST_ANALYTICS
            public string AppToken { get; }
            public AdjustEnvironment Environment { get; }

            public AdjustLogLevel LogLevel { get; private set; } = AdjustLogLevel.Info;
            public bool SendInBackground { get; private set; }
            public bool LaunchDeferredDeeplink { get; private set; }
            public Action<string> DeferredDeeplinkCallback { get; private set; }
            public string ExternalDeviceId { get; private set; }

            public Builder(string appToken, AdjustEnvironment environment)
            {
                AppToken = appToken;
                Environment = environment;
            }

            public Builder WithLogLevel(AdjustLogLevel logLevel)
            {
                LogLevel = logLevel;
                return this;
            }
#else
            public Builder(string appToken, object environment) { }

            public Builder WithLogLevel(object logLevel)
            {
                return this;
            }
#endif

            public Builder WithSendInBackground(bool value)
            {
#if ADJUST_ANALYTICS
                SendInBackground = value;
#endif
                return this;
            }

            public Builder WithLaunchDeferredDeeplink(bool value)
            {
#if ADJUST_ANALYTICS
                LaunchDeferredDeeplink = value;
#endif
                return this;
            }

            public Builder WithDeferredDeeplinkCallback(Action<string> callback)
            {
#if ADJUST_ANALYTICS
                DeferredDeeplinkCallback = callback;
#endif
                return this;
            }

            public Builder WithExternalDeviceId(string id)
            {
#if ADJUST_ANALYTICS
                ExternalDeviceId = id;
#endif
                return this;
            }

            public AdjustTrackingProvider Build()
            {
#if ADJUST_ANALYTICS
                return new AdjustTrackingProvider(this);
#else
                return new AdjustTrackingProvider();
#endif
            }
        }
    }
}
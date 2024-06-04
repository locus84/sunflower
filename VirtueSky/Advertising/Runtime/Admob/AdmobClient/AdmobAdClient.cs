#if VIRTUESKY_ADS && ADS_ADMOB
using GoogleMobileAds.Api;
#endif
using UnityEngine;
using VirtueSky.Inspector;
using VirtueSky.Core;
using VirtueSky.Tracking;

namespace VirtueSky.Ads
{
    [EditorIcon("icon_scriptable")]
    public sealed class AdmobAdClient : AdClient
    {
        public override void Initialize()
        {
#if VIRTUESKY_ADS && ADS_ADMOB
            // On Android, Unity is paused when displaying interstitial or rewarded video.
            // This setting makes iOS behave consistently with Android.
            MobileAds.SetiOSAppPauseOnBackground(true);

            // When true all events raised by GoogleMobileAds will be raised
            // on the Unity main thread. The default value is false.
            // https://developers.google.com/admob/unity/quick-start#raise_ad_events_on_the_unity_main_thread
            MobileAds.RaiseAdEventsOnUnityMainThread = true;

            adSetting.AdmobBannerVariable.Init();
            adSetting.AdmobInterVariable.Init();
            adSetting.AdmobRewardVariable.Init();
            adSetting.AdmobRewardInterVariable.Init();
            adSetting.AdmobAppOpenVariable.Init();

            MobileAds.Initialize(initStatus =>
            {
                App.RunOnMainThread(() =>
                {
                    if (!adSetting.AdmobEnableTestMode) return;
                    var configuration = new RequestConfiguration
                        { TestDeviceIds = adSetting.AdmobDevicesTest };
                    MobileAds.SetRequestConfiguration(configuration);
                });
            });
            adSetting.AdmobBannerVariable.paidedCallback = AppTracking.TrackRevenue;
            adSetting.AdmobInterVariable.paidedCallback = AppTracking.TrackRevenue;
            adSetting.AdmobRewardVariable.paidedCallback = AppTracking.TrackRevenue;
            adSetting.AdmobRewardInterVariable.paidedCallback = AppTracking.TrackRevenue;
            adSetting.AdmobAppOpenVariable.paidedCallback = AppTracking.TrackRevenue;
            RegisterAppStateChange();
            LoadInterstitial();
            LoadRewarded();
            LoadRewardedInterstitial();
            LoadAppOpen();
#endif
        }

        public override void LoadInterstitial()
        {
#if VIRTUESKY_ADS && ADS_ADMOB
            if (!IsInterstitialReady()) adSetting.AdmobInterVariable.Load();
#endif
        }

        public override bool IsInterstitialReady()
        {
#if VIRTUESKY_ADS && ADS_ADMOB
            return adSetting.AdmobInterVariable.IsReady();
#else
            return false;
#endif
        }

        public override void LoadRewarded()
        {
#if VIRTUESKY_ADS && ADS_ADMOB
            if (!IsRewardedReady()) adSetting.AdmobRewardVariable.Load();
#endif
        }

        public override bool IsRewardedReady()
        {
#if VIRTUESKY_ADS && ADS_ADMOB
            return adSetting.AdmobRewardVariable.IsReady();
#else
            return false;
#endif
        }

        public override void LoadRewardedInterstitial()
        {
#if VIRTUESKY_ADS && ADS_ADMOB
            if (!IsRewardedInterstitialReady()) adSetting.AdmobRewardInterVariable.Load();
#endif
        }

        public override bool IsRewardedInterstitialReady()
        {
#if VIRTUESKY_ADS && ADS_ADMOB
            return adSetting.AdmobRewardInterVariable.IsReady();
#else
            return false;
#endif
        }

        public override void LoadAppOpen()
        {
#if VIRTUESKY_ADS && ADS_ADMOB
            if (!IsAppOpenReady()) adSetting.AdmobAppOpenVariable.Load();
#endif
        }

        public override bool IsAppOpenReady()
        {
#if VIRTUESKY_ADS && ADS_ADMOB
            return adSetting.AdmobAppOpenVariable.IsReady();
#else
            return false;
#endif
        }

        void ShowAppOpen()
        {
#if VIRTUESKY_ADS && ADS_ADMOB
            if (statusAppOpenFirstIgnore) adSetting.AdmobAppOpenVariable.Show();
            statusAppOpenFirstIgnore = true;
#endif
        }
#if VIRTUESKY_ADS && ADS_ADMOB
        public void RegisterAppStateChange()
        {
            GoogleMobileAds.Api.AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
        }

        void OnAppStateChanged(GoogleMobileAds.Common.AppState state)
        {
            if (state == GoogleMobileAds.Common.AppState.Foreground && adSetting.AdmobAppOpenVariable.AutoShow)
            {
                if (adSetting.CurrentAdNetwork == AdNetwork.Admob) ShowAppOpen();
            }
        }
#endif
    }
}
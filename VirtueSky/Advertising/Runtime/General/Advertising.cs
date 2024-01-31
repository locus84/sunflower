using System.Collections;

#if ADS_ADMOB
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
#endif

using UnityEngine;
#if UNITY_EDITOR
using VirtueSky.UtilsEditor;
#endif
using VirtueSky.Events;
using VirtueSky.Variables;

namespace VirtueSky.Ads
{
    public class Advertising : MonoBehaviour
    {
        [SerializeField] private AdSetting adSetting;
        [SerializeField] private BooleanEvent changePreventDisplayAppOpenEvent;
        [SerializeField] private BooleanVariable isGDPRCanRequestAds;
        private IEnumerator autoLoadAdCoroutine;
        private float _lastTimeLoadInterstitialAdTimestamp = DEFAULT_TIMESTAMP;
        private float _lastTimeLoadRewardedTimestamp = DEFAULT_TIMESTAMP;
        private float _lastTimeLoadRewardedInterstitialTimestamp = DEFAULT_TIMESTAMP;
        private float _lastTimeLoadAppOpenTimestamp = DEFAULT_TIMESTAMP;
        private const float DEFAULT_TIMESTAMP = -1000;

        private AdClient currentAdClient;

        private void Start()
        {
            switch (adSetting.CurrentAdNetwork)
            {
                case AdNetwork.Max:
                    currentAdClient = adSetting.MaxAdClient;
                    break;
                case AdNetwork.Admob:
                    currentAdClient = adSetting.AdmobAdClient;
                    break;
            }

            if (changePreventDisplayAppOpenEvent != null)
                changePreventDisplayAppOpenEvent.AddListener(OnChangePreventDisplayOpenAd);
            if (adSetting.AdmobEnableGDPR)
            {
                InitGDPR();
            }
            else
            {
                InitAutoLoadAds();
            }
        }

        public void InitAutoLoadAds()
        {
            currentAdClient.Initialize();
            if (autoLoadAdCoroutine != null) StopCoroutine(autoLoadAdCoroutine);
            autoLoadAdCoroutine = IeAutoLoadAll();
            StartCoroutine(autoLoadAdCoroutine);
            Debug.Log("currentAdClient: " + currentAdClient.name);
        }

        public void OnChangePreventDisplayOpenAd(bool state)
        {
            AdStatic.isShowingAd = state;
        }

        IEnumerator IeAutoLoadAll(float delay = 0)
        {
            if (delay > 0) yield return new WaitForSeconds(delay);
            while (true)
            {
                AutoLoadInterAds();
                AutoLoadRewardAds();
                AutoLoadRewardInterAds();
                AutoLoadAppOpenAds();
                yield return new WaitForSeconds(adSetting.AdCheckingInterval);
            }
        }

        #region Func Load Ads

        void AutoLoadInterAds()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadInterstitialAdTimestamp <
                adSetting.AdLoadingInterval) return;
            currentAdClient.LoadInterstitial();
            _lastTimeLoadInterstitialAdTimestamp = Time.realtimeSinceStartup;
        }

        void AutoLoadRewardAds()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadRewardedTimestamp <
                adSetting.AdLoadingInterval) return;
            currentAdClient.LoadRewarded();
            _lastTimeLoadRewardedTimestamp = Time.realtimeSinceStartup;
        }

        void AutoLoadRewardInterAds()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadRewardedInterstitialTimestamp <
                adSetting.AdLoadingInterval) return;
            currentAdClient.LoadRewardedInterstitial();
            _lastTimeLoadRewardedInterstitialTimestamp = Time.realtimeSinceStartup;
        }

        void AutoLoadAppOpenAds()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadAppOpenTimestamp <
                adSetting.AdLoadingInterval) return;
            currentAdClient.LoadAppOpen();
            _lastTimeLoadAppOpenTimestamp = Time.realtimeSinceStartup;
        }

        #endregion

        #region Admob GDPR

#if ADS_ADMOB
        public void InitGDPR()
        {
#if !UNITY_EDITOR
            string deviceID = SystemInfo.deviceUniqueIdentifier;
            string deviceIDUpperCase = deviceID.ToUpper();
            Debug.Log("TestDeviceHashedId = " + deviceIDUpperCase);
            if (adSetting.IsTestGDPR)
            {
                ConsentRequestParameters request = new ConsentRequestParameters
                {
                    TagForUnderAgeOfConsent = false,
                    ConsentDebugSettings = new ConsentDebugSettings()
                    {
                        DebugGeography = DebugGeography.EEA,
                        TestDeviceHashedIds = adSetting.AdmobDevicesTest
                    }
                };

                ConsentInformation.Update(request, OnConsentInfoUpdated);
            }
            else
            {
                ConsentRequestParameters request = new ConsentRequestParameters
                {
                    TagForUnderAgeOfConsent = false,
                };

                ConsentInformation.Update(request, OnConsentInfoUpdated);
            }
#endif
        }

        private void OnConsentInfoUpdated(FormError consentError)
        {
            if (consentError != null)
            {
                Debug.Log("error consentError = " + consentError);
                return;
            }

            ConsentForm.LoadAndShowConsentFormIfRequired(
                (FormError formError) =>
                {
                    if (formError != null)
                    {
                        Debug.Log("error consentError = " + consentError);
                        return;
                    }

                    Debug.Log("ConsentStatus = " + ConsentInformation.ConsentStatus.ToString());
                    Debug.Log("CanRequestAds = " + ConsentInformation.CanRequestAds());
                    if (ConsentInformation.CanRequestAds())
                    {
                        MobileAds.RaiseAdEventsOnUnityMainThread = true;
                        InitAutoLoadAds();
                        if (isGDPRCanRequestAds != null)
                        {
                            isGDPRCanRequestAds.Value = true;
                        }
                    }
                    else
                    {
                        if (isGDPRCanRequestAds != null)
                        {
                            isGDPRCanRequestAds.Value = false;
                        }
                    }
                }
            );
        }

        public void LoadAndShowConsentForm()
        {
            Debug.Log("LoadAndShowConsentForm Start!");

            ConsentForm.Load((consentForm, loadError) =>
            {
                if (loadError != null)
                {
                    Debug.Log("error loadError = " + loadError);
                    return;
                }


                consentForm.Show(showError =>
                {
                    if (showError != null)
                    {
                        Debug.Log("error showError = " + showError);
                        return;
                    }
                });
            });
        }

        public void ResetGDPR()
        {
#if !UNITY_EDITOR
            Debug.Log("Reset GDPR!");
            ConsentInformation.Reset();
#endif
        }
#endif

        #endregion

#if ADS_APPLOVIN
        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus) (currentAdClient as MaxAdClient)?.ShowAppOpen();
        }
#endif

#if UNITY_EDITOR
        private void Reset()
        {
            adSetting = CreateAsset.CreateAndGetScriptableAsset<VirtueSky.Ads.AdSetting>("/Ads");
        }
#endif
    }
}
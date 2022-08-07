using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Analytics;

public class AdsManager : MonoBehaviour, IUnityAdsListener
{
    [Header("** Game Essentials **")]
    public string AppleStoreAppId;

    [Header("Admob Banner Settings")]
    public GoogleMobileAds.Api.AdPosition GeneralAdmobBannerAdPostion;

    [Header("Enter Android Ad Ids Here....")]
    public AdIds AndroidAdIds;
    [Header("Enter iOS Ad Ids Here....")]
    public AdIds iOSAdIds;
    [Header("** Select Ad Priorties Here **")]
    public AdsDisplayProperties LevelOver;
    public AdsDisplayProperties LevelPause, BackToMenu, OtherAreas;
    [Header("Select Rewarded Ad Priorties Here")]
    public RewardedAds rewardedAds;

    [Header("Test Mode")]
    public bool EnableTestMode;

    #region ADDITIONAL_VARIABLES

    string ProductName;
    string PackageName;

    public delegate void rewardedAdShown();
    public static event rewardedAdShown OnRewardedAdShown;

    string AdmobBannerId, AdmobInterstitialId, AdmobRewardedId, UnityAdId;

    string ProductVersion;
    string[] PrivacyLinks, MoreAppsLinks;
    string MoreAppsLink;
    string RateAppsLink;
    string PrivacyPolicyLink;
    private bool RemoveAds, DisplayExitDialoug;

    public static AdsManager Instance;

    #endregion

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
        AdsEssentials();
        ShowConsetDialoug();
    }

    private void Start()
    {
        if (!AreAdsRemoved())
        {
            SelectIDsPlatform();
        }
    }

    #region AdsEssentials

    void AdsEssentials()
    {
        ProductName = Application.productName;
        PackageName = Application.identifier;
        ProductVersion = Application.version;

        MoreAppsLinks = new string[4];
        PrivacyLinks = new string[5];

        MoreAppsLinks[0] = "https://play.google.com/store/apps/developer?id=Lodgers%20Games&hl=en";
        MoreAppsLinks[1] = "https://play.google.com/store/apps/developer?id=Mayhem+Studio";
        MoreAppsLinks[2] = "https://play.google.com/store/apps/developer?id=E404+Games";
        MoreAppsLinks[3] = "https://play.google.com/store/apps/developer?id=PopCorn+Games";

        PrivacyLinks[0] = "https://sites.google.com/view/lodgersgamesprivacypolicy/";
        PrivacyLinks[1] = "https://sites.google.com/view/mayhem-studios-privacy-policy/home";
        PrivacyLinks[2] = "https://sites.google.com/view/e404-games-privacy-policy/home";
        PrivacyLinks[3] = "https://sites.google.com/view/popcorngamesprivacypolicy/home";
        PrivacyLinks[4] = "https://sites.google.com/view/pcppprivacypolicy/home";

        if (PackageName.Contains("com.lodgers."))
        {
#if UNITY_ANDROID
            MoreAppsLink = MoreAppsLinks[0];
#elif UNITY_IPHONE
            MoreAppsLink = "";
#endif
            PrivacyPolicyLink = PrivacyLinks[0];
        }
        else if (PackageName.Contains("com.mayhem."))
        {
#if UNITY_ANDROID
            MoreAppsLink = MoreAppsLinks[1];
#elif UNITY_IPHONE
            MoreAppsLink = "https://apps.apple.com/us/developer/shoaib-sadaqat/id1471164171";
#endif
            PrivacyPolicyLink = PrivacyLinks[1];
        }
        else if (PackageName.Contains("com.ee."))
        {
            MoreAppsLink = MoreAppsLinks[2];
            PrivacyPolicyLink = PrivacyLinks[2];
        }
        else if (PackageName.Contains("com.pcg."))
        {
            MoreAppsLink = MoreAppsLinks[3];
            PrivacyPolicyLink = PrivacyLinks[3];
        }
        else if (PackageName.Contains("com.haider."))
        {
            //MoreAppsLink = MoreAppsLinks[3];
            PrivacyPolicyLink = PrivacyLinks[4];
        }
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    public bool AreAdsRemoved()
    {
        if (PlayerPrefs.GetInt("removeAds") == 1)
        {
            RemoveAds = true;
            UnityLogs("Remove ads purchased");
            return true;
        }
        return false;
    }

    public void RemoveAllAds()
    {
        PlayerPrefs.SetInt("removeAds", 1);
        RemoveAds = true;
    }

    #endregion

    #region AdsInit

    void SelectIDsPlatform()
    {
#if UNITY_ANDROID

        AdmobBannerId = AndroidAdIds.AdmobBannerId;
        AdmobInterstitialId = AndroidAdIds.AdmobInterstitalId;
        AdmobRewardedId = AndroidAdIds.AdmobRewardedAdId;
        UnityAdId = AndroidAdIds.UnityAdId;

#elif UNITY_IPHONE

        AdmobBannerId = iOSAdIds.AdmobBannerId;
        AdmobInterstitialId = iOSAdIds.AdmobInterstitalId;
        AdmobRewardedId = iOSAdIds.AdmobRewardedAdId;
        UnityAdId = iOSAdIds.UnityAdId;

#endif
        if (EnableTestMode)
        {
            AdmobInterstitialId = "ca-app-pub-3940256099942544/1033173712";
            AdmobBannerId = "ca-app-pub-3940256099942544/6300978111";
            AdmobRewardedId = "ca-app-pub-3940256099942544/5224354917";
        }
        RemoveWhiteSpaces();
        AdInit();
    }

    void RemoveWhiteSpaces()
    {
        AdmobBannerId = AdmobBannerId.Trim();
        AdmobInterstitialId = AdmobInterstitialId.Trim();
        AdmobRewardedId = AdmobRewardedId.Trim();
        UnityAdId = UnityAdId.Trim();
    }


    void AdInit()
    {
        try
        {
            MobileAds.Initialize(initStatus => { });
            RequestAdmobBanner();
            RequestAdmobInterstitialAd();
            RequestAdmobRewardedAd();
            Advertisement.AddListener(this);
            Advertisement.Initialize(UnityAdId, EnableTestMode);
        }
        catch (Exception)
        {
            throw;
        }
    }

    #endregion

    #region DISPLAY_AD

    #region InterstitialAd_Zone

    public void ShowGameOverAd()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            if (!AreAdsRemoved())
            {
                if (DisplayFirstPriortyAd(LevelOver))
                {
                    Debug.Log("TT AD Stuff : 1st Priority Ad on Element was sucessfull");
                }
                else if (DisplaySecondPriortyAd(LevelOver))
                {
                    Debug.Log("TT AD Stuff : 2nd Priority Ad on Element was sucessfull");
                }
            }
        }
    }

    public void ShowLevelPauseAd()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            if (!AreAdsRemoved())
            {
                if (DisplayFirstPriortyAd(LevelPause))
                {
                    Debug.Log("TT AD Stuff : 1st Priority Ad on Element was sucessfull");
                }
                else if (DisplaySecondPriortyAd(LevelPause))
                {
                    Debug.Log("TT AD Stuff : 2nd Priority Ad on Element was sucessfull");
                }
            }
        }
    }

    public void ShowBackToMenuAd()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            if (!AreAdsRemoved())
            {
                if (DisplayFirstPriortyAd(BackToMenu))
                {
                    Debug.Log("TT AD Stuff : 1st Priority Ad on Element was sucessfull");
                }
                else if (DisplaySecondPriortyAd(BackToMenu))
                {
                    Debug.Log("TT AD Stuff : 2nd Priority Ad on Element was sucessfull");
                }
            }
        }
    }

    public void ShowOtherAreasAd()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            if (!AreAdsRemoved())
            {
                if (DisplayFirstPriortyAd(OtherAreas))
                {
                    Debug.Log("TT AD Stuff : 1st Priority Ad on Element was sucessfull");
                }
                else if (DisplaySecondPriortyAd(OtherAreas))
                {
                    Debug.Log("TT AD Stuff : 2nd Priority Ad on Element was sucessfull");
                }
            }
        }
    }

    private bool DisplayFirstPriortyAd(AdsDisplayProperties temp)
    {
        if (temp.FirstPriority == AdsDisplayProperties.AdPriority.Admob)
        {
            if (ShowAdmobInterstitialAd())
            {
                return true;
            }
        }
        else if (temp.FirstPriority == AdsDisplayProperties.AdPriority.Unity)
        {
            if (ShowUnityAds())
            {
                return true;
            }
        }
       
        return false;
    }

    private bool DisplaySecondPriortyAd(AdsDisplayProperties temp)
    {
        if (temp.SecondPriority == AdsDisplayProperties.AdPriority.Admob)
        {
            if (ShowAdmobInterstitialAd())
            {
                return true;
            }
        }
        else if (temp.SecondPriority == AdsDisplayProperties.AdPriority.Unity)
        {
            if (ShowUnityAds())
            {
                return true;
            }
        }
        
        return false;
    }

    #endregion

    #region RewardedAd_Zone

    public void ShowRewardedAd()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            if (DisplayFirstRewardedAd(rewardedAds))
            {
                Debug.Log("TT AD Stuff : 1st Rewarded Priority Ad on Element was sucessfull");
            }
            else if (DisplaySecondRewardedAd(rewardedAds))
            {
                Debug.Log("TT AD Stuff : 2nd Rewarded Priority Ad on Element was sucessfull");
            }
            else
            {
                try
                {
                    MNPopup popup = new MNPopup("Ad not available", "Sorry rewarded ad not available, please try again later.");
                    popup.AddAction("Ok", () =>
                    {
                        Debug.Log("TT Ads SDK Logs: Rewarded ad not available");
                    });
                    popup.AddDismissListener(() =>
                    {
                        Debug.Log("dismiss listener");
                    });
                    popup.Show();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }

    private bool DisplayFirstRewardedAd(RewardedAds temp)
    {
        if (temp.FirstPriority == RewardedAds.AdPriority.Unity)
        {
            if (ShowUnityRewardedAd())
            {
                return true;
            }
        }
        else if (temp.FirstPriority == RewardedAds.AdPriority.Admob)
        {
            if (ShowAdmobRewardedAd())
            {
                return true;
            }
        }
        return false;
    }

    private bool DisplaySecondRewardedAd(RewardedAds temp)
    {
        if (temp.SecondPriority == RewardedAds.AdPriority.Unity)
        {
            if (ShowUnityRewardedAd())
            {
                return true;
            }
        }
        else if (temp.SecondPriority == RewardedAds.AdPriority.Admob)
        {
            if (ShowAdmobRewardedAd())
            {
                return true;
            }
        }
        return false;
    }

    #endregion

    #endregion

    #region ADMOB_REGION

    private GoogleMobileAds.Api.InterstitialAd Interstitial;
    private GoogleMobileAds.Api.BannerView bannerView, DBannerView;
    private GoogleMobileAds.Api.AdRequest GBannerRequest, InterstitialRequest, RewarededRequest;
    private GoogleMobileAds.Api.AdRequest RequestRewarded;
    private GoogleMobileAds.Api.RewardBasedVideoAd RewardBasedVideo;
    private bool AdmobBannerHasLoaded;

    #region ADMOB_BANNER_AD

    private void RequestAdmobBanner()
    {
        try
        {
            bannerView = new GoogleMobileAds.Api.BannerView(AdmobBannerId, GoogleMobileAds.Api.AdSize.Banner, GeneralAdmobBannerAdPostion);
            bannerView.OnAdLoaded += HandleOnAdLoaded;
            bannerView.OnAdFailedToLoad += HandleOnAdFailedToLoad;
            GBannerRequest = new GoogleMobileAds.Api.AdRequest.Builder().Build();
            bannerView.LoadAd(GBannerRequest);
        }
        catch (Exception)
        {

            throw;
        }
    }

    private void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs e)
    {
        AdmobBannerHasLoaded = false;
    }

    private void HandleOnAdLoaded(object sender, EventArgs e)
    {
        AdmobBannerHasLoaded = true;
    }

    public void ShowAdmobBannerAd()
    {
        try
        {
            if (!AreAdsRemoved())
            {
                if (AdmobBannerHasLoaded)
                {
                    bannerView.Show();
                }
                RequestAdmobBanner();
            }
        }
        catch (Exception)
        {

            throw;
        }
    }

    
    public void DestoryAdmobBannerAd()
    {
        bannerView.Hide();
        bannerView.Destroy();
    }
    #endregion

    #region ADMOB_INTERSTITIAL_AD

    private void RequestAdmobInterstitialAd()
    {
        try
        {
            Interstitial = new GoogleMobileAds.Api.InterstitialAd(AdmobInterstitialId);
            InterstitialRequest = new GoogleMobileAds.Api.AdRequest.Builder().Build();
            Interstitial.LoadAd(InterstitialRequest);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private bool ShowAdmobInterstitialAd()
    {
        try
        {
            if (Interstitial.IsLoaded())
            {
                Interstitial.Show();
                RequestAdmobInterstitialAd();
                return true;
            }
            RequestAdmobInterstitialAd();
        }
        catch (Exception)
        {

            throw;
        }
        return false;
    }


    #endregion

    #region ADMOB_REWARDED_AD

    private void RequestAdmobRewardedAd()
    {
        this.RewardBasedVideo = GoogleMobileAds.Api.RewardBasedVideoAd.Instance;
        RewarededRequest = new GoogleMobileAds.Api.AdRequest.Builder().Build();
        this.RewardBasedVideo.LoadAd(RewarededRequest, AdmobRewardedId);
        RewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
    }

    private void HandleRewardBasedVideoRewarded(object sender, Reward e)
    {
        OnRewardedAdShown();
    }

    public bool ShowAdmobRewardedAd()
    {
        try
        {
            if (RewardBasedVideo.IsLoaded())
            {
                RewardBasedVideo.Show();
                RequestAdmobRewardedAd();
                return true;
            }
            RequestAdmobRewardedAd();
        }
        catch (Exception)
        {

            throw;
        }
        return false;
    }

    #endregion

    #endregion

    #region UNITY_REGION

    private string UnitySimpleVideoAd = "video";
    private string UnityRewardedVideoAd = "rewardedVideo";
    private bool isLoadedUnitySimpleVideoAd, isLoadedUnityRewardedVideoAd;

    private bool ShowUnityAds()
    {
        try
        {
            if (isLoadedUnitySimpleVideoAd)
            {
                Advertisement.Show(UnitySimpleVideoAd);
                isLoadedUnitySimpleVideoAd = false;
                return true;
            }
        }
        catch (Exception)
        {

            throw;
        }
        return false;
    }

    private bool ShowUnityRewardedAd()
    {
        try
        {
            if (isLoadedUnityRewardedVideoAd)
            {
                Advertisement.Show(UnityRewardedVideoAd);
                isLoadedUnityRewardedVideoAd = false;
                return true;
            }
        }
        catch (Exception)
        {
            throw;
        }
        return false;
    }

    public void OnUnityAdsReady(string placementId)
    {
        Debug.Log($"TT Ads Manager : {placementId} Unity Ad is ready to display");

        #region REWARDED_AD_STATUS

        if (placementId.Equals(UnityRewardedVideoAd))
        {
            isLoadedUnityRewardedVideoAd = true;
        }

        #endregion

        #region SIMPLE_AD_STATUS

        if (placementId.Equals(UnitySimpleVideoAd))
        {
            isLoadedUnitySimpleVideoAd = true;
        }

        #endregion
    }

    public void OnUnityAdsDidError(string message)
    {
        try
        {
            Debug.LogError($"TT Ads Manager : Unity Ad has produced an error = {message}");
        }
        catch (Exception)
        {
            throw;
        }
    }

    public void OnUnityAdsDidStart(string placementId)
    {
        Debug.Log($"TT Ads Manager : {placementId} Unity Ad has started");
    }

    public void OnUnityAdsDidFinish(string placementId, UnityEngine.Advertisements.ShowResult showResult)
    {
        if (showResult == UnityEngine.Advertisements.ShowResult.Finished && placementId.Equals(UnityRewardedVideoAd))
        {
            Debug.Log($"TT Ads Manager : Unity Ad has finished displaying.");
            OnRewardedAdShown();
        }
        else if (showResult == UnityEngine.Advertisements.ShowResult.Skipped)
        {
            Debug.LogWarning($"TT Ads Manager : Unity Ad was skipped.");
        }
        else if (showResult == UnityEngine.Advertisements.ShowResult.Failed)
        {
            Debug.LogError($"TT Ads Manager : Unity Ad Failed Due to some error.");
        }
    }

    #endregion

    #region UNITY_ANALYTICS_REGION

    public void UnityLogs(string logName)
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            Analytics.CustomEvent(logName);
        }
    }

    public void UnityLogs(string logName, string logDiscription, int number)
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            Analytics.CustomEvent(logName, new Dictionary<string, object> {
                { logDiscription,number }
            });
        }
    }

    #endregion

    #region NATIVE POPUPS SECTION

    public void ShowRateDialoug()
    {
        try
        {
            MNPopup popup = new MNPopup("Rate Us", "Did you like our game? Then rate our game.");
            popup.AddAction("Sure", () =>
            {
#if UNITY_ANDROID
                Application.OpenURL("https://play.google.com/store/apps/details?id=" + Application.identifier);
#elif UNITY_IPHONE
                Application.OpenURL("itms-apps://itunes.apple.com/app/id"+AppleStoreAppId);
#endif
            });
            popup.AddAction("Not interested", () =>
            {

            });
            popup.AddDismissListener(() => { Debug.Log("dismiss listener"); });
            popup.Show();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public void ShowConsetDialoug()
    {
        try
        {
            if (PlayerPrefs.GetInt("consetGDPR", 0) == 0)
            {
                MNPopup popup = new MNPopup("Terms and Conditions", ProductName + " is a free to play game. This product is subject to our terms of services and privacy policy. Press ok to accept the terms and conditions and priavcy policy");
                popup.AddAction("Ok", () =>
                {
                    PlayerPrefs.SetInt("consetGDPR", 1);
                });
                popup.AddAction("Terms and Conditions", () =>
                {
                    Application.OpenURL(PrivacyPolicy());
                });
                popup.Show();
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public string PrivacyPolicy()
    {
        return PrivacyPolicyLink;
    }

    public void ShowExitMenu()
    {
        try
        {
            if (!DisplayExitDialoug)
            {
                DisplayExitDialoug = !DisplayExitDialoug;
                ShowOtherAreasAd();
                MNPopup popup = new MNPopup("Exit Game", "Do you really want to exit this game?");
                popup.AddAction("Yes", () =>
                {
                    Application.Quit();
                });
                popup.AddAction("No", () =>
                {
                    DisplayExitDialoug = !DisplayExitDialoug;
                });
                popup.Show();
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public void InAppTimedOut()
    {
        try
        {
            MNPopup popup = new MNPopup("Request Timed Out", "Failed to contact server please try again later");
            popup.AddAction("Ok", () =>
            {

            });
            popup.Show();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public void NoInternetConnection()
    {
        try
        {
            MNPopup popup = new MNPopup("Request Timed Out", "No internet connection detected. Please connect to an internet and try again.");
            popup.AddAction("Ok", () =>
            {

            });
            popup.Show();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public void InAppFailed()
    {
        try
        {
            MNPopup popup = new MNPopup("Request Failed", "Could not buy this product please try again later");
            popup.AddAction("Ok", () =>
            {

            });
            popup.Show();
        }
        catch (Exception)
        {
            throw;
        }
    }

    void OnApplicationQuit()
    {

    }


    public void OpenMoreApps()
    {
        Application.OpenURL(MoreAppsLink);
    }
    #endregion

    #region CUSTOM CLASSES SECTION

    [System.Serializable]
    public class AdsDisplayProperties
    {
        public enum AdPriority
        {
            Admob,
            Unity
        };
        public AdPriority FirstPriority, SecondPriority;
    }

    [System.Serializable]
    public class RewardedAds
    {
        public enum AdPriority
        {
            Unity,
            Admob        };

        public AdPriority FirstPriority, SecondPriority;
    }

    [System.Serializable]
    public class AdIds
    {
        [Header("Admob Ads Ids")]
        [Tooltip("Enter your AdMob Banner Id Here")]
        public string AdmobBannerId;
        [Tooltip("Enter your AdMob Interstital Id Here")]
        public string AdmobInterstitalId;
        [Tooltip("Enter your AdMob Interstital Id Here")]
        public string AdmobRewardedAdId;
        [Header("Unity Ads Ids")]
        [Tooltip("Enter your Unity Ads Id Here")]
        public string UnityAdId;
    }
    #endregion
}

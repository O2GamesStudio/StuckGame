using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class GoogleAdmobManager : MonoBehaviour
{
    public static GoogleAdmobManager Instance { get; private set; }

    private string androidBannerAdUnitId = "ca-app-pub-3490273194196393/8505313920"; // Real ID
    private string androidRewardedAdUnitId = "ca-app-pub-3490273194196393/2518490075"; // Real ID

    // Test Ad Unit IDs (Google 제공 테스트 ID)
    private const string TEST_ANDROID_BANNER = "ca-app-pub-3940256099942544/6300978111";
    private const string TEST_ANDROID_REWARDED = "ca-app-pub-3940256099942544/5224354917";

    private BannerView bannerView;
    private RewardedAd rewardedAd;

    private Action onRewardedAdCompleted;
    private Action onRewardedAdFailed;

    private bool isRewardedAdLoading = false;
    private bool isRewardedAdReady = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Initialize the Google Mobile Ads SDK
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("AdMob initialized");

            // 배너 광고는 GameManager에서 명시적으로 호출할 때만 로딩
            // LoadBannerAd(); ← 제거!

            // Pre-load rewarded ad
            LoadRewardedAd();
        });
    }

    #region Banner Ad

    /// <summary>
    /// Creates and loads a banner ad
    /// </summary>
    public void LoadBannerAd()
    {
        // Clean up banner before reusing
        if (bannerView != null)
        {
            DestroyBannerAd();
        }

        // Create a 320x50 banner at bottom of the screen
        bannerView = new BannerView(GetBannerAdUnitId(), AdSize.Banner, AdPosition.Bottom);

        // Register for ad events
        bannerView.OnBannerAdLoaded += OnBannerAdLoaded;
        bannerView.OnBannerAdLoadFailed += OnBannerAdLoadFailed;

        // Load the banner ad
        AdRequest adRequest = new AdRequest();
        bannerView.LoadAd(adRequest);

        Debug.Log("Loading Banner Ad...");
    }

    /// <summary>
    /// Shows the banner ad
    /// </summary>
    public void ShowBannerAd()
    {
        if (bannerView != null)
        {
            bannerView.Show();
            Debug.Log("Banner Ad Shown");
        }
    }

    /// <summary>
    /// Hides the banner ad
    /// </summary>
    public void HideBannerAd()
    {
        if (bannerView != null)
        {
            bannerView.Hide();
            Debug.Log("Banner Ad Hidden");
        }
    }

    /// <summary>
    /// Destroys the banner ad
    /// </summary>
    public void DestroyBannerAd()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
            Debug.Log("Banner Ad Destroyed");
        }
    }

    private void OnBannerAdLoaded()
    {
        Debug.Log("Banner Ad Loaded");
        // 로딩 완료 후 자동으로 표시
        if (bannerView != null)
        {
            bannerView.Show();
            Debug.Log("Banner Ad Auto-Shown after loading");
        }
    }

    private void OnBannerAdLoadFailed(LoadAdError error)
    {
        Debug.LogError($"Banner Ad Failed to Load: {error.GetMessage()}");
    }

    #endregion

    #region Rewarded Ad

    /// <summary>
    /// Loads a rewarded ad
    /// </summary>
    public void LoadRewardedAd()
    {
        // Clean up old ad before loading a new one
        if (rewardedAd != null)
        {
            DestroyRewardedAd();
        }

        if (isRewardedAdLoading)
        {
            Debug.Log("Rewarded Ad is already loading...");
            return;
        }

        isRewardedAdLoading = true;
        isRewardedAdReady = false;

        AdRequest adRequest = new AdRequest();

        RewardedAd.Load(GetRewardedAdUnitId(), adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            isRewardedAdLoading = false;

            if (error != null || ad == null)
            {
                Debug.LogError($"Rewarded Ad Failed to Load: {error?.GetMessage() ?? "Unknown error"}");
                isRewardedAdReady = false;
                return;
            }

            Debug.Log("Rewarded Ad Loaded");
            rewardedAd = ad;
            isRewardedAdReady = true;

            // Register for ad events
            RegisterRewardedAdEvents();
        });
    }

    /// <summary>
    /// Shows the rewarded ad
    /// </summary>
    /// <param name="onCompleted">Callback when user earns reward</param>
    /// <param name="onFailed">Callback when ad fails to show or user closes early</param>
    public void ShowRewardedAd(Action onCompleted, Action onFailed = null)
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            onRewardedAdCompleted = onCompleted;
            onRewardedAdFailed = onFailed;

            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log($"Rewarded Ad - User earned reward: {reward.Amount} {reward.Type}");
                OnUserEarnedReward();
            });
        }
        else
        {
            Debug.LogWarning("Rewarded Ad is not ready yet");
            onFailed?.Invoke();

            // Try to load a new ad for next time
            LoadRewardedAd();
        }
    }

    /// <summary>
    /// Check if rewarded ad is ready to show
    /// </summary>
    public bool IsRewardedAdReady()
    {
        return isRewardedAdReady && rewardedAd != null && rewardedAd.CanShowAd();
    }

    private void RegisterRewardedAdEvents()
    {
        if (rewardedAd == null) return;

        rewardedAd.OnAdFullScreenContentClosed += OnRewardedAdClosed;
        rewardedAd.OnAdFullScreenContentFailed += OnRewardedAdFailedToShow;
    }

    private void OnUserEarnedReward()
    {
        Debug.Log("User earned reward!");
        onRewardedAdCompleted?.Invoke();
        onRewardedAdCompleted = null;
        onRewardedAdFailed = null;

        // Load next rewarded ad
        LoadRewardedAd();
    }

    private void OnRewardedAdClosed()
    {
        Debug.Log("Rewarded Ad Closed");

        // If user closed ad without earning reward
        if (onRewardedAdCompleted != null)
        {
            onRewardedAdFailed?.Invoke();
            onRewardedAdCompleted = null;
            onRewardedAdFailed = null;
        }

        // Load next rewarded ad
        LoadRewardedAd();
    }

    private void OnRewardedAdFailedToShow(AdError error)
    {
        Debug.LogError($"Rewarded Ad Failed to Show: {error.GetMessage()}");
        onRewardedAdFailed?.Invoke();
        onRewardedAdCompleted = null;
        onRewardedAdFailed = null;

        // Load next rewarded ad
        LoadRewardedAd();
    }

    private void DestroyRewardedAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
            isRewardedAdReady = false;
        }
    }

    #endregion

    #region Helper Methods

    private string GetBannerAdUnitId()
    {
#if UNITY_ANDROID
        return androidBannerAdUnitId;
#else
        return TEST_ANDROID_BANNER;
#endif
    }

    private string GetRewardedAdUnitId()
    {
#if UNITY_ANDROID
        return androidRewardedAdUnitId;
#else
        return TEST_ANDROID_REWARDED;
#endif
    }

    #endregion

    void OnDestroy()
    {
        // Clean up ads
        DestroyBannerAd();
        DestroyRewardedAd();
    }
}
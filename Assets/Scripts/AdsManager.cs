using UnityEngine;
using UnityEngine.Advertisements;

public class AdsManager : MonoBehaviour, IUnityAdsListener {
#if UNITY_IOS
    string gameId = "3614624"; //Apple
#elif UNITY_ANDROID
    string gameId = "3614625"; //Android
#else
    string gameId = "3614625"; 
#endif

    bool testMode = true;
    bool adsReady = false;

    private const string rewardedId = "rewardedVideo";
    private const string rewardedInterstitialId = "rewardedInterstitial";
    private const string interstitialId = "video";
    private const string bannerAd = "banner";

    private GameManager game;
    public delegate void AdCompleteHandler(AdType adType);
    private AdCompleteHandler adCompleteHandler;

    public bool isPlayingAd { get; private set; }

    public enum AdType {
        Interstitial,
        Rewarded,
        RewardedInterstitial
    }

    void Start() {
        game = GetComponent<GameManager>();
        adsReady = (Advertisement.IsReady(rewardedId) && Advertisement.IsReady(rewardedInterstitialId) && Advertisement.IsReady(interstitialId));
        Advertisement.AddListener(this);
        Advertisement.Initialize(gameId, testMode);
        Advertisement.Banner.SetPosition(BannerPosition.CENTER);
    }

    public void SetAdCompleteHandler(AdCompleteHandler handler) {
        adCompleteHandler = handler;
    }

    public void ShowAd(AdType adType) {
        isPlayingAd = true;
        switch (adType) {
            case AdType.Interstitial:
                Advertisement.Show(interstitialId);
                break;
            case AdType.Rewarded:
                Advertisement.Show(rewardedId);
                break;
            case AdType.RewardedInterstitial:
                Advertisement.Show(rewardedInterstitialId);
                break;
        }
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult) {
        // Define conditional logic for each ad completion status:
        isPlayingAd = false;
        if (showResult == ShowResult.Finished) {
            /*
            switch (PlacementIdToAdType(placementId)) {
                case AdType.Interstitial:
                    break;
                case AdType.Rewarded:
                    game.AddCoins(50);
                    break;
                case AdType.RewardedInterstitial:
                    game.AddCoins(50);
                    break;
            }
            */
            //game.OnAdCompleted(PlacementIdToAdType(placementId));
            adCompleteHandler(PlacementIdToAdType(placementId));
            // Reward the user for watching the ad to completion.
        } else if (showResult == ShowResult.Skipped) {
            print("did skip ad");
            adCompleteHandler(PlacementIdToAdType(placementId));
            // Do not reward the user for skipping the ad.
        } else if (showResult == ShowResult.Failed) {
            Debug.LogWarning("The ad did not finish due to an error.");
        }
    }

    private AdType PlacementIdToAdType(string placementId) {
        switch (placementId) {
            case interstitialId:
                return AdType.Interstitial;
            case rewardedId:
                return AdType.Rewarded;
            case rewardedInterstitialId:
                return AdType.RewardedInterstitial;
            default:
                return AdType.Interstitial; //TODO this shoudlnt be
        }
    }

    public void OnUnityAdsReady(string placementId) {
        //every time an add is ready, check if they are all ready
        adsReady = (Advertisement.IsReady(rewardedId) && Advertisement.IsReady(rewardedInterstitialId) && Advertisement.IsReady(interstitialId));
    }

    public void OnUnityAdsDidError(string message) {
        // Log the error.
        print("OnUnityAdsDidError");
    }

    public void OnUnityAdsDidStart(string placementId) {
        // Optional actions to take when the end-users triggers an ad.
    }

    public void ShowBannerAd() {
        Advertisement.Banner.Show(bannerAd);
    }

    public void HideBannerAd() {
        Advertisement.Banner.Hide();
    }
}
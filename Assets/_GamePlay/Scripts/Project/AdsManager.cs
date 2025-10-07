using System;
using System.Collections;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
    [SerializeField] private bool testMode = true;
    [SerializeField] private bool showAds = true;
    [Space]
    [SerializeField] private int MaxAutomaticallyAttempsToReloadAd = 5;
    [SerializeField] private int maxTimeForAdLoadingInSeconds = 15;
    [Space]
    [SerializeField] private string adLoadingMessageLocalizationKey = "loading_ad";
    [SerializeField] private string adLoadingErrorMessageLocalizationKey = "ad_loading_error_message";

    private Func<string, string> _getLocalizedValueFunc;

    private const string _gameID = "";
    private const string _rewardedAdPlacementID = "Rewarded_Android";
    private const string _interstitialAdPlacementID = "Interstitial_Android";

    private Ad _rewardedAd;
    private Ad _interstitialAd;

    private ConfirmationPanel _overlayPanel;

    private string _adLoadingMessage;
    private string _adLoadingErrorMessage;

    private Timer _adLoadingTimer;

    [Zenject.Inject]
    private void Init(Func<string, string> getLocalizedValueFunc){
        _rewardedAd = new Ad(_rewardedAdPlacementID, false);
        _interstitialAd = new Ad(_interstitialAdPlacementID, true);
        _getLocalizedValueFunc = getLocalizedValueFunc;
    }

    private void Start() {
        _adLoadingMessage = _getLocalizedValueFunc.Invoke(adLoadingMessageLocalizationKey);
        _adLoadingErrorMessage = _getLocalizedValueFunc.Invoke(adLoadingErrorMessageLocalizationKey);

        StartCoroutine(InitAds());
    }

    private IEnumerator InitAds(){
        if(string.IsNullOrEmpty(_gameID))
            yield break;

        LoadAds();
    }

    public static bool AdLevel(int levelNumber) => levelNumber > 8 && levelNumber % 4 == 0;

    public void LoadAds(){
        LoadAd(_rewardedAd);
        LoadAd(_interstitialAd);
    }

    private void LoadAd(Ad ad){
        if(ad.isAdLoading)
            return;

        ad.isAdLoading = true;
        ad.AttempsToReloadAd++;
    }

    private void ShowAd(Ad ad, Action onComplete, Action onFailure){
        if(showAds == false)
            return;

        if(ad.isAdReady == false){
            if(ad.SkipAdIfItIsNotReady)
                return;

            if(ad.isAdLoading){
                _overlayPanel = OverlayPanels.CreateNewInformationPanel(_adLoadingMessage, null, false);
                _adLoadingTimer = Timer.StartNew(this, maxTimeForAdLoadingInSeconds, () => {
                    OverlayPanels.CreateNewInformationPanel(_adLoadingErrorMessage, null, true);
                    _overlayPanel.Cancel();
                    ad.OnAdLoaded = null;
                });

                ad.OnAdLoaded += () => ShowAd(ad, onComplete, onFailure);
            }

            return;
        }

        if(_adLoadingTimer != null)
            _adLoadingTimer.Dispose();

        ad.SetCallbacks(onComplete, onFailure, null);
        ad.isAdReady = false;
    }

    public static async Task<bool> IsInternetReachable(){
        using (var client = new HttpClient()){
            var result = await client.GetAsync("http://google.com");
            return result.IsSuccessStatusCode;
        }
    }

    public void ShowInterstitialAd(Action onComplete, Action onFailure) => ShowAd(_interstitialAd, onComplete, onFailure);
    public void ShowRewardedAd(Action onComplete, Action onFailure) => ShowAd(_rewardedAd, onComplete, onFailure);
    
    public void OnUnityAdsAdLoaded(string placementId){
        GetAdByPlacementID(placementId, out Ad ad);
        ad.AdLoaded();
    }

    private void GetAdByPlacementID(string placementID, out Ad ad){
        if(string.Equals(_rewardedAdPlacementID, placementID)) ad = _rewardedAd;
        else if(string.Equals(_interstitialAdPlacementID, placementID)) ad = _interstitialAd;
        else ad = new Ad(string.Empty, true);
    }

    public void OnUnityAdsShowStart(string placementId) => print("OnUnityAdsShowStart");
    public void OnUnityAdsShowClick(string placementId) => print("OnUnityAdsShowClick");

    [Serializable]
    public class Ad
    {
        public Ad(string placementID, bool skipAdIfItIsNotReady){
            PlacementID = placementID;
            SkipAdIfItIsNotReady = skipAdIfItIsNotReady;
        }

        public string PlacementID;
        public bool SkipAdIfItIsNotReady;

        public Action OnAdComplete;
        public Action OnAdFailure;

        public Action OnAdLoaded;

        public bool isAdReady;
        public bool isAdLoading;

        public int AttempsToReloadAd;

        public void AdShowed() {
            OnAdComplete?.Invoke();
            Reset();
        }

        public void SetCallbacks(Action onComplete, Action onFailure, Action onLoaded){
            OnAdComplete = onComplete ?? OnAdComplete;
            OnAdFailure = onFailure ?? OnAdFailure;
            OnAdLoaded = onLoaded ?? OnAdLoaded;
        }

        public void AdLoaded(){
            isAdReady = true;
            isAdLoading = false;
            AttempsToReloadAd = 0;

            OnAdLoaded?.Invoke();
            OnAdLoaded = null;
        }

        public void AdFailedToLoad(){
            OnAdFailure?.Invoke();
            Reset();
        }

        public void Reset(){
            OnAdComplete = OnAdFailure = null;
            isAdReady = isAdLoading = false;
            AttempsToReloadAd = 0;
        }
    }
}

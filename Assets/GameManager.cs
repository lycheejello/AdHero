using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour {

    private bool welcomeComplete = false;

    private AdsManager adsManager;
    [SerializeField] private GameObject welcomeSequence;
    [SerializeField] private GameObject mainGame;

    private string username;
    [SerializeField] TMP_InputField loginField;

    //[SerializeField] private Text goldText;
    [SerializeField] private Button button1;
    [SerializeField] private Button button2;

    //0. Title
    //1. welcome screen (message)
    //a. ad
    //2. give coins (message)
    //3. create username
    //4. create password
    //5. congratulations (message)
    //8. ad
    //9. get into main game
    private int welcomeIndex = 0;
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private GameObject loginPanel;

    private int coins = 0;
    [SerializeField] private GameObject coinsText;

    void Start() {
        adsManager = GetComponent<AdsManager>();
        adsManager.SetAdCompleteHandler(OnAdCompleted);
        coinsText.GetComponent<TMP_Text>().SetText(coins.ToString());

        if (welcomeComplete) {
            StartMainGame();
        } else {
            welcomeSequence.SetActive(true);
            mainGame.SetActive(false);
        }
    }

    void OnGUI() {
        HandleText();
    }

    private void StartMainGame() {
        welcomeComplete = true;
        mainGame.SetActive(true);
        welcomeSequence.SetActive(false);
        mainGame.GetComponent<MainGame>().InitMainGame(username, coins);
    }

    // Update is called once per frame
    void Update() {
        if (adsManager.isPlayingAd) {
            return;
        }

        if (Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) {
                HandleInput();
            }
        } else if (Input.GetMouseButtonDown(0)) {
            HandleInput();
        }
    }

    private void HandleText() {
        switch (welcomeIndex) {
            case 0:
                ShowTitle();
                coinsText.SetActive(false);
                break;
            case 1:
                ShowMessage("We see you are a 1st time user. Please enjoy this ad to activate ADHERO PREMIUM");
                coinsText.SetActive(false);
                break;
            case 2:
                ShowMessage("Here are some coins to start you off");
                coinsText.SetActive(true);
                break;
            case 3:
                ShowLogin("Username cost based on whatever");
                if (loginField.interactable) {
                    button1.GetComponentInChildren<TMP_Text>().SetText("Confirm username\n({0}g)", GetUserNameCost(loginField.text));
                }
                break;
            case 4:
                ShowLogin("Password cost based on whoever");
                if (loginField.interactable) {
                    button1.GetComponentInChildren<TMP_Text>().SetText("Confirm password\n({0}g)", GetUserNameCost(loginField.text));
                }
                break;
            case 5:
                ShowMessage(string.Format("Go forth, {0} and fullfill your destiny... after these messages.", username));
                break;
            case 6:
                ShowMessage(string.Format("Your AdHero Premium trial has ended", username));
                break;
            default:
                break;
        }

    }

    private void HandleInput() {
        if (welcomeComplete) {
            return;
        }
        switch (welcomeIndex) {
            case 0:
                ShowTitle();
                OnProceed();
                break;
            case 1:
                OnProceedWithAd(AdsManager.AdType.RewardedInterstitial);
                break;
            case 2:
                OnProceed();
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                OnProceedWithAd(AdsManager.AdType.Interstitial);
                break;
            case 6:
                Invoke("MuteAll", 1.0f);
                Invoke("StartMainGame", 5f);
                break;

        }
    }

    //notice case + 1 is the end of welcomeIndex;
    public void OnAdCompleted(AdsManager.AdType adType) {
        switch (adType) {
            case AdsManager.AdType.Interstitial:
                break;
            case AdsManager.AdType.Rewarded:
                AddCoins(20);
                break;
            case AdsManager.AdType.RewardedInterstitial:
                AddCoins(50);
                break;
        }
    }

    private void ShowTitle() {
        titlePanel.SetActive(true);
        messagePanel.SetActive(false);
        loginPanel.SetActive(false);
    }

    private void ShowMessage(string msg) {
        titlePanel.SetActive(false);
        messagePanel.SetActive(true);
        loginPanel.SetActive(false);

        messagePanel.GetComponentInChildren<TMP_Text>().SetText(msg);
    }

    private void ShowLogin(string msg) {
        titlePanel.SetActive(false);
        messagePanel.SetActive(false);
        loginPanel.SetActive(true);

        loginPanel.GetComponentInChildren<TMP_Text>().SetText(msg);
    }

    public void OnProceedWithAd(AdsManager.AdType adType) {
        adsManager.ShowAd(adType);
        OnProceed();
    }


    public void OnProceed() {
        welcomeIndex += 1;
    }

    public void OnButton1() {
        if (!loginField.interactable) {
            AddCoins(-5);
            UnlockUsername();
        } else if (welcomeIndex == 3) { //username
            AddCoins(-1 * GetUserNameCost(loginField.text));
            username = loginField.text;
            loginField.text = "";
            OnProceed();
        } else { //must be password
            AddCoins(-1 * GetPasswordCost(loginField.text));
            OnProceed();
        }
    }

    public void OnButton2() {
        adsManager.ShowAd(AdsManager.AdType.Rewarded);
    }
    private int GetUserNameCost(string name) {
        return name.Length * 10;
    }

    private int GetPasswordCost(string password) {
        return password.Length * 10;
    }


    public void AddCoins(int c) {
        int startCoins = coins;
        coins += c;
        StartCoroutine(DisplayCoins(startCoins));
    }

    public IEnumerator DisplayCoins(int startCoins) {
        int jump = (coins - startCoins) / 10;

        float totalTime = 0;
        float duration = 1f;
        int showCoins = startCoins;
        while (showCoins != coins) {
            showCoins = (int)Mathf.Lerp(startCoins, coins, totalTime / duration);
            coinsText.GetComponent<TMP_Text>().SetText(showCoins.ToString());
            totalTime += Time.deltaTime;
            yield return null;
        }
    }

    public void UnlockUsername() {
        loginField.interactable = true;
    }

    private void MuteAll() {
        GetComponent<AudioSource>().mute = true;
        Invoke("MuteBG", 2f);
    }

    private void MuteBG() {
        var bwBG = Resources.Load<Sprite>("Morning-BW");
        messagePanel.GetComponentInParent<Image>().sprite = bwBG;
    }
}

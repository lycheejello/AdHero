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
    [SerializeField] private TMP_Text tapToContinue;

    [SerializeField] private float messageSpeed = 0.05f;
    private IEnumerator messageCoroutine;

    [SerializeField] private GameObject loginPanel;

    private int coins = 0;
    [SerializeField] private GameObject coinsText;


    [SerializeField] private Image transitionImage;
    [SerializeField] private float transitionLength = 0.4f;

    void Start() {
        adsManager = GetComponent<AdsManager>();
        adsManager.SetAdCompleteHandler(OnAdCompleted);
        coinsText.GetComponent<TMP_Text>().SetText(coins.ToString());

        if (welcomeComplete) {
            StartMainGame();
        } else {
            welcomeSequence.SetActive(true);
            mainGame.SetActive(false);
            ShowTitle();
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
                coinsText.SetActive(false);
                break;
            case 1:
                coinsText.SetActive(false);
                break;
            case 2:
                coinsText.SetActive(true);
                break;
            case 3:
                StartCoroutine(ShowLogin("Username cost based on whatever"));
                if (loginField.interactable) {
                    button1.GetComponentInChildren<TMP_Text>().SetText("Confirm username\n({0}g)", GetUserNameCost(loginField.text));
                }
                break;
            case 4:
                StartCoroutine(ShowLogin("Password cost based on whoever"));
                if (loginField.interactable) {
                    button1.GetComponentInChildren<TMP_Text>().SetText("Confirm password\n({0}g)", GetUserNameCost(loginField.text));
                }
                break;
            default:
                break;
        }

    }

    private void HandleInput() {
        if (!tapToContinue.enabled || welcomeComplete) {
            return;
        }
        switch (welcomeIndex) {
            case 0:
                OnProceed();
                StartCoroutine(ShowMessage("Welcome, Hero! Please enjoy this ad to for a free ADHERO PREMIUM trial.", transitionLength));
                break;
            case 1:
                OnProceedWithAd(AdsManager.AdType.RewardedInterstitial);
                break;
            case 2:
                OnProceed();
                break;
            case 5:
                OnProceedWithAd(AdsManager.AdType.Interstitial);
                break;
            case 6:
                StartCoroutine(MuteAll());
                welcomeIndex += 1;
                break;
            case 7:
                StartMainGame();
                break;
        }
    }

    public void OnAdCompleted(AdsManager.AdType adType) {
        switch (adType) {
            case AdsManager.AdType.Interstitial:
                break;
            case AdsManager.AdType.Rewarded:
                AddCoins(20);
                break;
            case AdsManager.AdType.RewardedInterstitial:
                //AddCoins(50);
                break;
        }

        switch (welcomeIndex) {
            case 2:
                StartCoroutine(ShowMessage("Here are some coins to start you off.", transitionLength));
                AddCoins(50); break;
            case 6:
                StartCoroutine(ShowMessage(string.Format("Your AdHero Premium trial has ended.", username), transitionLength));
                break;
        }
    }

    private void ShowTitle() {
        titlePanel.SetActive(true);
        messagePanel.SetActive(false);
        loginPanel.SetActive(false);
    }

    private IEnumerator ShowMessage(string msg, float waitTime) {
        TMP_Text messageText = messagePanel.GetComponentInChildren<TMP_Text>();
        messageText.SetText("");
        if (messageCoroutine != null) {
            StopCoroutine(messageCoroutine);
        }
        tapToContinue.enabled = false;
        yield return new WaitForSeconds(waitTime / 2);
        titlePanel.SetActive(false);
        messagePanel.SetActive(true);
        loginPanel.SetActive(false);

        yield return new WaitForSeconds(waitTime / 2);
        messageCoroutine = TypeDialogue(msg, messageText);
        StartCoroutine(messageCoroutine);
        Invoke("ShowTapToContinue", 4f);
    }

    private IEnumerator TypeDialogue(string msg, TMP_Text text) {
        text.SetText("");
        foreach (char l in msg.ToCharArray()) {
            text.SetText(text.text += l);
            if (".!?".Contains(l.ToString())) {
                yield return new WaitForSeconds(messageSpeed * 10);
            } else {
                yield return new WaitForSeconds(messageSpeed);
            }
        }
        yield return null;
    }

    private void ShowTapToContinue() {
        tapToContinue.enabled = true;
    }

    private IEnumerator ShowLogin(string msg) {
        yield return new WaitForSeconds(transitionLength / 2);
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
        StartCoroutine(FadeTransition());
        welcomeIndex += 1;
    }

    private IEnumerator FadeTransition() {
        float tTime = transitionLength / 2f;
        transitionImage.color = new Color32(0, 0, 0, 255);
        transitionImage.canvasRenderer.SetAlpha(0f);
        transitionImage.CrossFadeAlpha(1f, tTime, false);
        yield return new WaitForSeconds(tTime);

        transitionImage.CrossFadeAlpha(0, tTime, false);
        yield return new WaitForSeconds(tTime);
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
            StartCoroutine(ShowMessage(string.Format("Go forth, {0} and fullfill your destiny. After these messages.", username), transitionLength));
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

    private IEnumerator MuteAll() {
        StartCoroutine(ShowMessage("Disabling Sound...", transitionLength));
        yield return new WaitForSeconds(1);
        GetComponent<AudioSource>().mute = true;

        yield return new WaitForSeconds(2);
        StartCoroutine(ShowMessage("Removing Color...", transitionLength));
        yield return new WaitForSeconds(1);
        var bwBG = Resources.Load<Sprite>("Morning-BW");
        messagePanel.GetComponentInParent<Image>().sprite = bwBG;

    }
}

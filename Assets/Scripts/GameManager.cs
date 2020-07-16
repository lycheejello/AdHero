using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Linq;

public class GameManager : MonoBehaviour {

    private bool welcomeComplete = false;

    private AdsManager adsManager;
    [SerializeField] private GameObject welcomeSequence;
    [SerializeField] private GameObject mainGame;

    private string username;
    private string password;
    [SerializeField] TMP_InputField loginField;
    [SerializeField] TMP_Text placeholder;

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
    [SerializeField] private MessageAnimationManager messageAnimation;
    [SerializeField] private TMP_Text tapToContinue;

    [SerializeField] private float messageSpeed = 0.05f;
    private IEnumerator messageCoroutine;

    [SerializeField] private GameObject loginPanel;

    private BankManager gameData;
    [SerializeField] private GameObject coinsText;

    [SerializeField] private Image transitionImage;
    [SerializeField] private float transitionLength = 0.4f;


    void Start() {
        adsManager = GetComponent<AdsManager>();
        adsManager.SetAdCompleteHandler(OnAdCompleted);
        gameData = GetComponent<BankManager>();

        coinsText.GetComponentInChildren<TMP_Text>().SetText(gameData.coins.ToString());

        button1.onClick.AddListener(OnButton1);
        button2.onClick.AddListener(OnButton2);

        welcomeComplete = PlayerPrefs.GetInt("welcomeComplete") == 0 ? false : true;

        //welcomeComplete = true;
        if (welcomeComplete) {
            username = PlayerPrefs.GetString("username");
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
        PlayerPrefs.SetInt("welcomeComplete", 1);
        PlayerPrefs.SetString("username", username);
        PlayerPrefs.SetString("password", password);
        mainGame.SetActive(true);
        welcomeSequence.SetActive(false);
        GetComponent<MainGame>().InitMainGame(username);
    }

    void Update() {
        if (adsManager.isPlayingAd || welcomeComplete) {
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
                StartCoroutine(ShowLogin("Free username quote!"));
                TMP_Text buttonText = button1.GetComponentInChildren<TMP_Text>();
                if (IsValidUsername(loginField.text)) {
                    button1.enabled = true;
                    buttonText.SetText("{0}", GetUsernameCost(loginField.text));
                } else {
                    button1.enabled = false;
                    buttonText.SetText("--");
                }
                /*
                if (loginField.interactable) {
                } else {
                    buttonText.SetText("Unlock username\n({0}g)", GetUsernameCost(loginField.text));
                }
                */
                break;
            case 4:
                placeholder.text = "Password";
                StartCoroutine(ShowLogin("Create password"));
                TMP_Text pwText = button1.GetComponentInChildren<TMP_Text>();
                pwText.SetText("{0}", GetPasswordCost(loginField.text));

                if (IsValidUsername(loginField.text)) {
                    button1.enabled = true;
                    pwText.SetText("{0}", GetUsernameCost(loginField.text));
                } else {
                    button1.enabled = false;
                    pwText.SetText("--");
                }
                /*
                if (loginField.interactable) {
                } else {
                    pwText.SetText("Unlock password\n({0}g)", GetPasswordCost(loginField.text));
                }
                */
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
                StartCoroutine(ShowMessage("The following\nsponsored message\nwill be a tutorial on\nhow to play this\ngame.", transitionLength));
                StartCoroutine(PlayAnimation(MessageAnimationManager.State.Wave, 0));
                Invoke("ShowTapToContinue", transitionLength + 4f);
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
                gameData.AddCoins(20);
                break;
            case AdsManager.AdType.RewardedInterstitial:
                //AddCoins(50);
                break;
        }

        switch (welcomeIndex) {
            case 2:
                StartCoroutine(ShowMessage("Here are some coins\nto start you off.", transitionLength));
                StartCoroutine(PlayAnimation(MessageAnimationManager.State.Earn, 0));
                gameData.AddCoins(50); 
                Invoke("ShowTapToContinue", transitionLength + 2.5f);
                break;
            case 6:
                StartCoroutine(ShowMessage(string.Format("Your AdHero premium trial has ended.", username), transitionLength));
                Invoke("ShowTapToContinue", transitionLength + 2f);
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
        if (messageCoroutine != null) {
            StopCoroutine(messageCoroutine);
        }
        messageText.text = "";
        tapToContinue.enabled = false;
        yield return new WaitForSeconds(waitTime / 2);
        titlePanel.SetActive(false);
        messagePanel.SetActive(true);
        loginPanel.SetActive(false);

        yield return new WaitForSeconds(waitTime / 2);
        messageCoroutine = TypeDialogue(msg, messageText);
        StartCoroutine(messageCoroutine);
    }

    private IEnumerator PlayAnimation(MessageAnimationManager.State state, float delay) {
        yield return new WaitForSeconds(delay);
        messageAnimation.SetAnimation(state);
        yield return null;
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

        if (msg.Substring(msg.Length - 3) == "...") {
            int i = 2;
            while (true) {
                if (i <= 1) { 
                    text.SetText(text.text += ".");
                    i += 1;
                } else {
                    text.SetText(msg.Substring(0, msg.Length - 2));
                    i = 0;
                }
                yield return new WaitForSeconds(messageSpeed * 10);
            }
        }
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
            gameData.AddCoins(-5);
            UnlockUsername();
        } else if (welcomeIndex == 3) { //username
            if (gameData.AddCoins(-1 * GetUsernameCost(loginField.text))) {
                username = loginField.text;
                loginField.text = "";
                OnProceed();
            }
        } else { //must be password
            if (gameData.AddCoins(-1 * GetUsernameCost(loginField.text))) {
                password = loginField.text;
                StartCoroutine(ShowMessage(string.Format("Go forth, {0}\nand fullfill your\ndestiny...\n\nafter these\nmessages.", username), transitionLength));
                StartCoroutine(PlayAnimation(MessageAnimationManager.State.Sword, transitionLength / 2));
                Invoke("ShowTapToContinue", transitionLength + 4.5f);
                OnProceed();
            }
        }
    }

    public void OnButton2() {
        adsManager.ShowAd(AdsManager.AdType.Rewarded);
    }

    private bool IsValidUsername(string name) {
        return name.Length <= 18 && name.Length > 0;
    }

    private int GetUsernameCost(string name) {
        /*
        character cost( base cost = 50g. Dach additional chracter = -2g cost. 
        If there is at least 1 letter and 1 number, each characater cost is -5g. 
        if there is at least 1 number, 1 letter, and 1 symbol, each character cost is -10g.
        If there is at least 1 number, 1 letter, 1 symbol, and 1 cap letter, everything is free
        */
        int cost = 50;
        int discountPerLetter = 0;

        int categoriesHit = 0;
        if (!name.All(char.IsLetterOrDigit)) {
            categoriesHit += 1;
        }
        if (name.Any(char.IsDigit)) {
            categoriesHit += 1;
        }
        if (name.Any(char.IsUpper)) {
            categoriesHit += 1;
        }
        if (name.Any(char.IsLower)) {
            categoriesHit += 1;
        }

        switch (categoriesHit) {
            case 1:
                discountPerLetter = 2;
                break;
            case 2:
                discountPerLetter = 5;
                break;
            case 3:
            case 4:
                discountPerLetter = 10;
                break;
            default:
                discountPerLetter = 0;
                break;
        }
        if (categoriesHit == 4) {
            return -10;
        } else {
            return Mathf.Max(cost - discountPerLetter * name.Length, 0);
        }
    }

    private int GetPasswordCost(string password) {
        return GetUsernameCost(password);
    }

    public void UnlockUsername() {
        loginField.interactable = true;
    }

    private IEnumerator MuteAll() {
        StartCoroutine(ShowMessage("Disabling sound...", transitionLength));
        StartCoroutine(PlayAnimation(MessageAnimationManager.State.Sad, 0));
        yield return new WaitForSeconds(1.5f);
        GetComponent<AudioSource>().mute = true;

        yield return new WaitForSeconds(3);
        StartCoroutine(ShowMessage("Removing color...", transitionLength));
        yield return new WaitForSeconds(1.5f);
        var bwBG = Resources.Load<Sprite>("Background/Background-half-bw");
        messagePanel.GetComponentInParent<Image>().sprite = bwBG;
        var bwGround = Resources.Load<Sprite>("Background/Ground2-bw");
        messagePanel.GetComponentInChildren<Image>().sprite = bwGround;
        StartCoroutine(PlayAnimation(MessageAnimationManager.State.Fall, 1f));
        Invoke("ShowTapToContinue", 3f);
    }

}

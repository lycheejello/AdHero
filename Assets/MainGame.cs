using System.Collections;
using UnityEngine;
using TMPro;
using System.Xml.Serialization;
using UnityEngine.UI;

public class MainGame : MonoBehaviour {

    private AdsManager adsManager;

    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject earnCoinsPanel;
    [SerializeField] GameObject accountPanel;
    [SerializeField] GameObject levelUpPanel;
    //[SerializeField] GameObject messagePanel;

    [SerializeField] GameObject coinsText;

    private int coins = 50;
    private string username = "";
    private int level = 1;

    private bool soundUnlocked = false;
    private bool colorUnlocked = false;

    // Start is called before the first frame update
    void Start() {

    }

    public void InitMainGame(string username, int coin) {
        adsManager = GetComponentInParent<AdsManager>();
        adsManager.SetAdCompleteHandler(OnAdCompleted);

        SetUsername(username);
        SetCoins(coin);
        coinsText.GetComponent<TMP_Text>().SetText(coins.ToString());

        NavMainMenu();
    }

    // Update is called once per frame
    void Update() {
        if (Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) {
                HandleInput();
            }
        } else if (Input.GetMouseButtonDown(0)) {
            HandleInput();
        }

    }

    private void OnAdCompleted(AdsManager.AdType adType) {
        if (adType == AdsManager.AdType.Rewarded) {
            AddCoins(20);
        }
    }

    public void HandleInput() {
        //if (messagePanel.activeSelf) {
            //messagePanel.SetActive(false);
        //}
    }
    private void NavReset() {
        mainPanel.SetActive(false);
        accountPanel.SetActive(false);
        //messagePanel.SetActive(false);
        levelUpPanel.SetActive(false);
        earnCoinsPanel.SetActive(false);
    }

    public void NavMainMenu() {
        NavReset();
        mainPanel.SetActive(true);
    }

    public void NavEarnCoins() {
        NavReset();
        earnCoinsPanel.SetActive(true);
    }

    public void NavAccount() {
        NavReset();
        accountPanel.SetActive(true);
    }

    public void NavPremium() {
        adsManager.ShowAd(AdsManager.AdType.Rewarded);
    }

    public void OnEarnCoins() {
        adsManager.ShowAd(AdsManager.AdType.Rewarded);
    }

    public void NavLevelUp() {
        NavReset();
        levelUpPanel.SetActive(true);
    }

    /*
    private void NavLogin(string msg, buttonText, ) {
        titlePanel.SetActive(false);
        messagePanel.SetActive(false);
        loginPanel.SetActive(true);

        loginPanel.GetComponentInChildren<TMP_Text>().SetText(msg);
    }
    */


    public void OnLevelUp() {
        level += 1;
        AddCoins(-50);
        levelUpPanel.GetComponentInChildren<TMP_Text>().SetText("Your Hero Level\n{0}", level);
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

    public void SetCoins(int c) {
        coins = c;
    }

    public void SetUsername(string u) {
        username = u;
    }

    public void OnUnlockColor() {
        //https://www.vectorstock.com/royalty-free-vector/landscape-at-morning-for-game-background-vector-14966453

        AddCoins(-200);
        colorUnlocked = !colorUnlocked;
        ToggleColor();
    }

    private void ToggleColor() {

        Sprite bg;
        if (colorUnlocked) {
            bg =  Resources.Load<Sprite>("Morning");
        } else {
            bg = Resources.Load<Sprite>("Morning-BW");
        }

        GameObject[] panels = new GameObject[] {mainPanel, earnCoinsPanel, accountPanel, levelUpPanel};
        foreach (GameObject p in panels) {
            p.GetComponent<Image>().sprite = bg;
        }

}

    public void OnUnlockSound() {
        AddCoins(-200);
        ToggleSound();
    }

    private void ToggleSound() {
        AudioSource bg = GetComponentInParent<AudioSource>();
        bg.mute = !bg.mute;
    }

    //TODO screen transitions: https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/HOWTO-UIScreenTransition.html
}

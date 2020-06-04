using System.Collections;
using UnityEngine;
using TMPro;

public class MainGame : MonoBehaviour {

    private AdsManager adsManager;

    [SerializeField] private GameObject earnCoinsPanel;
    [SerializeField] private GameObject levelUpPanel;
    //[SerializeField] private GameObject accountPanel;

    [SerializeField] private GameObject usernameText;
    [SerializeField] private GameObject levelText;
    [SerializeField] private GameObject coinsText;

    [SerializeField] private GameObject cam;
    private float cameraDistance = 50;
    private float cameraSpeed = 50;
    private float cameraTarget = 0;
    private IEnumerator camCR;


    [SerializeField] private SpriteRenderer bgSprite;

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

        NavEarnCoins();
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
        //mainPanel.SetActive(false);
        //accountPanel.SetActive(false);
        //messagePanel.SetActive(false);
        levelUpPanel.SetActive(false);
        earnCoinsPanel.SetActive(false);
        //premiumPanel.SetActive(false);
        //adsManager.HideBannerAd();
    }

    private void NavEarnCoins() {
        levelUpPanel.SetActive(false);
        earnCoinsPanel.SetActive(true);
    }

    private void NavLevelUp() {
        levelUpPanel.SetActive(true);
        earnCoinsPanel.SetActive(false);
    }

    public void OnEarnCoins() {
        adsManager.ShowAd(AdsManager.AdType.Rewarded);
    }

    public void OnLevelUp() {
        level += 1;
        AddCoins(-50);
        levelUpPanel.GetComponentInChildren<TMP_Text>().SetText("{0}", level);
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

    public void OnToggleScreen() {
        if (cameraTarget == 0) {
            cameraTarget = cameraDistance;
            NavLevelUp();
        } else {
            cameraTarget = 0;
            NavEarnCoins();
        }
        if (camCR != null) {
            StopCoroutine(camCR);
        }
        camCR = MoveCamera();
        StartCoroutine(camCR);
    }

    private IEnumerator MoveCamera() {
        while (Mathf.Abs(cam.transform.position.x - cameraTarget) > 0.001f) {
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, new Vector3(cameraTarget, 0, 0), cameraSpeed * Time.deltaTime);
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
            bg = Resources.Load<Sprite>("Morning");
        } else {
            bg = Resources.Load<Sprite>("Morning-BW");
        }

        bgSprite.sprite = bg;
    }

    public void OnUnlockSound() {
        AddCoins(-200);
        ToggleSound();
    }

    public void OnUnlockAds() {
        adsManager.HideBannerAd();
    }

    private void ToggleSound() {
        AudioSource bg = GetComponentInParent<AudioSource>();
        bg.mute = !bg.mute;
    }

    //TODO screen transitions: https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/HOWTO-UIScreenTransition.html
}

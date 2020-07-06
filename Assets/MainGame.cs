using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class MainGame : MonoBehaviour {

    private AdsManager adsManager;

    [SerializeField] private SpriteRenderer bgSprite;

    [SerializeField] private GameObject earnCoinsPanel;

    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private GameObject bonusButton;
    [SerializeField] private TMP_Text rewardText;

    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private TMP_Text levelCost;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text gemsText;
    [SerializeField] private Button levelUpButton;
    [SerializeField] private GameObject levelImages;
    [SerializeField] private GameObject levelParticles;


    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject coinShopPanel;
    [SerializeField] private GameObject coinShopContent;
    [SerializeField] private GameObject[] coinShopItemPrefab;

    [SerializeField] private GameObject noMoneyPanel;

    [SerializeField] private GameObject navLevel;
    [SerializeField] private TMP_Text usernameText;

    [SerializeField] private GameObject cam;
    private float cameraDistance = 50;
    [SerializeField]
    private float cameraSpeed = 100;
    private float cameraTarget = 0;
    private IEnumerator camCR;

    [SerializeField] private AnimationManager animationManager;

    private BankManager bank;
    private int level = 1;
    private int levelProgress = 0;
    private int gemsRewarded = 0;
    private bool bonusReward = false;

    private bool soundUnlocked = false;
    private bool colorUnlocked = false;

    private Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    private string[] spritesToLoad = { "Background/Background", "Background/Background-bw", "Background/Coin", "Background/Coin-bw"};

    private void LoadSprites() {
        foreach (string sprite in spritesToLoad) {

            sprites[sprite] = Resources.Load<Sprite>(sprite);
        }
    }

    public void InitMainGame(string username) {
        adsManager = GetComponentInParent<AdsManager>();
        adsManager.SetAdCompleteHandler(OnAdCompleted);
        bank = GetComponent<BankManager>();

        SetUsername(username);
        coinsText.GetComponent<TMP_Text>().SetText(bank.GetCoins().ToString());
        gemsText.GetComponent<TMP_Text>().SetText(bank.GetGems().ToString());

        LoadSprites();
        levelUpButton.onClick.AddListener(OnLevelUp);
        ResetCamera();
        DisplayBG();
        DisplayLevelProgress();
        DisplayLevelInfo();
        ToggleSound();
        SetupCoinShop();
        NavReset();
        NavEarnCoins();
    }

    private void OnAdCompleted(AdsManager.AdType adType) {
        if (adType == AdsManager.AdType.Rewarded) {
            if (bonusReward) {
                gemsRewarded *= 5;
                bonusButton.SetActive(false);
            } else {
                gemsRewarded = 5 * level;
                bonusButton.SetActive(true);
                bonusButton.GetComponentInChildren<TMP_Text>().SetText((gemsRewarded * 5).ToString());
            }
            rewardPanel.SetActive(true);
            rewardText.SetText(gemsRewarded.ToString());
        }
    }

    public void OnCollectReward() {
        AddCoins(gemsRewarded);
        gemsRewarded = 0;
        bonusReward = false;
        rewardPanel.SetActive(false);
    }

    public void OnDoubleReward() {
        bonusReward = true;
        OnAdReward();
    }

    private void NavReset() {
        levelUpPanel.SetActive(false);
        earnCoinsPanel.SetActive(false);
        rewardPanel.SetActive(false);
        shopPanel.SetActive(false);
    }

    private void NavEarnCoins() {
        levelUpPanel.SetActive(false);
        earnCoinsPanel.SetActive(true);
    }

    private void NavLevelUp() {
        levelUpPanel.SetActive(true);
        earnCoinsPanel.SetActive(false);
    }

    public void ToggleNoMoney() {
        noMoneyPanel.SetActive(false);
        //noMoneyPanel.SetActive(!noMoneyPanel.activeSelf);
    }

    public void OnShopItem() {
        shopPanel.SetActive(true);
        coinShopPanel.SetActive(false);
    }
    
    public void OnShopCoin() {
        coinShopPanel.SetActive(true);
    }

    public void OnShopClose() {
        shopPanel.SetActive(false);
        coinShopPanel.SetActive(false);
    }

    public void OnAdReward() {
        adsManager.ShowAd(AdsManager.AdType.Rewarded);
    }

    public void OnLevelUp() {
        if (AddCoins(-1 * GetLevelCost())) {
            levelProgress += 1;

            StartCoroutine(AnimateLevelBar());
            DisplayLevelProgress();

            if (levelProgress >= 3) {
                StartCoroutine(LevelUp());
            }
        }

    }

    private IEnumerator AnimateLevelBar() {
        float current = navLevel.GetComponentInChildren<Slider>().value;
        float totalTime = 0;
        float moveSpeed = 15;
        while (current != levelProgress) {
            current = Mathf.MoveTowards(current, levelProgress, Time.deltaTime * moveSpeed);
            navLevel.GetComponentInChildren<Slider>().value = current;
            totalTime += Time.deltaTime;
            yield return null;
        }
    }

    private void DisplayLevelProgress() {
        SpriteRenderer[] levelSprites = levelImages.GetComponentsInChildren<SpriteRenderer>();
        for(int i = 0; i < 3; i++) {
            if (i < levelProgress) {
                levelSprites[i].sprite = sprites["Background/Coin"];
            } else {
                levelSprites[i].sprite = sprites["Background/Coin-bw"];
            }
        }
    }

    private IEnumerator LevelUp() {
        PlayFireworks();
        level += 1;
        levelUpButton.interactable = false;
        yield return new WaitForSeconds(1f);
        levelProgress = 0;
        levelUpButton.interactable = true;
        DisplayLevelProgress();
        DisplayLevelInfo();
        yield return null;
    }

    private void DisplayLevelInfo() {
        levelUpPanel.GetComponentInChildren<TMP_Text>().SetText("{0}", level);
        levelCost.SetText("{0}", GetLevelCost());
        navLevel.GetComponentInChildren<TMP_Text>().SetText("{0}", level);
        StartCoroutine(AnimateLevelBar());
        //navLevel.GetComponentInChildren<Slider>().value = levelProgress;
    }

    private int GetLevelCost() {
        return (int)Math.Pow(1.5, level);
    }

    private void PlayFireworks() {
        foreach (ParticleSystem p in levelParticles.GetComponentsInChildren<ParticleSystem> ()) {
            p.Play();
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

        animationManager.MoveRaccoon();
    }

    private IEnumerator MoveCamera() {
        while (Mathf.Abs(cam.transform.position.x - cameraTarget) > 0.001f) {
            cam.transform.position = Vector2.MoveTowards(cam.transform.position, new Vector2(cameraTarget, 0), cameraSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private void ResetCamera() {
        cam.transform.position =  new Vector2(0, 0);
    }

    public void SetUsername(string u) {
        usernameText.SetText(u);
    }

    public void OnUnlockColor() {
        //https://www.vectorstock.com/royalty-free-vector/landscape-at-morning-for-game-background-vector-14966453

        //if (AddCoins(-200)) {
        if (true) {
            colorUnlocked = !colorUnlocked;
            DisplayBG();
            SetupCoinShop();
        }
    }

    private void DisplayBG() {
        bgSprite.sprite = colorUnlocked ? sprites["Background/Background"] : sprites["Background/Background-bw"];
    }

    public void OnUnlockSound() {
        //if (AddCoins(-200)) {
        if (true) {
            soundUnlocked = !soundUnlocked;
            ToggleSound();
        }
    }

    public void OnUnlockAds() {
        adsManager.HideBannerAd();
    }

    private void ToggleSound() {
        AudioSource bg = GetComponentInParent<AudioSource>();
        bg.mute = !soundUnlocked;
    }

    public void OnMysteryBox() {
        bank.DisplayCoins(bank.GetCoins(), 0, coinsText);
        bank.DisplayCoins(bank.GetGems(), 0, gemsText);
        bank.ResetBank();
    }


    int[] cost = { 1, 9, 49, 99, 0, 999 };
    int[] reward = { 5, 480, 0, 4500, 1, 40000 };
    string[] bannerTag = { "Sale", "Limited Time", "Popular", "Best Deal", "Free", "+20%" };
    private void SetupCoinShop() {
        foreach (Transform child in coinShopContent.transform) {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < cost.Length; i++) {
            GameObject item = Instantiate(coinShopItemPrefab[colorUnlocked ? 0 : 1], new Vector3(0, 0, 0), Quaternion.identity);
            TMP_Text[] itemText = item.GetComponentsInChildren<TMP_Text>();

            itemText[0].text = reward[i].ToString();
            itemText[1].text = cost[i].ToString();
            itemText[2].text = bannerTag[i];

            item.transform.SetParent(coinShopContent.transform, false);

            int j = i;
            item.GetComponentInChildren<Button>().onClick.AddListener(() => OnBuy(j));
        }
    }

    private void OnBuy(int i) {
        if (AddCoins(-1 * cost[i])) {
            AddGems(reward[i]);
        }
    }

    public bool AddCoins(int c) {
        int startCoins = bank.GetCoins();
        int balance = bank.AddCoins(c);
        if (balance >= 0) {
            StartCoroutine(bank.DisplayCoins(startCoins, balance, coinsText));
            return true;
        } else {
            return false;
        }
    }

    public bool AddGems(int g) {
        int startGems = bank.GetGems();
        int balance = bank.AddGems(g);
        if (balance >= 0) {
            StartCoroutine(bank.DisplayCoins(startGems, balance, gemsText));
            return true;
        } else {
            return false;
        }
    }

    //TODO screen transitions: https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/HOWTO-UIScreenTransition.html
}

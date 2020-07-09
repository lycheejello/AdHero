using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.ComponentModel.Design;

public class MainGame : MonoBehaviour {

    private AdsManager adsManager;

    //Objects that go through color change
    [SerializeField] private SpriteRenderer bgSprite;
    [SerializeField] private SpriteRenderer ground;
    [SerializeField] private GameObject frontHills;
    [SerializeField] private GameObject backHills;
    [SerializeField] private Image shopColorImage;

    [SerializeField] private Image shopSoundImage;
    [SerializeField] private SpriteRenderer coinMound;

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
    [SerializeField] private GameObject prestigePanel;

    [SerializeField] private GameObject navLevel;
    [SerializeField] private TMP_Text usernameText;

    [SerializeField] private GameObject cam;
    private float cameraDistance = 50;
    [SerializeField]
    private float cameraSpeed = 100;
    private float cameraTarget = 0;
    private IEnumerator camCR;

    [SerializeField] private AnimationManager animationManager;
    [SerializeField] private AudioManager sfxManager;

    private BankManager gameData;
    private bool bonusReward = false;

    private bool soundUnlocked = false;
    private bool colorUnlocked = false;

    private Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    private string[] spritesToLoad = {  "Background/Background", "Background/Background-bw",
                                        "Background/Coin", "Background/Coin-bw",
                                        "Background/hill front", "Background/hill front-bw",
                                        "Background/hill back", "Background/hill back-bw",
                                        "Background/Ground2", "Background/Ground2-bw",
                                        "buttons/cmyk_circle", "buttons/cmyk_circle-bw",
                                        "buttons/sound_w", "buttons/sound_w-off",
                                        "Background/Gold Mound/GoldMound1of6", "Background/Gold Mound/GoldMound2of6", "Background/Gold Mound/GoldMound3of6", 
                                        "Background/Gold Mound/GoldMound4of6", "Background/Gold Mound/GoldMound5of6", "Background/Gold Mound/GoldMound6of6"};


    private void LoadSprites() {
        foreach (string sprite in spritesToLoad) {
            sprites[sprite] = Resources.Load<Sprite>(sprite);
        }
    }

    public void InitMainGame(string username) {
        adsManager = GetComponentInParent<AdsManager>();
        adsManager.SetAdCompleteHandler(OnAdCompleted);
        gameData = GetComponent<BankManager>();

        SetUsername(username);
        coinsText.GetComponent<TMP_Text>().SetText(gameData.coins.ToString());
        gemsText.GetComponent<TMP_Text>().SetText(gameData.gems.ToString());

        LoadSprites();
        levelUpButton.onClick.AddListener(OnLevelUp);
        ResetCamera();
        LoadPreferences();
        DisplayBG();
        DisplayLevelProgress();
        DisplayLevelInfo();
        DisplayCoinMound();
        SetupSound();
        SetupCoinShop();
        NavReset();
        NavEarnCoins();
    }

    private void OnAdCompleted(AdsManager.AdType adType) {
        if (adType == AdsManager.AdType.Rewarded) {
            rewardPanel.SetActive(true);
            if (bonusReward) {
                bonusButton.SetActive(false);
                rewardText.SetText(GetBonusReward().ToString());
            } else {
                bonusButton.SetActive(true);
                rewardText.SetText(GetPlayReward().ToString());
                bonusButton.GetComponentInChildren<TMP_Text>().SetText("Bonus ({0})", GetBonusReward());
            }
        }
    }

    private int GetPlayReward() {
        return 3 * gameData.level;
    }

    private int GetBonusReward() {
        int bonusMultiplier = 3;
        return GetPlayReward() * bonusMultiplier;
    }

    public void OnCollectReward() {
        if (bonusReward) {
            AddCoins(GetBonusReward());
            sfxManager.PlayCoinsMulti();
        } else {
            AddCoins(GetPlayReward());
            sfxManager.PlayCoinSingle();
        }
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
        sfxManager.PlayClick(0);
        noMoneyPanel.SetActive(false);
    }

    public void Prestige() {
        sfxManager.PlayCoinsMulti();
        DisplayCoinMound();
        prestigePanel.SetActive(true);
    }

    public void OnClosePrestige() {
        sfxManager.PlayClick(0);
        prestigePanel.SetActive(false);
    }

    public void OnShopItem() {
        sfxManager.PlayClick(1);
        shopPanel.SetActive(true);
        coinShopPanel.SetActive(false);
    }
    
    public void OnShopCoin() {
        sfxManager.PlayClick(1);
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
        if (AddCoins(10)) {
            gameData.ProgressLevel();

            //Player level up
            if (gameData.levelProgress == 0) {
                StartCoroutine(AnimateLevelBar(2, 3));
                StartCoroutine(LevelUp());
                //Player has prestiged
                sfxManager.PlayLevelUp();
                if (gameData.level == 1) {
                    Prestige();
                }
            } else {
                StartCoroutine(AnimateLevelBar());
            }
            DisplayLevelProgress();
        }
    }

    private IEnumerator AnimateLevelBar() {
        float current = navLevel.GetComponentInChildren<Slider>().value;
        return AnimateLevelBar(current, gameData.levelProgress);

    }

    private IEnumerator AnimateLevelBar(float from, float to) {
        float totalTime = 0;
        float moveSpeed = 15;
        float current = from;
        while(current != to) { 
            current = Mathf.MoveTowards(current, to, Time.deltaTime * moveSpeed);
            navLevel.GetComponentInChildren<Slider>().value = current;
            totalTime += Time.deltaTime;
            yield return null;
        }
    }

    private void DisplayLevelProgress() {
        SpriteRenderer[] levelSprites = levelImages.GetComponentsInChildren<SpriteRenderer>();
        for(int i = 0; i < 3; i++) {
            if (i < gameData.levelProgress) {
                levelSprites[i].sprite = sprites["Background/Coin"];
            } else {
                levelSprites[i].sprite = sprites["Background/Coin-bw"];
            }
        }
    }

    private IEnumerator LevelUp() {
        PlayFireworks();
        levelUpButton.interactable = false;

        yield return new WaitForSeconds(1f);
        levelUpButton.interactable = true;
        DisplayLevelProgress();
        DisplayLevelInfo();
        yield return null;
    }

    private void DisplayLevelInfo() {
        levelUpPanel.GetComponentInChildren<TMP_Text>().SetText("{0}", gameData.level);
        levelCost.SetText("{0}", GetLevelCost());
        navLevel.GetComponentInChildren<TMP_Text>().SetText("{0}", gameData.level);
        StartCoroutine(AnimateLevelBar());
    }

    private int GetLevelCost() {
        return gameData.level;
    }


    private void PlayFireworks() {
        foreach (ParticleSystem p in levelParticles.GetComponentsInChildren<ParticleSystem> ()) {
            p.Play();
        }
    }

    public void OnToggleScreen() {
        sfxManager.PlayClick(0);
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

    private void LoadPreferences() {
        soundUnlocked = PlayerPrefs.GetInt("soundUnlocked") == 1;
        colorUnlocked = PlayerPrefs.GetInt("colorUnlocked") == 1;
    }

    public void OnUnlockColor() {
        //https://www.vectorstock.com/royalty-free-vector/landscape-at-morning-for-game-background-vector-14966453
        if (AddCoins(-40)) {
            colorUnlocked = !colorUnlocked;
            PlayerPrefs.SetInt("colorUnlocked", colorUnlocked ? 1 : 0);
            DisplayBG();
            SetupCoinShop();
        }
    }

    private void DisplayBG() {
        if (colorUnlocked) {
            bgSprite.sprite = sprites["Background/Background"];
            ground.sprite = sprites["Background/Ground2"];
            shopColorImage.sprite = sprites["buttons/cmyk_circle"];

            foreach(SpriteRenderer hillFront in frontHills.GetComponentsInChildren<SpriteRenderer>()) {
                hillFront.sprite = sprites["Background/hill front"];
            }

            foreach(SpriteRenderer hillBack in backHills.GetComponentsInChildren<SpriteRenderer>()) {
                hillBack.sprite = sprites["Background/hill back"];
            }
        } else {
            bgSprite.sprite = sprites["Background/Background-bw"];
            ground.sprite = sprites["Background/Ground2-bw"];
            shopColorImage.sprite = sprites["buttons/cmyk_circle-bw"];

            foreach(SpriteRenderer hillFront in frontHills.GetComponentsInChildren<SpriteRenderer>()) {
                hillFront.sprite = sprites["Background/hill front-bw"];
            }

            foreach(SpriteRenderer hillBack in backHills.GetComponentsInChildren<SpriteRenderer>()) {
                hillBack.sprite = sprites["Background/hill back-bw"];
            }
        }
    }

    private void DisplayCoinMound() {
        if (gameData.prestige <= 0 || gameData.prestige > 6) {
            coinMound.sprite = null;
        } else {
            coinMound.sprite = sprites["Background/Gold Mound/GoldMound" + gameData.prestige + "of6"];
        }
    }

    public void OnUnlockSound() {
        if (AddCoins(-40)) {
            soundUnlocked = !soundUnlocked;
            PlayerPrefs.SetInt("soundUnlocked", soundUnlocked ? 1 : 0);
            SetupSound();
        }
    }

    private void SetupSound() {
        AudioSource bgMusic = GetComponentInParent<AudioSource>();
        bgMusic.mute = !soundUnlocked;
        sfxManager.mute = !soundUnlocked;
        shopSoundImage.sprite = soundUnlocked ? sprites["buttons/sound_w"] : sprites["buttons/sound_w-off"];
    }

    public void OnMysteryBox() {
        gameData.DisplayCoins(gameData.coins, 0, coinsText);
        gameData.DisplayCoins(gameData.gems, 0, gemsText);
        gameData.ResetBank();
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
        sfxManager.PlayCoinSingle();
        int startCoins = gameData.coins;
        int balance = gameData.AddCoins(c);
        if (balance >= 0) {
            StartCoroutine(gameData.DisplayCoins(startCoins, balance, coinsText));
            return true;
        } else {
            return false;
        }
    }

    public bool AddGems(int g) {
        int startGems = gameData.gems;
        int balance = gameData.AddGems(g);
        if (balance >= 0) {
            StartCoroutine(gameData.DisplayCoins(startGems, balance, gemsText));
            return true;
        } else {
            return false;
        }
    }
}

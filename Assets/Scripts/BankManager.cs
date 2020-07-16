using System.Collections;
using UnityEngine;
using TMPro;

public class BankManager : MonoBehaviour
{
    [SerializeField] private AudioManager sfxManager;

    [SerializeField] private TMP_Text welcomeCoinsText;

    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text gemsText;

    [SerializeField] private GameObject noMoneyPanel;
    [SerializeField] private float coinDuration = 2f;

    public int coins { get; private set; }
    public int gems { get; private set; }
    public int level { get; private set; }
    public int levelProgress { get; private set; }
    public int prestige { get; private set; }


    void Awake() {
        coins = PlayerPrefs.GetInt("coins", 0);
        gems = PlayerPrefs.GetInt("gems", 0);
        level = PlayerPrefs.GetInt("level", 1);
        levelProgress = PlayerPrefs.GetInt("levelProgress", 0);
        prestige = PlayerPrefs.GetInt("prestige", 0);

        welcomeCoinsText.GetComponent<TMP_Text>().SetText(coins.ToString());
        coinsText.GetComponent<TMP_Text>().SetText(coins.ToString());
        gemsText.GetComponent<TMP_Text>().SetText(gems.ToString());
        noMoneyPanel.SetActive(false);
    }

    public IEnumerator DisplayCoins(int startCoins, int targetCoins, TMP_Text text) {
        float totalTime = 0;
        int showCoins = startCoins;
        while (showCoins != targetCoins) {
            showCoins = Mathf.RoundToInt(Mathf.Lerp(startCoins, targetCoins, totalTime / coinDuration));
            text.SetText(showCoins.ToString());
            totalTime += Time.deltaTime;
            yield return null;
        }
    }

    public void ToggleNoMoney() {
        noMoneyPanel.SetActive(!noMoneyPanel.activeSelf);
    }

    public int ProgressLevel() {
        levelProgress += 1;
        if (levelProgress >= 3) {
            LevelUp();
            levelProgress = 0;
        }
        SaveLevelProgress();
        return levelProgress;
    }

    private int LevelUp() {
        level += 1;
        if (prestige < 6 && level >= 10) {
            prestige += 1;
            level = 1;
        }
        SaveLevelProgress();
        return level;
    }

    public bool AddCoins(int c) {
        sfxManager.PlayCoinSingle();
        int startCoins = coins;
        if (coins + c < 0) {
            ToggleNoMoney();
            return false;
        } else {
            coins += c;
            PlayerPrefs.SetInt("coins", coins);

            StartCoroutine(DisplayCoins(startCoins, coins, welcomeCoinsText));
            StartCoroutine(DisplayCoins(startCoins, coins, coinsText));
            return true;
        }
    }

    public bool AddGems(int g) {
        sfxManager.PlayCoinSingle();
        int startGems = gems;
        if (gems + g < 0) {
            ToggleNoMoney();
            return false;
        } else {
            gems += g;
            PlayerPrefs.SetInt("gems", gems);
            StartCoroutine(DisplayCoins(startGems, gems, gemsText));
            return true;
        }
    }

    private void SaveLevelProgress () {

        PlayerPrefs.SetInt("levelProgress", levelProgress);
        PlayerPrefs.SetInt("level", level);
        PlayerPrefs.SetInt("prestige", prestige);
    }

    public void ResetBank() {
        AddCoins(-1 * coins);
        AddGems(-1 * gems);
        PlayerPrefs.SetInt("coins", 0);
        PlayerPrefs.SetInt("gems", 0);
        PlayerPrefs.SetInt("welcomeComplete", 0);

        PlayerPrefs.SetInt("level", 1);
        PlayerPrefs.SetInt("levelProgress", 0);
        PlayerPrefs.SetInt("prestige", 0);

        PlayerPrefs.SetInt("soundUnlocked", 0);
        PlayerPrefs.SetInt("colorUnlocked", 0);
    }
}

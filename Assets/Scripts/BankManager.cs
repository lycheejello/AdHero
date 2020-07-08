using System.Collections;
using UnityEngine;
using TMPro;

public class BankManager : MonoBehaviour
{
    public int coins { get; private set; }
    public int gems { get; private set; }
    public int level { get; private set; }
    public int levelProgress { get; private set; }
    public int prestige { get; private set; }

    [SerializeField] private GameObject noMoneyPanel;
    [SerializeField] private float coinDuration = 2f;

    void Awake() {
        coins = PlayerPrefs.GetInt("coins", 0);
        gems = PlayerPrefs.GetInt("gems", 0);
        level = PlayerPrefs.GetInt("level", 1);
        level = PlayerPrefs.GetInt("levelProgress", 0);
        prestige = PlayerPrefs.GetInt("levelPrestige", 0);
    }

    public int AddCoins(int c) {
        if (coins + c < 0) {
            ToggleNoMoney();
            return -1;
        }
        coins += c;
        PlayerPrefs.SetInt("coins", coins);
        return coins;
    }

    public int AddGems(int g) {
        if (gems + g < 0) {
            ToggleNoMoney();
            return -1;
        }
        gems += g;
        PlayerPrefs.SetInt("gems", gems);
        return gems;
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
        return levelProgress;
    }

    private int LevelUp() {
        level += 1;
        if (level >= 10) {
            prestige += 1;
            level = 0;
        }
        return level;
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
    }

}

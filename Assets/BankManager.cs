using System.Collections;
using UnityEngine;
using TMPro;

public class BankManager : MonoBehaviour
{
    private int coins;
    private int gems;

    [SerializeField] private GameObject noMoneyPanel;
    [SerializeField] private float coinDuration = 2f;

    void Start() {
        coins = PlayerPrefs.GetInt("coins");
        gems = PlayerPrefs.GetInt("gems");
    }

    public int GetCoins() {
        return coins;
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

    public int GetGems() {
        return gems;
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

    public void ResetBank() {
        AddCoins(-1 * coins);
        AddGems(-1 * gems);
        PlayerPrefs.SetInt("coins", 0);
        PlayerPrefs.SetInt("gems", 0);
        PlayerPrefs.SetInt("welcomeComplete", 0);
    }

}

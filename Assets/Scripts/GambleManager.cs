using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class GambleManager : MonoBehaviour
{

    [SerializeField]
    private GameObject gamblePanel;

    private string[] rewardTitle = { "Aww", "Yay?", "Woo!", "Alright!", "YESS!", "JACKPOT!!" };
    [SerializeField]
    private Sprite[] rewardImages;
    [SerializeField]
    private Image rewardImage;
    private int[] rewardWeights = { 371, 200, 100, 35,  15, 4 };
    private int[] payout = { 0, 10, 20, 50, 100, 1000};

    BankManager gameData;

    private void Awake() {
        gameData = GetComponent<BankManager>();
        OnClose();
    }

    public void OnGamble() {
        if (gameData.AddGems(-100)) {
            gamblePanel.SetActive(true);
            Roll();
        }
    }

    public void OnClose() {
        gamblePanel.SetActive(false);
    }

    private void Roll() {
        int totalWeight = rewardWeights.Sum();
        int roll = Random.Range(0, totalWeight);

        int rewardIndex = 0;
        while (roll > rewardWeights[rewardIndex]) {
            roll -= rewardWeights[rewardIndex];
            rewardIndex += 1;
        }

        gameData.AddCoins(payout[rewardIndex]);
        gamblePanel.GetComponentsInChildren<TMP_Text>()[0].text = rewardTitle[rewardIndex];
        gamblePanel.GetComponentsInChildren<TMP_Text>()[1].text = payout[rewardIndex].ToString();
        rewardImage.sprite = rewardImages[rewardIndex];
    }
}

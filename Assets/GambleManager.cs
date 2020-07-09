using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GambleManager : MonoBehaviour
{

    [SerializeField]
    private GameObject gamblePanel;
    // Start is called before the first frame update

    public void OnGamble() {
        gamblePanel.SetActive(true);
        Roll();
    }

    public void OnClose() {
        gamblePanel.SetActive(false);
    }


    private void Roll() {
        float roll = Random.value;
        if (roll < .25) {

        } else if (roll >= .25 && roll < .75) {

        } else {

        }
    }
}

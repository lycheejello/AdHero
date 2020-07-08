using UnityEngine;

public class AudioManager : MonoBehaviour
{

    [SerializeField]
    private AudioSource audioSource;
    
    [SerializeField]
    private AudioClip[] coinClips;

    public bool mute { get; set; }

    public void Awake() {
        if (!audioSource) {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void PlayCoinSingle() {
        if (!mute) {
            audioSource.PlayOneShot(coinClips[0]);
        }
    }

    public void PlayCoinsMulti() {
        if (!mute) {
            audioSource.PlayOneShot(coinClips[1]);
        }
    }
    public void Stop() {
        audioSource.Stop(); 
    }

}

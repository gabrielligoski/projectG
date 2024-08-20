using System.Collections;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void playSFXClip(AudioClip clip, Transform spawnTranform, float volume, float delay)
    {
        StartCoroutine(playSound(clip, spawnTranform, volume, delay));
    }

    IEnumerator playSound(AudioClip clip, Transform spawnTranform, float volume, float delay)
    {
        yield return new WaitForSeconds(delay);
        var soundObject = new GameObject(clip.name);
        soundObject.transform.position = spawnTranform.position;
        var source = soundObject.AddComponent<AudioSource>();

        source.clip = clip;

        source.volume = volume;

        source.Play();

        float clipLen = source.clip.length;
        Destroy(soundObject, clipLen);
    }

}

using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [SerializeField] private AudioSource SFXObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void playSFXClip(AudioClip clip, Transform spawnTranform, float volume)
    {
        AudioSource source = Instantiate(SFXObject, spawnTranform.position, Quaternion.identity);

        source.clip = clip;

        source.volume = volume;

        source.Play();

        float clipLen = source.clip.length;

        Destroy(source, clipLen);
    }
}

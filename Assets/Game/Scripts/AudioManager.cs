using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource audioSource;
    private readonly int sampleRate = 44100;

    // Caged audio clips to avoid runtime instantiation hitching
    private AudioClip[] connectClips;
    private AudioClip[] matchClips;
    private AudioClip[] milestoneClips;

    private AudioSource musicSource;
    private AudioClip backgroundMusicClip;
    private float lastDangerTickTime = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D Sound
            
            PreGenerateAudioClips();

            // Setup dedicated music source
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = 0.25f;
            backgroundMusicClip = CreateMusicLoopClip();
            musicSource.clip = backgroundMusicClip;
            musicSource.Play();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPlaying && musicSource != null)
        {
            if (ScoreManager.Instance != null && DifficultyManager.Instance != null)
            {
                float target = DifficultyManager.Instance.ActiveGoal;
                float progress = target > 0 ? Mathf.Clamp01((float)ScoreManager.Instance.CurrentScore / target) : 0f;
                // Pitch rises dynamically as player approaches the level goal
                musicSource.pitch = Mathf.Lerp(1.0f, 1.25f, progress);
            }
        }
        else if (musicSource != null)
        {
            musicSource.pitch = 1.0f;
        }
    }

    private AudioClip CreateMusicLoopClip()
    {
        float duration = 4.0f; // 4-second arpeggiated loop
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];
        float[] notes = { 220f, 246.94f, 277.18f, 329.63f, 369.99f }; // A3, B3, C#4, E4, F#4 pentatonic scale

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            int noteIndex = (int)(t * 4f) % notes.Length;
            float freq = notes[noteIndex];

            // 16th note envelope pop
            float noteT = t % 0.25f;
            float env = Mathf.Clamp01(1f - (noteT / 0.25f));

            float sine = Mathf.Sin(2f * Mathf.PI * freq * t);
            samples[i] = sine * env * 0.12f;
        }

        AudioClip clip = AudioClip.Create("MusicLoop", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void PreGenerateAudioClips()
    {
        // 1. Connect Clips (15 steps)
        int[] pentatonicScale = { 0, 2, 4, 7, 9, 12, 14, 16, 19, 21, 24, 26, 28, 31, 33 };
        connectClips = new AudioClip[pentatonicScale.Length];
        for (int i = 0; i < pentatonicScale.Length; i++)
        {
            float freq = 440f * Mathf.Pow(1.059463f, pentatonicScale[i]);
            connectClips[i] = CreateSynthClip(freq, 0.12f, true);
        }

        // 2. Match Clips (3 levels)
        matchClips = new AudioClip[3];
        matchClips[0] = CreateChordClip(440f, 0.3f);    // A4 chord
        matchClips[1] = CreateChordClip(554.37f, 0.3f); // C#5 chord
        matchClips[2] = CreateChordClip(659.25f, 0.3f); // E5 chord

        // 3. Milestone Clips (6 levels)
        milestoneClips = new AudioClip[6];
        for (int i = 0; i < 6; i++)
        {
            float freq = 220f * (i + 2);
            milestoneClips[i] = CreateSynthClip(freq, 0.6f, false);
        }
    }

    public void PlayDangerWarningSound(bool fast)
    {
        float interval = fast ? 0.3f : 0.75f;
        if (Time.time - lastDangerTickTime >= interval)
        {
            lastDangerTickTime = Time.time;
            float freq = fast ? 800f : 350f;
            float duration = fast ? 0.08f : 0.14f;
            audioSource.PlayOneShot(CreateSynthClip(freq, duration, true), 0.5f);
        }
    }

    public void PlayConnectSound(int chainIndex)
    {
        if (connectClips == null || connectClips.Length == 0) return;
        int idx = Mathf.Clamp(chainIndex, 0, connectClips.Length - 1);
        audioSource.PlayOneShot(connectClips[idx], 0.6f);
    }

    public void PlayMatchSound(int chainLength)
    {
        if (matchClips == null || matchClips.Length == 0) return;
        int idx = 0;
        if (chainLength >= 15) idx = 2;
        else if (chainLength >= 8) idx = 1;

        audioSource.PlayOneShot(matchClips[idx], 0.8f);
    }

    public void PlayMilestoneSound(int milestoneLevel)
    {
        if (milestoneClips == null || milestoneClips.Length == 0) return;
        int idx = Mathf.Clamp(milestoneLevel, 0, milestoneClips.Length - 1);
        audioSource.PlayOneShot(milestoneClips[idx], 1.0f);
    }

    private AudioClip CreateSynthClip(float frequency, float duration, bool isPluck)
    {
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = isPluck 
                ? Mathf.Clamp01(1f - (t / duration)) 
                : Mathf.Sin(t * Mathf.PI / duration);

            float sineValue = Mathf.Sin(2f * Mathf.PI * frequency * t);
            float triangleValue = Mathf.PingPong(t * frequency * 2f, 1f) * 2f - 1f;

            samples[i] = (sineValue * 0.7f + triangleValue * 0.3f) * envelope * 0.15f;
        }

        AudioClip clip = AudioClip.Create("SynthBeep", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateChordClip(float baseFrequency, float duration)
    {
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float root = baseFrequency;
        float third = baseFrequency * 1.25f;  
        float fifth = baseFrequency * 1.5f;   
        float octave = baseFrequency * 2.0f;  

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Clamp01(1f - (t / duration));

            float wave = Mathf.Sin(2f * Mathf.PI * root * t) +
                         Mathf.Sin(2f * Mathf.PI * third * t) +
                         Mathf.Sin(2f * Mathf.PI * fifth * t) +
                         Mathf.Sin(2f * Mathf.PI * octave * t);

            samples[i] = (wave / 4f) * envelope * 0.2f;
        }

        AudioClip clip = AudioClip.Create("SynthChord", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}

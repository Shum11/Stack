using UnityEngine;

public class SoundManager : MonoBehaviour
{
    AudioSource src;
    AudioClip placeClip;
    AudioClip perfectClip;
    AudioClip overClip;

    public void Build()
    {
        src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;
        placeClip = MakeTone(340f, 0.08f, Wave.Square);
        perfectClip = MakeChord(0.16f);
        overClip = MakeSlide(0.45f);
    }

    public void PlayPlace()
    {
        src.PlayOneShot(placeClip, 0.55f);
    }

    public void PlayPerfect()
    {
        src.PlayOneShot(perfectClip, 0.7f);
    }

    public void PlayOver()
    {
        src.PlayOneShot(overClip, 0.8f);
    }

    enum Wave { Sine, Square, Saw }

    static AudioClip MakeTone(float freq, float dur, Wave wave)
    {
        int rate = 44100;
        int n = Mathf.Max(1, (int)(dur * rate));
        float[] data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            float f = freq;
            float env = 1f - (i / (float)n);
            float s = Sample(t, f, wave);
            data[i] = s * env;
        }
        AudioClip c = AudioClip.Create("tone", n, 1, rate, false);
        c.SetData(data, 0);
        return c;
    }

    static AudioClip MakeChord(float dur)
    {
        int rate = 44100;
        int n = Mathf.Max(1, (int)(dur * rate));
        float[] data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            float f = 660f + (880f - 660f) * Mathf.Clamp01(t / dur);
            float env = Mathf.Exp(-6f * t / dur);
            data[i] = (Sample(t, f, Wave.Sine) * 0.8f + Sample(t, f * 2f, Wave.Sine) * 0.3f) * env;
        }
        AudioClip c = AudioClip.Create("perfect", n, 1, rate, false);
        c.SetData(data, 0);
        return c;
    }

    static AudioClip MakeSlide(float dur)
    {
        int rate = 44100;
        int n = Mathf.Max(1, (int)(dur * rate));
        float[] data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            float f = Mathf.Lerp(420f, 130f, t / dur);
            float env = 1f - (t / dur);
            data[i] = Sample(t, f, Wave.Saw) * env;
        }
        AudioClip c = AudioClip.Create("over", n, 1, rate, false);
        c.SetData(data, 0);
        return c;
    }

    static float Sample(float t, float freq, Wave wave)
    {
        float phase = (t * freq) % 1f;
        switch (wave)
        {
            case Wave.Square: return phase < 0.5f ? 1f : -1f;
            case Wave.Saw: return 2f * phase - 1f;
            default: return Mathf.Sin(phase * Mathf.PI * 2f);
        }
    }
}
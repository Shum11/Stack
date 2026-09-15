using UnityEngine;

public class SoundManager : MonoBehaviour
{
    AudioSource src;
    AudioSource music;
    AudioClip placeClip;
    AudioClip perfectClip;
    AudioClip overClip;
    AudioClip bgClip;

    public void Build()
    {
        src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;
        placeClip = MakeTone(340f, 0.08f, Wave.Square);
        perfectClip = MakeChord(0.16f);
        overClip = MakeSlide(0.45f);

        bgClip = MakeMusic(0.22f);
        music = gameObject.AddComponent<AudioSource>();
        music.clip = bgClip;
        music.loop = true;
        music.volume = 0.45f;
        music.Play();
    }

    public void SetMusic(bool on)
    {
        if (music == null) return;
        if (on && !music.isPlaying) music.Play();
        if (!on && music.isPlaying) music.Pause();
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

    static AudioClip MakeMusic(float eighth)
    {
        // Lead (semitones above C5), one per eighth, rests = -100
        var lead = new[]
        {
            0, 4, 7, 4, 9, 7, 4, 0,
            2, 4, 7, 4, 7, 2, 0, -100,
            0, 4, 7, 4, 9, 12, 9, 7,
            4, 7, 9, 7, 4, 2, 0, -100
        };
        // Bass roots (semitones above C2), one per quarter note (2 eighths)
        var bass = new[] { 0, 0, 7, 7, 9, 9, 7, 7 };

        int rate = 44100;
        float total = eighth * lead.Length;
        int n = Mathf.Max(1, (int)(total * rate));
        float[] data = new float[n];
        float baseLead = 523.25f;   // C5
        float baseBass = 65.41f;    // C2

        for (int i = 0; i < lead.Length; i++)
        {
            if (lead[i] >= 0)
            {
                float freq = baseLead * Mathf.Pow(2f, lead[i] / 12f);
                Pluck(data, eighth * i, eighth * 0.9f, freq, 0.5f, rate, Wave.Square);
            }
        }
        for (int i = 0; i < bass.Length; i++)
        {
            float freq = baseBass * Mathf.Pow(2f, bass[i] / 12f);
            Pluck(data, eighth * i * 2f, eighth * 1.6f, freq, 0.42f, rate, Wave.Square);
        }

        AudioClip c = AudioClip.Create("music", n, 1, rate, false);
        c.SetData(data, 0);
        return c;
    }

    static void Pluck(float[] data, float start, float dur, float freq, float amp, int rate, Wave wave)
    {
        int i0 = Mathf.Max(0, (int)(start * rate));
        int i1 = Mathf.Min(data.Length, i0 + (int)(dur * rate));
        for (int i = i0; i < i1; i++)
        {
            float t = (i - i0) / (float)rate;
            float env = Mathf.Exp(-t / 0.14f);
            if (t < 0.008f) env = t / 0.008f;
            data[i] += Sample(t, freq, wave) * amp * env;
        }
    }
}
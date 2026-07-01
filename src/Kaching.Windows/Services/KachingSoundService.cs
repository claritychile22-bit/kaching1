using System.IO;
using System.Media;

namespace Kaching.Windows.Services;

public sealed class KachingSoundService
{
    private readonly string soundPath;

    public KachingSoundService(string appDataPath)
    {
        soundPath = Path.Combine(appDataPath, "kaching.wav");
        EnsureSoundFile();
    }

    public void Play()
    {
        try
        {
            using var player = new SoundPlayer(soundPath);
            player.Play();
        }
        catch
        {
            SystemSounds.Asterisk.Play();
        }
    }

    private void EnsureSoundFile()
    {
        if (File.Exists(soundPath))
        {
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(soundPath)!);
        using var stream = File.Create(soundPath);
        using var writer = new BinaryWriter(stream);

        const int sampleRate = 44100;
        var samples = BuildTone(sampleRate, 0.34);
        var dataLength = samples.Length * sizeof(short);

        writer.Write("RIFF"u8.ToArray());
        writer.Write(36 + dataLength);
        writer.Write("WAVEfmt "u8.ToArray());
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(sampleRate);
        writer.Write(sampleRate * sizeof(short));
        writer.Write((short)sizeof(short));
        writer.Write((short)16);
        writer.Write("data"u8.ToArray());
        writer.Write(dataLength);

        foreach (var sample in samples)
        {
            writer.Write(sample);
        }
    }

    private static short[] BuildTone(int sampleRate, double durationSeconds)
    {
        var count = (int)(sampleRate * durationSeconds);
        var samples = new short[count];

        for (var i = 0; i < count; i++)
        {
            var t = i / (double)sampleRate;
            var envelope = Math.Exp(-5.2 * t);
            var firstBell = Math.Sin(2 * Math.PI * 1320 * t);
            var secondBell = Math.Sin(2 * Math.PI * 1760 * Math.Max(0, t - 0.08));
            samples[i] = (short)(short.MaxValue * 0.35 * envelope * (firstBell + 0.65 * secondBell));
        }

        return samples;
    }
}

using System;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;

namespace BasicSynthesizer
{
    public partial class Synth : Form
    {
        private const int SAMPLE_RATE = 41000;
        private const short BITS_PER_SAMPLE = 16;

        public Synth()
        {
            InitializeComponent();
        }

        private void BasicSynthesizer_KeyDown(object sender, KeyEventArgs e)
        {
            short[] wave = new short[SAMPLE_RATE];
            byte[] binaryWave = new byte[SAMPLE_RATE * sizeof(short)];
            float frequency = 440f;
            int samplesPerWaveLength = (int)(SAMPLE_RATE / frequency);
            short ampStep = (short)((short.MaxValue * 2) / samplesPerWaveLength);
            short tempSample;

            foreach (Oscillator oscillator in this.Controls.OfType<Oscillator>())
            {
                switch (oscillator.WaveForm)
                {
                    case WaveForm.Sine:
                        for (int i = 0; i < SAMPLE_RATE; i++)
                        {
                        wave[i] = Convert.ToInt16(short.MaxValue * Math.Sin((2 * Math.PI * frequency / SAMPLE_RATE) * i));
                        }
                        break;

                    case WaveForm.Square:
                        for (int i = 0; i < SAMPLE_RATE; i++)
                        {
                            wave[i] = Convert.ToInt16(short.MaxValue * Math.Sign(Math.Sin((2 * Math.PI * frequency / SAMPLE_RATE) * i)));
                        }
                        break;

                    case WaveForm.Saw:
                        for (int i = 0; i < SAMPLE_RATE; i++)
                        {
                            tempSample = -short.MaxValue;
                            for (int j = 0; j < samplesPerWaveLength && i < SAMPLE_RATE; j++)
                            {
                                tempSample += ampStep;
                                wave[i++] = Convert.ToInt16(tempSample);

                            }
                            i--;
                        }
                        break;

                    case WaveForm.Triangle:
                        tempSample = -short.MaxValue;
                        for (int i = 0; i < SAMPLE_RATE; i++)
                        {
                            if (Math.Abs(tempSample + ampStep) > short.MaxValue)
                            {
                                ampStep = (short)-ampStep;
                            }
                            tempSample += ampStep;
                            wave[i] = Convert.ToInt16(tempSample);
                        }
                        break;

                    case WaveForm.Noise:
                        Random rnd = new Random();
                        for (int i = 0; i < SAMPLE_RATE; i++)
                        {
                            wave[i] = (short)rnd.Next(-short.MaxValue, short.MinValue);
                        }

                        break;
                }
            
            }
            
            Buffer.BlockCopy(wave, 0, binaryWave, 0, wave.Length * sizeof(short));
            using (MemoryStream memoryStream = new MemoryStream())
            using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
            {
                int blockAlign = BITS_PER_SAMPLE / 8;
                int numOfChannels = 1;
                int subChunkTwoSize = SAMPLE_RATE * numOfChannels * blockAlign;
                binaryWriter.Write(new[] { 'R', 'I', 'F', 'F' });
                binaryWriter.Write(36 + subChunkTwoSize);
                binaryWriter.Write(new[] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
                binaryWriter.Write(16);
                binaryWriter.Write((short)1);
                binaryWriter.Write((short)numOfChannels);
                binaryWriter.Write(SAMPLE_RATE);
                binaryWriter.Write(SAMPLE_RATE * blockAlign);
                binaryWriter.Write((short)blockAlign);
                binaryWriter.Write(BITS_PER_SAMPLE);
                binaryWriter.Write(new[] { 'd', 'a', 't', 'a' });
                binaryWriter.Write(subChunkTwoSize);
                binaryWriter.Write(binaryWave);
                memoryStream.Position = 0;
                new SoundPlayer(memoryStream).Play();
            }
        }   
    }

    public enum WaveForm
    {
        Sine, Square, Saw, Triangle, Noise
    }
}

using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MusicAnalyzer.Models
{
    public class WAVReader
    {
        public short[] channelBuffer;
        public int bitrate, channels, sampleSize=1024, divider = 1;
        public double totalSeconds;
        public void read(String filename, int bitrate, int channels)
        {
            using (WaveFileReader reader = new WaveFileReader(filename))
            {
                if (16 == reader.WaveFormat.BitsPerSample)
                {
                    this.bitrate = bitrate;
                    this.channels = channels;
                    this.totalSeconds = reader.TotalTime.TotalSeconds;
                    byte[] buffer = new byte[reader.Length];
                    int read = reader.Read(buffer, 0, buffer.Length);
                    short[] musicBuffer = new short[read/2];
                    Buffer.BlockCopy(buffer, 0, musicBuffer, 0, read);
                    this.channelBuffer = new short[musicBuffer.Length / (divider * channels)];
                    for (int i = 0; i < channelBuffer.Length; i++)
                        channelBuffer[i] = musicBuffer[divider * i * channels];
                }
                else Console.WriteLine("Reading WAV file failed. Only works with 16 bit audio!");
            }
        }

        public short[] getDefaultRange(int starter)
        {
            /*
            int range = bitrate / (divider * 2);
            range = (int)Math.Floor(Math.Log(range, 2));
            range = (int)Math.Pow(2, range);
            int scale = range / sampleSize;
            short[] channelData = new short[sampleSize];
            for (int i = 0; i < range; i ++)
            {
                channelData[i] = channelBuffer[i + starter];
            }
            return channelData;
            */
            short[] channelData = new short[sampleSize];
            Array.Copy(channelBuffer, starter, channelData, 0, sampleSize);
            return channelData;
        }

        public MyFrame[] viewDataSource()
        {
            MyFrame[] resData = new MyFrame[channelBuffer.Length / 10];
            for (int i = 0; i < channelBuffer.Length/10; i ++)
            {
                resData[i] = new MyFrame(Math.Abs(channelBuffer[i * 10]), i*10000/bitrate);
            }
            return resData;
        }
    }
}

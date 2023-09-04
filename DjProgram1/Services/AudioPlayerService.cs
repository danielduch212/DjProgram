using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DjProgram1.Services
{
    class AudioPlayerService
    {
        public List<WaveStream> waveStreams;
        public List<Mp3FileReader> mp3Files;

        public AudioPlayerService()
        {
            waveStreams = new List<WaveStream>();
            mp3Files = new List<Mp3FileReader>();
        }
        public void AddWaveStream(WaveStream waveStream)
        {
            waveStreams.Add(waveStream);
        }

        public void AddMp3File(Mp3FileReader mp3File)
        {
            mp3Files.Add(mp3File);
        }
    }
}

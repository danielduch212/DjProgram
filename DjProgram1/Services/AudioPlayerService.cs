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
        private List<WaveStream> waveStreams;
        private List<Mp3FileReader> mp3Files;

        public AudioPlayerService(List<WaveStream> waveStreams, List<Mp3FileReader> mp3Files)
        {
            this.waveStreams = waveStreams;
            this.mp3Files = mp3Files;
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

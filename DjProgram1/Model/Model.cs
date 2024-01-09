using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DjProgram1.Model.Data;

namespace DjProgram1.Model
{
    internal class Model
    {

        private List<AudioFile> audioFiles;

        public Model(List<AudioFile> audioFiles)
        {
            this.audioFiles = audioFiles;
        }

        public List<AudioFile> GetAudioFiles()
        {
            return audioFiles;
        }
        public int GetAudioFilesCount()
        {
            return audioFiles.Count;
        }
        public AudioFile GetAudioFile(int index)
        {
            return audioFiles[index];
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Mixer;

namespace DjProgram1.Data
{
    public class AudioFile
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public double? BPM { get; set; }
    }
}

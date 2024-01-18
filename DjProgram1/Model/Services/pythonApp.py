import os
import librosa
import soundfile as sf


def calculate_bpm(file_path):
    
    y, sr = librosa.load(file_path, sr=None) 

    tempo, _ = librosa.beat.beat_track(y=y, sr=sr)
    return tempo




def change_bpm(file_path, old_bpm, new_bpm, file_path_dir):

    y, sr = librosa.load(file_path, sr=None)

    rate = new_bpm / old_bpm

    y_changed = librosa.effects.time_stretch(y, rate=rate)


    file_name = os.path.basename(file_path)


    output_dir = file_path_dir
    output_file_path = os.path.join(output_dir, file_name)

    if not os.path.exists(output_dir):
        os.makedirs(output_dir)

    sf.write(output_file_path, y_changed, sr)


    return output_file_path
   
def return_time_stamps(file_path):

    y, sr = librosa.load(file_path)

    tempo, beats = librosa.beat.beat_track(y=y, sr=sr)

    beat_times = librosa.frames_to_time(beats, sr=sr)

    return beat_times

import os
import librosa


def calculate_bpm(file_path):
    """
    Calculate the BPM (beats per minute) of an audio file using the librosa library.

    Args:
    file_path (str): The path to the audio file.

    Returns:
    float: The calculated BPM of the audio file.
    """
    # Load the audio file
    y, sr = librosa.load(file_path, sr=None)  # Load with the default sample rate

    # Calculate the tempo (BPM)
    tempo, _ = librosa.beat.beat_track(y=y, sr=sr)
    return tempo
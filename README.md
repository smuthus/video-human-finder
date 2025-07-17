# Human Detection Video Snapshot Tool

This tool processes all `.mp4` videos in a specified directory (including subdirectories), detects humans in video frames using OpenCV Haar cascades, and saves snapshots of detected frames to an output directory. It supports parallel processing for faster execution.

## Features
- Detects humans in video frames using `haarcascade_fullbody.xml`
- Skips saving images that are 90% similar to the previous snapshot
- Processes every 5th frame for efficiency
- Shows progress percentage for each video
- Automatically creates video and image directories if they do not exist
- Prompts for custom video and image directories, with sensible defaults

## Requirements
- .NET 8
- OpenCvSharp4
- `haarcascade_fullbody.xml` in the project source directory

## Usage
1. Place your `.mp4` videos in the `video` folder in your project directory (or specify a custom directory when prompted).
2. Place `haarcascade_fullbody.xml` in your project directory.
3. Build and run the application.
4. Enter the video and image output directories when prompted, or press Enter to use defaults.
5. Snapshots will be saved in the `image` directory.

## Notes
- Only frames with detected humans are saved.
- Images very similar to the previous snapshot are skipped.
- If no videos are found, a message is displayed.

## Contribute
Please Contribute

paypal.me/smuthus

Email : smuthus333@gmail.com

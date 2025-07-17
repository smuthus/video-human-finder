using System;
using System.IO;
using System.Threading.Tasks;
using OpenCvSharp;

class Program
{
    static void Main(string[] args)
    {
        // Get the project source code directory (one level up from bin)
        string binDir = AppDomain.CurrentDomain.BaseDirectory;
        string projectDir = Directory.GetParent(binDir).Parent.Parent.Parent.FullName;
        string videoDir = Path.Combine(projectDir, "video");

        Console.Write($"Enter video directory (leave empty for default {videoDir}): ");
        string dir = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(dir))
            dir = videoDir;

        // Create video directory if it does not exist
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        Console.Write($"Enter image output directory (leave empty for default [project_dir]\\image): ");
        string imageDir = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(imageDir))
            imageDir = Path.Combine(projectDir, "image");

        // Create image directory if it does not exist
        if (!Directory.Exists(imageDir))
            Directory.CreateDirectory(imageDir);

        string[] videoFiles = Directory.GetFiles(dir, "*.mp4", SearchOption.AllDirectories);
        if (videoFiles.Length == 0)
        {
            Console.WriteLine("No video files found in the specified directory.");
            return;
        }

        string cascadePath = Path.Combine(projectDir, "haarcascade_fullbody.xml");

        Parallel.ForEach(videoFiles, videoPath =>
        {
            var cascade = new CascadeClassifier(cascadePath);
            Mat frame = new Mat();
            Mat prevSnapshot = null;

            using var capture = new VideoCapture(videoPath);
            if (!capture.IsOpened())
            {
                Console.WriteLine($"Error: Unable to open video file {videoPath}.");
                return;
            }

            int totalFrames = (int)capture.Get(VideoCaptureProperties.FrameCount);
            int snapshotCount = 0;
            int frameSkip = 5;
            int frameIndex = 0;
            int lastPercent = -1;

            Console.Write($"Processing {Path.GetFileName(videoPath)}:   0%");

            while (true)
            {
                if (!capture.Read(frame) || frame.Empty())
                    break;
                frameIndex++;
                if (frameIndex % frameSkip != 0)
                    continue;

                var grayFrame = frame.CvtColor(ColorConversionCodes.BGR2GRAY);
                var humans = cascade.DetectMultiScale(
                    grayFrame,
                    scaleFactor: 1.1,
                    minNeighbors: 5,
                    HaarDetectionTypes.ScaleImage,
                    new Size(50, 50)
                );

                foreach (var rect in humans)
                {
                    frame.Rectangle(rect, Scalar.Red, 2);
                }

                if (humans.Length > 0)
                {
                    Mat currentSnapshot = frame.Clone();
                    bool isSimilar = false;
                    if (prevSnapshot != null)
                    {
                        Mat prevGray = prevSnapshot.CvtColor(ColorConversionCodes.BGR2GRAY);
                        Mat currGray = currentSnapshot.CvtColor(ColorConversionCodes.BGR2GRAY);
                        var histPrev = new Mat();
                        var histCurr = new Mat();
                        Cv2.CalcHist(new[] { prevGray }, new[] { 0 }, null, histPrev, 1, new[] { 256 }, new[] { new Rangef(0, 256) });
                        Cv2.CalcHist(new[] { currGray }, new[] { 0 }, null, histCurr, 1, new[] { 256 }, new[] { new Rangef(0, 256) });
                        Cv2.Normalize(histPrev, histPrev, 0, 1, NormTypes.MinMax);
                        Cv2.Normalize(histCurr, histCurr, 0, 1, NormTypes.MinMax);
                        double similarity = Cv2.CompareHist(histPrev, histCurr, HistCompMethods.Correl);
                        if (similarity > 0.90)
                            isSimilar = true;
                        histPrev.Dispose();
                        histCurr.Dispose();
                        prevGray.Dispose();
                        currGray.Dispose();
                    }

                    if (!isSimilar)
                    {
                        string snapshotPath = Path.Combine(imageDir, $"snapshot_{Path.GetFileNameWithoutExtension(videoPath)}_{snapshotCount++}.png");
                        currentSnapshot.SaveImage(snapshotPath);
                        Console.WriteLine($"\nSnapshot saved: {snapshotPath}");
                        prevSnapshot?.Dispose();
                        prevSnapshot = currentSnapshot;
                    }
                    else
                    {
                        currentSnapshot.Dispose();
                    }
                }

                int percent = (int)((double)frameIndex / totalFrames * 100);
                if (percent != lastPercent)
                {
                    Console.Write($"\rProcessing {Path.GetFileName(videoPath)}: {percent,3}%");
                    lastPercent = percent;
                }
            }

            Console.WriteLine();
            capture.Release();
            prevSnapshot?.Dispose();
            prevSnapshot = null;
            frame.Dispose();
            cascade.Dispose();
        });
    }
}

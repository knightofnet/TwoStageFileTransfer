using System;
using System.ComponentModel;
using System.Text;
using System.Threading;
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.utils;
using TwoStageFileTransferCore.utils.events;

namespace TwoStageFileTransfer.utils
{
	/// <summary>
	/// An ASCII progress bar
	/// </summary>
	public class ProgressBar : IProgressTransfer //, IProgress<double>
	{
        public bool IsInitialized { get; private set; }

		private const int blockCount = 10;
		private readonly TimeSpan animationInterval = TimeSpan.FromSeconds(1.0 / 4);
		private const string animation = @"|/-\";

		private Timer timer;

		private double currentProgress ;
		private string currentProgressText = string.Empty;
		private string currentText = string.Empty;
		private bool disposed ;
		private int animationIndex ;

		public ProgressBar()
		{

		}

        public event TsftFileCreated TsftFileCreated;
        public event Action<double, string> WorkReportProgress;
        public event Action<string, double, BckgerReportType> Progress;
        public Func<bool> CheckIsCanceled { get; set; }

        public void Init()
        {
            timer = new Timer(TimerHandler);

            // A progress bar is only for temporary display in a console window.
            // If the console output is redirected to a file, draw nothing.
            // Otherwise, we'll end up with a lot of garbage in the target file.
            if (!Console.IsOutputRedirected)
            {
                ResetTimer();
            }

            IsInitialized = true;
        }

        public void OnTsftFileCreated(TsftFileCreatedArgs args)
        {
            
        }

        public void OnProgress(string text, double percentDone = double.NaN, BckgerReportType bclReportType = BckgerReportType.ProgressTextOnly)
		{
            switch (bclReportType)
            {
				case BckgerReportType.ProgressPbarText:
                case BckgerReportType.ProgressPbarOnly:
					Report(percentDone, text ?? string.Empty);
					break;
				case BckgerReportType.ProgressTextOnly:
                    Console.Title = text ;
                    break;

            }

            
			//Progress?.Invoke(text, percentDone, bclReportType);
        }


        public void Report(double value, string text)
        {
            if (!IsInitialized) throw new Exception("ProgressBar not initialized");

			// Make sure value is in [0..1] range
			value = Math.Max(0, Math.Min(1, value));
			Interlocked.Exchange(ref currentProgress, value);
			Interlocked.Exchange(ref currentProgressText, text);
		}

		private void TimerHandler(object state)
		{
			lock (timer)
			{
				if (disposed) return;

				int progressBlockCount = (int)(currentProgress * blockCount);
				int percent = (int)(currentProgress * 100);
				string text = string.Format("[{0}{1}] {2,3}% {3} {4}",
					new string('#', progressBlockCount), new string('-', blockCount - progressBlockCount),
					percent,
					animation[animationIndex++ % animation.Length],
					currentProgressText
					);
				UpdateText(text);

				ResetTimer();
			}
		}

		private void UpdateText(string text)
		{
			// Get length of common portion
			int commonPrefixLength = 0;
			int commonLength = Math.Min(currentText.Length, text.Length);
			while (commonPrefixLength < commonLength && text[commonPrefixLength] == currentText[commonPrefixLength])
			{
				commonPrefixLength++;
			}

			// Backtrack to the first differing character
			StringBuilder outputBuilder = new StringBuilder();
			outputBuilder.Append('\b', currentText.Length - commonPrefixLength);

			// Output new suffix
			outputBuilder.Append(text.Substring(commonPrefixLength));

			// If the new text is shorter than the old one: delete overlapping characters
			int overlapCount = currentText.Length - text.Length;
			if (overlapCount > 0)
			{
				outputBuilder.Append(' ', overlapCount);
				outputBuilder.Append('\b', overlapCount);
			}

			Console.Write(outputBuilder);
			currentText = text;
		}

		private void ResetTimer()
		{
			timer.Change(animationInterval, TimeSpan.FromMilliseconds(-1));
		}

		public void Dispose()
		{
			lock (timer)
			{
				disposed = true;
				UpdateText(string.Empty);
			}
		}

        
    }

}

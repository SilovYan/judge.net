﻿using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Judge.Runner
{
    public sealed class RunResult
    {
        public RunStatus RunStatus { get; private set; }
        public int TimeConsumedMilliseconds { get; private set; }
        public int TimePassedMilliseconds { get; private set; }
        public int PeakMemoryBytes { get; private set; }
        public string TextStatus { get; private set; }
        public string Description { get; private set; }
        public string Output { get; private set; }

        private RunResult()
        {

        }

        private static readonly Regex TimeRegex = new Regex(@"\d*\.\d{2}", RegexOptions.Compiled);
        private static readonly Regex NumberRegex = new Regex(@"\d+", RegexOptions.Compiled);

        public static RunResult Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentOutOfRangeException(nameof(input));
            }

            var rows = input.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var startIndex = Array.FindIndex(rows, o => o.StartsWith("  time consumed"));

            var textStatus = rows[1];
            var exitCodeRow = rows.FirstOrDefault(o => o.StartsWith("  exit code"));
            var runStatus = GetStatus(textStatus, exitCodeRow != null ? int.Parse(NumberRegex.Match(exitCodeRow).Value) : 0);

            if (runStatus == RunStatus.InvocationFailed)
            {
                return InvocationFailed(textStatus, input);
            }

            var description = startIndex == 2 ? rows[1] : rows[2];
            var timeConsumedRow = rows[startIndex];
            var timePassedRow = rows[startIndex + 1];
            var peakMemoryRow = rows[startIndex + 2];

            var runResult = new RunResult
            {
                TimeConsumedMilliseconds =
                    (int)
                    (double.Parse(TimeRegex.Match(timeConsumedRow).Value, CultureInfo.InvariantCulture) * 1000),
                TimePassedMilliseconds =
                    (int)(double.Parse(TimeRegex.Match(timePassedRow).Value, CultureInfo.InvariantCulture) * 1000),
                PeakMemoryBytes = int.Parse(NumberRegex.Match(peakMemoryRow).Value),
                RunStatus = runStatus,
                TextStatus = textStatus,
                Description = description,
                Output = input
            };
            return runResult;
        }

        private static RunResult InvocationFailed(string textStatus, string output)
        {
            return new RunResult
            {
                RunStatus = RunStatus.InvocationFailed,
                TextStatus = textStatus,
                Output = output
            };
        }

        private static RunStatus GetStatus(string textStatus, int exitCode)
        {
            if (exitCode != 0)
            {
                return RunStatus.RuntimeError;
            }

            if (textStatus == "Time limit exceeded")
                return RunStatus.TimeLimitExceeded;

            if (textStatus == "Memory limit exceeded")
                return RunStatus.MemoryLimitExceeded;

            if (textStatus == "Crash")
                return RunStatus.RuntimeError;

            if (textStatus == "Idleness limit exceeded")
                return RunStatus.IdlenessLimitExceeded;

            if (textStatus.StartsWith("Security violation"))
                return RunStatus.SecurityViolation;

            if (textStatus.StartsWith("Invocation failed"))
                return RunStatus.InvocationFailed;

            if (textStatus.StartsWith("Program successfully terminated"))
                return RunStatus.Success;

            throw new ArgumentOutOfRangeException(nameof(textStatus));
        }
    }
}

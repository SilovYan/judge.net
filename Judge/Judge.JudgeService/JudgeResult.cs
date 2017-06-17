﻿using System;
using Judge.Checker;
using Judge.Compiler;
using Judge.Model.SubmitSolution;
using Judge.RunnerInterface;

namespace Judge.JudgeService
{
    internal sealed class JudgeResult
    {
        public CompileResult CompileResult { get; set; }
        public RunStatus? RunStatus { get; set; }
        public int? TimeConsumedMilliseconds { get; set; }
        public int? PeakMemoryBytes { get; set; }
        public string TextStatus { get; set; }
        public string Description { get; set; }
        public string Output { get; set; }
        public int TestRunsCount { get; set; }
        public int TestsPassedCount => GetStatus() == SubmitStatus.Accepted ? TestRunsCount : TestRunsCount - 1;
        public CheckStatus? CheckStatus { get; set; }
        public int? TimePassedMilliseconds { get; set; }

        public SubmitStatus GetStatus()
        {
            if (CompileResult.CompileStatus == CompileStatus.Error)
            {
                return SubmitStatus.CompilationError;
            }
            if (RunStatus == null)
                throw new InvalidOperationException();

            switch (RunStatus.Value)
            {
                case RunnerInterface.RunStatus.TimeLimitExceeded:
                    return SubmitStatus.TimeLimitExceeded;
                case RunnerInterface.RunStatus.MemoryLimitExceeded:
                    return SubmitStatus.MemoryLimitExceeded;
                case RunnerInterface.RunStatus.SecurityViolation:
                    return SubmitStatus.RuntimeError;
                case RunnerInterface.RunStatus.RuntimeError:
                    return SubmitStatus.RuntimeError;
                case RunnerInterface.RunStatus.InvocationFailed:
                    return SubmitStatus.ServerError;
                case RunnerInterface.RunStatus.IdlenessLimitExceeded:
                    return SubmitStatus.TimeLimitExceeded;
                case RunnerInterface.RunStatus.Success:
                    return GetCheckStatus();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private SubmitStatus GetCheckStatus()
        {
            if (CheckStatus == null)
                throw new InvalidOperationException();

            switch (CheckStatus.Value)
            {
                case Checker.CheckStatus.OK:
                    return SubmitStatus.Accepted;
                case Checker.CheckStatus.Fail:
                    return SubmitStatus.ServerError;
                default:
                    return SubmitStatus.WrongAnswer;
            }
        }
    }
}

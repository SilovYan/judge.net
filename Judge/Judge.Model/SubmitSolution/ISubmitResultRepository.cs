﻿using System.Collections.Generic;

namespace Judge.Model.SubmitSolution
{
    public interface ISubmitResultRepository
    {
        SubmitResult Get(long id);

        IEnumerable<SubmitResult> GetSubmits(ISpecification<SubmitResult> specification, int page, int pageSize);

        IEnumerable<long> GetSolvedProblems(ISpecification<SubmitResult> specification);

        SubmitResult DequeueUnchecked();
        int Count(ISpecification<SubmitResult> specification);
    }
}

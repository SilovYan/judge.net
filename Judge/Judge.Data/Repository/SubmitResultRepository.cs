﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Judge.Model;
using Judge.Model.SubmitSolution;

namespace Judge.Data.Repository
{
    internal sealed class SubmitResultRepository : ISubmitResultRepository
    {
        private readonly DataContext _context;

        public SubmitResultRepository(DataContext context)
        {
            _context = context;
        }

        public SubmitResult Get(long id)
        {
            return _context.Set<SubmitResult>().Where(o => o.Id == id).Include(o => o.Submit).FirstOrDefault();
        }

        public IEnumerable<SubmitResult> GetSubmits(ISpecification<SubmitResult> specification, int page, int pageSize)
        {
            if (page <= 0)
                throw new ArgumentOutOfRangeException(nameof(page));
            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize));

            var query = _context.Set<SubmitResult>() as IQueryable<SubmitResult>;

            query = query.Where(specification.IsSatisfiedBy);

            query = query.OrderByDescending(o => o.Id);

            var skip = (page - 1) * pageSize;

            if (skip > 0)
            {
                query = query.Skip(skip);
            }

            query = query.Take(pageSize);

            return query.Include(o => o.Submit).AsEnumerable();
        }

        public IEnumerable<long> GetSolvedProblems(ISpecification<SubmitResult> specification)
        {
            return _context.Set<SubmitResult>()
                .Where(specification.IsSatisfiedBy)
                .Where(o => o.Status == SubmitStatus.Accepted)
                .Select(o => o.Submit.ProblemId)
                .Distinct()
                .AsEnumerable();
        }

        public SubmitResult DequeueUnchecked()
        {
            var check = _context.DequeueSubmitCheck();

            if (check == null) return null;

            return _context.Set<SubmitResult>().Where(o => o.Id == check.SubmitResultId)
                .Include(o => o.Submit).First();
        }

        public int Count(ISpecification<SubmitResult> specification)
        {
            var query = _context.Set<SubmitResult>()
                .Where(specification.IsSatisfiedBy);

            return query.Count();
        }
    }
}

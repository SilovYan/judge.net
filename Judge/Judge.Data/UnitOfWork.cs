﻿using System;
using System.Transactions;
using Unity;
using Unity.Resolution;

namespace Judge.Data
{
    internal sealed class UnitOfWork : IUnitOfWork
    {
        private readonly TransactionScope _transactionScope;
        private readonly IUnityContainer _container;
        private readonly Func<DataContext> _contextCreator;
        private DataContext _context;
        public UnitOfWork(bool transactionRequired, IUnityContainer container, Func<DataContext> contextCreator)
        {
            _container = container;
            _contextCreator = contextCreator;
            if (transactionRequired)
            {
                _transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted });
            }
        }
        public void Dispose()
        {
            _transactionScope?.Dispose();
        }

        public void Commit()
        {
            if (_transactionScope == null)
            {
                throw new InvalidOperationException("Commit transaction called, but transaction wasn't opened");
            }

            if (_context != null)
            {
                _context.SaveChanges();
                _transactionScope.Complete();
            }
        }

        public T GetRepository<T>()
        {
            _context = _context ?? _contextCreator();

            return _container.Resolve<T>(new DependencyOverride<DataContext>(_context));
        }
    }
}

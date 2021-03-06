﻿using System.Configuration;
using System.Transactions;
using Judge.Data;
using Judge.Model.SubmitSolution;
using NUnit.Framework;
using Unity;
using Unity.Lifetime;

namespace Judge.Tests.Data.SubmitResultRepositoryTests
{
    [TestFixture]
    [Category("Database")]
    public class DequeueUncheckedTests
    {
        private IUnityContainer _container;
        private TransactionScope _transaction;

        [SetUp]
        public void SetUp()
        {
            _container = new UnityContainer();
            var connectionString = ConfigurationManager.ConnectionStrings["DataBaseConnection"].ConnectionString;
            _container.AddExtension(new DataContainerExtension<HierarchicalLifetimeManager>(connectionString));
            _transaction = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted });
        }

        [TearDown]
        public void TearDown()
        {
            _container.Dispose();
            _transaction.Dispose();
        }

        [Test]
        public void DequeueTest()
        {
            var factory = _container.Resolve<IUnitOfWorkFactory>();
            long submitId;
            using (var uow = factory.GetUnitOfWork(true))
            {
                var submitRepository = uow.GetRepository<ISubmitRepository>();
                var submit = ProblemSubmit.Create();
                submit.FileName = "main.cpp";
                submit.LanguageId = 1;
                submit.ProblemId = 1;
                submit.SourceCode = "123";
                submit.UserId = 1;
                submitRepository.Add(submit);
                uow.Commit();
                submitId = submit.Id;
            }

            using (var uow = factory.GetUnitOfWork(true))
            {
                var submitResultRepository = uow.GetRepository<ISubmitResultRepository>();
                var submit = submitResultRepository.DequeueUnchecked();

                Assert.NotNull(submit);
                Assert.That(submit.Submit.Id, Is.EqualTo(submitId));
            }
        }
    }
}

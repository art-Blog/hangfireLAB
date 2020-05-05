using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using HangfireLAB.Web.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace HangfireLAB.Web.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Enqueue驗證有呼叫Create方法()
        {
            //arrange
            var mockJobClient = Substitute.For<IBackgroundJobClient>();
            var demoJob = new MyJob(mockJobClient);

            //act
            demoJob.EnqueueJob();

            //assert
            mockJobClient.Received().Create(
                Arg.Is<Job>(p => p.Method.Name == nameof(MyJob.SomeWork01)),
                Arg.Any<EnqueuedState>()
            );
        }
    }
}
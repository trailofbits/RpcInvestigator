using Microsoft.VisualStudio.TestTools.UnitTesting;
using RpcInvestigator;
using RpcInvestigator.TabPages;
using System;

namespace UnitTests
{
    [TestClass]
    public class RpcAlpcServerListTests
    {
        [TestMethod]
        public void AtLeastOneALPC()
        {
            var env = new Environment();
            env.Initialize();
            var list = new RpcAlpcServerList(env.m_TabManager);
            list.Build();
            Assert.IsTrue(list.GetCount() > 0);
        }
    }
}

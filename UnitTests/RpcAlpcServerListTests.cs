using Microsoft.VisualStudio.TestTools.UnitTesting;
using RpcInvestigator;
using NtApiDotNet;
using RpcInvestigator.TabPages;
using System;
using System.Linq;
using System.Threading.Tasks;

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
            var list = new RpcAlpcServerList(null);
            list.Build();
            Assert.IsTrue(list.GetCount() > 0);
        }
    }
}

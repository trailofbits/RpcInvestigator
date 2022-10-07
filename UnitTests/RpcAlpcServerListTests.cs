//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RpcInvestigator;

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

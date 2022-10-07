//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RpcInvestigator;
using System;
using System.IO;
using System.Collections.Generic;

namespace UnitTests
{
    [TestClass]
    public class RpcServerListTests
    {
        [TestMethod]
        public void AtLeastOneRpcServer()
        {
            var env = new Environment();
            env.Initialize();
            var source = Path.Combine(System.Environment.SystemDirectory, "samsrv.dll");
            var dest = Path.GetTempFileName();
            try
            {
                File.Copy(source, dest, true);
            }
            catch (Exception ex)
            {
                Assert.Fail("Unable to copy '" + source + "' to '" + dest + "': " + ex.Message);
            }

            var list = new RpcServerList(null);
            list.Build(new List<string>() { dest }, env.m_Settings, env.m_Library);
            Assert.IsTrue(list.GetCount() > 0);
            //
            // Check that it was cached in the library
            //
            var servers = list.GetAll();
            Assert.IsTrue(servers.Count > 0);
            servers.ForEach(server =>
            {
                Assert.IsNotNull(env.m_Library.Get(server.InterfaceId));
            });
        }
    }
}

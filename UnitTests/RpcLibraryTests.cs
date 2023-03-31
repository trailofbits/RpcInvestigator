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
using System.Linq;
using System.Threading.Tasks;

namespace UnitTests
{

    [TestClass]
    public class RpcLibraryTests
    {
        public TestContext m_TestContext { get; set; }
        private static int s_MaxServers = 25;

        [TestMethod]
        public void LoadLibrary()
        {
            var env = new Environment();
            env.Initialize();
            Assert.AreEqual(0, env.m_Library.GetServerCount());
        }

        [TestMethod]
        public void SaveLibrary()
        {
            var env = new Environment();
            env.Initialize();
            Assert.AreEqual(0, env.m_Library.GetServerCount());
            Task.Run(() => env.m_Library.Refresh(
                env.m_Settings, null, null, null, s_MaxServers)).Wait();
            Assert.IsTrue(env.m_Library.GetServerCount() > 0);
            try
            {
                env.m_Library.Save();
            }
            catch (Exception ex)
            {
                Assert.Fail("Save threw an exception: " + ex.Message);
            }
        }

        [TestMethod]
        public void BuildLibrary()
        {
            //
            // This unit test covers building the library as well as adding
            // individual RPC servers, since that class cannot be instantiated.
            //
            var env = new Environment();
            env.Initialize();
            Assert.AreEqual(0, env.m_Library.GetServerCount());
            Task.Run(() => env.m_Library.Refresh(
                env.m_Settings, null, null, null, s_MaxServers)).Wait();
            Assert.IsTrue(env.m_Library.GetServerCount() > 0);
        }

        [TestMethod]
        public void ClearLibrary()
        {
            var env = new Environment();
            env.Initialize();
            Assert.AreEqual(0, env.m_Library.GetServerCount());
            Task.Run(() => env.m_Library.Refresh(
                env.m_Settings, null, null, null, s_MaxServers)).Wait();
            Assert.IsTrue(env.m_Library.GetServerCount() > 0);
            env.m_Library.Clear();
            Assert.AreEqual(0, env.m_Library.GetServerCount());
        }

        [DataTestMethod]
        public void GetSingleServerFromLibrary()
        {
            var env = new Environment();
            env.Initialize();
            Assert.AreEqual(0, env.m_Library.GetServerCount());
            Task.Run(() => env.m_Library.Refresh(
                env.m_Settings, null, null, null, s_MaxServers)).Wait();
            //
            // Pick a random server and get it from the library.
            //
            var servers = env.m_Library.GetAllServers();
            Assert.IsTrue(servers.Count > 0);
            Random r = new Random();
            var server = servers[r.Next(servers.Count)];
            Assert.IsNotNull(env.m_Library.Get(
                server.InterfaceId, server.InterfaceVersion));
        }

        [DataTestMethod]
        public void GetMultipleServersFromLibrary()
        {
            var env = new Environment();
            env.Initialize();
            Assert.AreEqual(0, env.m_Library.GetServerCount());
            Task.Run(() => env.m_Library.Refresh(
                env.m_Settings, null, null, null, s_MaxServers)).Wait();
            //
            // Pick a random server with multiple versions and get them from the library.
            //
            var servers = env.m_Library.GetServersWithMultipleVersions();
            Assert.IsTrue(servers.Count > 0);
            Random r = new Random();
            var server = servers.ElementAt(r.Next(servers.Count));
            Assert.IsNotNull(env.m_Library.Get(server.Key));
        }

        [DataTestMethod]
        public void RemoveSingleServerFromLibrary()
        {
            var env = new Environment();
            env.Initialize();
            Assert.AreEqual(0, env.m_Library.GetServerCount());
            Task.Run(() => env.m_Library.Refresh(
                env.m_Settings, null, null, null, s_MaxServers)).Wait();
            //
            // Pick a random server and remove it from the library.
            //
            var servers = env.m_Library.GetAllServers();
            Assert.IsTrue(servers.Count > 0);
            Random r = new Random();
            var server = servers[r.Next(servers.Count)];
            Assert.IsTrue(env.m_Library.Remove(
                server.InterfaceId, server.InterfaceVersion));
            Assert.IsNull(env.m_Library.Get(
                server.InterfaceId, server.InterfaceVersion));
        }

        [DataTestMethod]
        public void RemoveMultipleServersFromLibrary()
        {
            var env = new Environment();
            env.Initialize();
            Assert.AreEqual(0, env.m_Library.GetServerCount());
            Task.Run(() => env.m_Library.Refresh(
                env.m_Settings, null, null, null, s_MaxServers)).Wait();
            //
            // Pick a random server with multiple versions and remove them from the library.
            //
            var servers = env.m_Library.GetServersWithMultipleVersions();
            Assert.IsTrue(servers.Count > 0);
            Random r = new Random();
            var server = servers.ElementAt(r.Next(servers.Count));
            Assert.IsTrue(env.m_Library.Remove(server.Key));
            Assert.IsNull(env.m_Library.Get(server.Key));
        }

        [DataTestMethod]
        public void RemoveServerFromLibraryExpectFailure()
        {
            var env = new Environment();
            env.Initialize();
            Assert.AreEqual(0, env.m_Library.GetServerCount());
            Task.Run(() => env.m_Library.Refresh(
                env.m_Settings, null, null, null, s_MaxServers)).Wait();
            Assert.IsFalse(env.m_Library.Remove(Guid.Empty));
        }

        [DataTestMethod]
        public void AddServerToLibraryExpectFailure()
        {
            var env = new Environment();
            env.Initialize();
            Assert.AreEqual(0, env.m_Library.GetServerCount());
            Task.Run(() => env.m_Library.Refresh(
                env.m_Settings, null, null, null, s_MaxServers)).Wait();
            //
            // Pick a random server and try to add it to the library.
            //
            var servers = env.m_Library.GetAllServers();
            Assert.IsTrue(servers.Count > 0);
            Random r = new Random();
            var server = servers[r.Next(servers.Count)];
            Assert.IsFalse(env.m_Library.Add(server));
        }

        [DataTestMethod]
        public void MergeServersIntoLibraryExpectMultiple()
        {
            var env = new Environment();
            env.Initialize();
            Assert.AreEqual(0, env.m_Library.GetServerCount());
            Task.Run(() => env.m_Library.Refresh(
                env.m_Settings, null, null, null, s_MaxServers)).Wait();
            //
            // Pick a random range of servers, remove them from the library, then merge back.
            //
            var servers = env.m_Library.GetAllServers();
            Assert.IsTrue(servers.Count > 0);
            Random r = new Random();
            int start = r.Next(servers.Count - 1);
            int count = r.Next(servers.Count - start);
            var subset = servers.Skip(start).Take(count).ToList();
            subset.ForEach(server =>
            {
                Assert.IsTrue(env.m_Library.Remove(
                    server.InterfaceId, server.InterfaceVersion));
            });
            Assert.IsTrue(env.m_Library.Merge(subset));
            subset.ForEach(server =>
            {
                Assert.IsNotNull(env.m_Library.Get(
                    server.InterfaceId, server.InterfaceVersion));
            });
        }

        [DataTestMethod]
        public void MergeServersIntoLibraryExpectNone()
        {
            var env = new Environment();
            env.Initialize();
            Assert.AreEqual(0, env.m_Library.GetServerCount());
            Task.Run(() => env.m_Library.Refresh(
                env.m_Settings, null, null, null, s_MaxServers)).Wait();
            //
            // Merging the current list should result in no new servers.
            //
            var servers = env.m_Library.GetAllServers();
            Assert.IsTrue(servers.Count > 0);
            int before = env.m_Library.GetServerCount();
            Assert.IsTrue(env.m_Library.Merge(servers));
            int after = env.m_Library.GetServerCount();
            Assert.AreEqual(before, after);
        }

        [DataTestMethod]
        public void FindServerByKeywordInLibrary()
        {
            var env = new Environment();
            env.Initialize();
            Assert.AreEqual(0, env.m_Library.GetServerCount());
            Task.Run(() => env.m_Library.Refresh(
                env.m_Settings, null, null, null, s_MaxServers)).Wait();
            var servers = env.m_Library.Find(new RpcLibraryFilter() { Keyword = "lsass" });
            Assert.IsTrue(servers.Count > 0);
        }
    }
}

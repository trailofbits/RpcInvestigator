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

namespace UnitTests
{
    internal class Environment
    {
        public Settings m_Settings;
        public RpcLibrary m_Library;

        public Environment()
        {
        }

        public void Initialize()
        {
            TraceLogger.Initialize();

            try
            {
                m_Settings = Settings.LoadDefault();
                //
                // TODO: This is hard-coded to match our github workflow action that installs
                // the debugging tools from an MSI stored in Digital Ocean. This sucks.
                //
                m_Settings.m_DbghelpPath = "C:\\Program Files (x86)\\Windows Kits\\10\\bin\\10.0.22621.0\\x64\\dbghelp.dll";
                m_Settings.m_SymbolPath = @"srv*c:\\symbols*https://msdl.microsoft.com/download/symbols";
            }
            catch (Exception ex)
            {
                Assert.Fail("Exception loading default settings: " + ex.Message);
            }

            Assert.IsTrue(File.Exists(m_Settings.m_DbghelpPath));

            try
            {
                //
                // Important: each instance of Environment must setup a database unique
                // to the caller, as the database is not meant to be thread-safe and
                // tests can run in parallel.
                //
                m_Library = new RpcLibrary(Path.GetRandomFileName());
                m_Library.Load();
                //
                // Wipe any existing entries from a prior test.
                //
                m_Library.Clear();
                m_Library.Save();
            }
            catch (Exception ex)
            {
                Assert.Fail("Exception loading default library: " + ex.Message);
            }

            Assert.AreEqual(0, m_Library.GetServerCount());
        }
    }
}

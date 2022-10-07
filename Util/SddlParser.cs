//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using NtApiDotNet;
using System;
using System.Text;
using System.Security.AccessControl;
using System.Security.Principal;
using static NtApiDotNet.NtSecurity;
using AceType = NtApiDotNet.AceType;
using AceFlags = NtApiDotNet.AceFlags;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace RpcInvestigator.Util
{
    using static TraceLogger;

    public static class SddlParser
    {
        private static string SidToString(SecurityIdentifier SidValue)
        {
            try
            {
                return SidValue.Translate(typeof(NTAccount)).Value;
            }
            catch
            {
                return null;
            }
        }

        private static string AclToString(RawAcl Acl)
        {
            StringBuilder result = new StringBuilder();

            if (Acl == null)
            {
                return " {none}";
            }
            foreach (var ace in Acl)
            {
                var aceData = new byte[ace.BinaryLength];
                IntPtr acePointer = Marshal.AllocHGlobal(ace.BinaryLength);
                IntPtr currentPointer = acePointer;
                try
                {
                    ace.GetBinaryForm(aceData, 0);
                    Marshal.Copy(aceData, 0, currentPointer, ace.BinaryLength);
                    var header = (ACE_HEADER)Marshal.PtrToStructure(
                        currentPointer, typeof(ACE_HEADER));
                    //
                    // What follows the header depends on the ACE type, but the
                    // access mask, which is the last part we need, is always
                    // directly after the header.
                    //
                    currentPointer = IntPtr.Add(
                        currentPointer, Marshal.SizeOf(typeof(ACE_HEADER)));
                    var accessMask = Marshal.ReadInt32(currentPointer);
                    currentPointer = IntPtr.Add(currentPointer, 4);
                    var type = (AceType)header.AceType;
                    if (IsObjectAceType(type))
                    {
                        //
                        // Skip 32 bytes (object type and inherited object type)
                        //
                        currentPointer = IntPtr.Add(currentPointer, 32);
                    }

                    var sid = new Sid(currentPointer);
                    var ntAce = new Ace((AceType)header.AceType,
                        (AceFlags)header.AceFlags,
                        accessMask,
                        sid);
                    result.Append(ntAce.ToString() + ", ");
                }
                catch (Exception ex)
                {
                    Trace(TraceLoggerType.SddlParser,
                        TraceEventType.Error,
                        "Exception parsing SDDL string: " + ex.Message);
                    break;
                }
                finally
                {
                    Marshal.FreeHGlobal(acePointer);
                }
            }
            return result.ToString();
        }

        public static string Parse(string SddlString)
        {
            StringBuilder result = new StringBuilder();
            RawSecurityDescriptor descriptor;
            try
            {
                descriptor = new RawSecurityDescriptor(SddlString);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to create RawSecurityDescriptor from " +
                    "the provided SDDL string '" + SddlString + "':  " + ex.Message);
            }

            result.AppendLine("Owner: " + SidToString(descriptor.Owner));
            result.AppendLine("Group: " + SidToString(descriptor.Group));
            result.Append("Discretionary ACL: ");
            result.Append(AclToString(descriptor.DiscretionaryAcl));
            result.AppendLine();
            result.Append("System ACL: ");
            result.Append(AclToString(descriptor.SystemAcl));
            result.AppendLine();
            return result.ToString();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ACE_HEADER
    {
        public byte AceType;
        public byte AceFlags;
        public ushort AceSize;
    }
}

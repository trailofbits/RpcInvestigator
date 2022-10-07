//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RpcInvestigator.Util
{
    public static class MarshalHelper
    {
        public
        static
        object
        MarshalArbitraryType<T>(IntPtr Pointer)
        {
            try
            {
                return Convert.ChangeType(Marshal.PtrToStructure(Pointer, typeof(T)), typeof(T));
            }
            catch (Exception ex)
            {
                throw new Exception("Could not marshal pointer 0x" +
                    Pointer.ToInt64().ToString("X") +
                    " to destination type " + typeof(T).ToString() + ":  " +
                    ex.Message);
            }
        }

        public
        static
        List<T>
        MarshalArray<T>(IntPtr ArrayAddress, uint ElementCount)
        {
            IntPtr entry = ArrayAddress;
            var result = new List<T>();

            for (int i = 0; i < ElementCount; i++)
            {
                Debug.Assert(entry != IntPtr.Zero);

                result.Add((T)MarshalArbitraryType<T>(entry));

                //
                // Advance to the next structure.
                //
                unsafe
                {
                    entry = (IntPtr)((byte*)entry.ToPointer() + Marshal.SizeOf(typeof(T)));
                }
            }
            return result;
        }
    }
}

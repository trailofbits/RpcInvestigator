//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using System;

namespace RpcInvestigator.Util
{
    internal static class Formatting
    {
        static readonly string[] SizeSuffixes = { "B", "KB", "MB", "GB" };
        public static string InfoUnit(dynamic Value, int DecimalPlaces = 2)
        {
            if (!decimal.TryParse(Value.ToString(), out decimal value))
            {
                return "NaN!";
            }

            if (value > int.MaxValue)
            {
                return "NaN!";
            }
            else if (value < 0)
            {
                return "-" + InfoUnit(-value, DecimalPlaces);
            }

            int i = 0;
            while (Math.Round(value, DecimalPlaces) >= 1000)
            {
                value /= 1024;
                i++;
            }

            return string.Format("{0:n" + DecimalPlaces + "} {1}", value, SizeSuffixes[i]);
        }
    }
}

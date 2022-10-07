//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using BrightIdeasSoftware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RpcInvestigator.Util
{
    public static class ReflectionHelper
    {
        public
        static
        List<string>
        GetOlvAttributes(Type T)
        {
            try
            {
                var properties = T.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                if (properties.Count() > 0)
                {
                    var props = new List<string>();
                    foreach (var prop in properties)
                    {
                        if (prop.CustomAttributes.Count() != 1)
                        {
                            continue;
                        }
                        if (prop.CustomAttributes.ElementAt(0).AttributeType ==
                            typeof(OLVColumnAttribute))
                        {
                            props.Add(prop.Name);
                        }
                    }
                    return props;
                }
            }
            catch (Exception) { }
            return null;
        }
    }
}

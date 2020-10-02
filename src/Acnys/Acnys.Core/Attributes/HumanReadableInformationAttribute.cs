﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Acnys.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HumanReadableInformationAttribute : Attribute
    {
        public HumanReadableInformationAttribute(string name, string description = "")
        {
            Name = name;
            Description = description;
        }

        public string Name { get; }
        public string Description { get; }
    }
}
﻿using System.Collections.Generic;

namespace Eu.EDelivery.AS4.Fe.Runtime
{
    public class Property
    {
        public string FriendlyName { get; set; }
        public string Type { get; set; }
        public string Regex { get; set; }
        public string Description { get; set; }
        public IEnumerable<Property> Properties { get; set; }
    }
}
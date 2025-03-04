﻿using System.Collections.Generic;
using Microsoft.OData;

namespace Simple.OData.Client.V4.Adapter
{
    public class ResourceProperties
    {
        public ODataResource Resource { get; }
        public string TypeName { get; set; }
        public IEnumerable<ODataPropertyInfo> PrimitiveProperties => this.Resource.Properties;
        public IDictionary<string, ODataCollectionValue> CollectionProperties { get; set; }
        public IDictionary<string, ODataResource> StructuralProperties { get; set; }

        public ResourceProperties(ODataResource resource)
        {
            this.Resource = resource;
        }
    }
}
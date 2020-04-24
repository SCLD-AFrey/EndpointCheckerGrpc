using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
namespace EndpointChecker
{

    public partial class EndpointCheck
    {
        public EndpointCheck(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }
    }

}

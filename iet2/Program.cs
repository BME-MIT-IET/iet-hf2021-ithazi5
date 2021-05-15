using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;

namespace iet2
{
    class Program
    {
        static void Main(string[] args)
        {

            ManualTest manualTest = new ManualTest();
            manualTest.runAllManualTests();
        }
    }
}

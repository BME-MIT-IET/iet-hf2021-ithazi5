using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace iet2
{
    class ManualTest
    {
        private readonly int numOfTests = 1;
        private int numOfSucces = 0;

        private IGraph g = new Graph();

        public void runAllManualTests()
        {
            Console.WriteLine("Manual tests: ");
            if (test1())
                ++numOfSucces;
            else
                Console.WriteLine("test1 was not succesful!");
            Console.WriteLine("{0}/{1} test was succesfull.",numOfSucces, numOfTests);
        }

        private bool test1()
        {
            Notation3Parser n3parser = new Notation3Parser();
            try
            {
                //Load using Filename
                n3parser.Load(g, "szepmuveszeti.n3");
            }
            catch (RdfParseException parseEx)
            {
                //This indicates a parser error e.g unexpected character, premature end of input, invalid syntax etc.
                Console.WriteLine("Parser Error");
                Console.WriteLine(parseEx.Message);
                return false;
            }
            catch (RdfException rdfEx)
            {
                //This represents a RDF error e.g. illegal triple for the given syntax, undefined namespace
                Console.WriteLine("RDF Error");
                Console.WriteLine(rdfEx.Message);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }





            int i = 0;
            //foreach (Triple t in g.Triples)
            //{
            //    Console.WriteLine(t.ToString());
            //    i++;
            //    if (i == 100)
            //        break;
            //}
            return true;
        }

    }
}

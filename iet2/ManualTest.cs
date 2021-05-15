using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Update;

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

            TripleStore ts = new TripleStore();
            ts.Add(g);

            //Get the Query processor
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(ts);

            try
            {
                SparqlQuery q = new SparqlQueryParser().ParseFromString("PREFIX ecrm:<http://erlangen-crm.org/current/> SELECT ?actor {?actor a ecrm:E39_Actor}");
                Object results = processor.ProcessQuery(q);
                Console.WriteLine("First 5 actor:");
                if (results is SparqlResultSet)
                {
                    SparqlResultSet rset = (SparqlResultSet)results;
                    for (int idx = 0; idx < 5; ++idx)
                    {
                        Console.WriteLine(rset.Results[idx]);
                    }
                }



                Console.WriteLine("Delete the first actor:");
                SparqlUpdateCommandSet delete = new SparqlUpdateParser().ParseFromString("PREFIX ecrm:<http://erlangen-crm.org/current/> DELETE {<http://data.szepmuveszeti.hu/id/collections/museum/E39_Actor/f5f86cb4-b308-34b9-a73b-d40d474d735d> a ecrm:E39_Actor}WHERE{}");
                var updateProcessor = new LeviathanUpdateProcessor(ts);
                updateProcessor.ProcessCommandSet(delete);
                var result2 = processor.ProcessQuery(q);
                if (result2 is SparqlResultSet)
                {
                    SparqlResultSet rset = (SparqlResultSet)result2;
                    for (int idx = 0; idx < 5; ++idx)
                    {
                        Console.WriteLine(rset.Results[idx]);
                    }
                }




                SparqlUpdateCommandSet insert = new SparqlUpdateParser().ParseFromString("PREFIX ecrm:<http://erlangen-crm.org/current/> INSERT {<http://data.szepmuveszeti.hu/id/collections/museum/E39_Actor/f5f86cb4-b308-34b9-a73b-d40d474d735d> a ecrm:E39_Actor}WHERE{}");
                updateProcessor = new LeviathanUpdateProcessor(ts);
                updateProcessor.ProcessCommandSet(insert);
                var result3 = processor.ProcessQuery(q);
                Console.WriteLine("Insert the first actor:");
                if (result3 is SparqlResultSet)
                {

                    SparqlResultSet rset = (SparqlResultSet)result3;
                    for (int idx = 0; idx < 5; ++idx)
                    {
                        Console.WriteLine(rset.Results[idx]);
                    }
                }
            }
            catch(Exception e)
            {
                return false;
            }

            

            return true;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;



namespace iet2
{
    
    class Program
    {
        static void WriteAnRDF() { 
            
            IGraph g = new Graph();

            IUriNode dotNetRDF = g.CreateUriNode(UriFactory.Create("http://www.dotnetrdf.org"));
            IUriNode says = g.CreateUriNode(UriFactory.Create("http://example.org/says"));
            ILiteralNode helloWorld = g.CreateLiteralNode("Hello World");
            ILiteralNode bonjourMonde = g.CreateLiteralNode("Bonjour tout le Monde", "fr");

            g.Assert(new Triple(dotNetRDF, says, helloWorld));
            g.Assert(new Triple(dotNetRDF, says, bonjourMonde));

            Notation3Writer n3writer = new Notation3Writer();
            n3writer.Save(g, "Example.n3");

        }

        static void AQuery() {
            IGraph g = new Graph();
          
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
            }
            catch (RdfException rdfEx)
            {
                //This represents a RDF error e.g. illegal triple for the given syntax, undefined namespace
                Console.WriteLine("RDF Error");
                Console.WriteLine(rdfEx.Message);
            }
            TripleStore store = new TripleStore();
            store.Add(g);
            ISparqlDataset ds = new InMemoryDataset(g);

            //Get the Query processor
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(ds);

            SparqlQuery q = new SparqlQueryParser().ParseFromString("PREFIX ecrm:<http://erlangen-crm.org/current/> SELECT ?actor {?actor a ecrm:E39_Actor}");
            Object results = processor.ProcessQuery(q);
            if (results is SparqlResultSet)
            {
                //Print out the Results
                //Console.WriteLine("working up to this ");
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult result in rset.Results)
                {
                    Console.WriteLine(result.ToString());
                }
            }
        }
      
        private static void PerformanceTestWithDatabase()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
          
            IGraph g = new Graph();
          
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
            }
            catch (RdfException rdfEx)
            {
                //This represents a RDF error e.g. illegal triple for the given syntax, undefined namespace
                Console.WriteLine("RDF Error");
                Console.WriteLine(rdfEx.Message);
            }
          
            IBlankNode b = g.GetBlankNode("nodeID");
            if (b != null)
            {
                Console.WriteLine("Blank Node with ID " + b.InternalID + " exists in the Graph");
            }
            else
            {
                Console.WriteLine("No Blank Node with the given ID existed in the Graph");
            }

            sw.Stop();
            Console.WriteLine("Load time: " + sw.ElapsedMilliseconds + " ms");
        }

        private static void PerformanceTestWith1Triple()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            IGraph g = new Graph();

            IUriNode dotNetRDF = g.CreateUriNode(UriFactory.Create("http://www.dotnetrdf.org"));
            IUriNode says = g.CreateUriNode(UriFactory.Create("http://example.org/says"));
            ILiteralNode helloWorld = g.CreateLiteralNode("Hello World");

            g.Assert(new Triple(dotNetRDF, says, helloWorld));

            sw.Stop();
            Console.WriteLine("Execution time: " + sw.ElapsedMilliseconds + " ms");
        }

        private static void PerformanceTestWith2Triples()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            IGraph g = new Graph();

            IUriNode dotNetRDF = g.CreateUriNode(UriFactory.Create("http://www.dotnetrdf.org"));
            IUriNode says = g.CreateUriNode(UriFactory.Create("http://example.org/says"));
            ILiteralNode helloWorld = g.CreateLiteralNode("Hello World");
            ILiteralNode bonjourMonde = g.CreateLiteralNode("Bonjour tout le Monde", "fr");

            g.Assert(new Triple(dotNetRDF, says, helloWorld));
            g.Assert(new Triple(dotNetRDF, says, bonjourMonde));

            sw.Stop();
            Console.WriteLine("Execution time: " + sw.ElapsedMilliseconds + " ms");
        }

        private static void PerformanceTestWith10Triples()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            IGraph g = new Graph();

            IUriNode dotNetRDF = g.CreateUriNode(UriFactory.Create("http://www.dotnetrdf.org"));
            IUriNode says = g.CreateUriNode(UriFactory.Create("http://example.org/says"));
            ILiteralNode helloWorld = g.CreateLiteralNode("Hello World");
            ILiteralNode bonjourMonde = g.CreateLiteralNode("Bonjour tout le Monde", "fr");

            g.Assert(new Triple(dotNetRDF, says, helloWorld));
            g.Assert(new Triple(dotNetRDF, says, bonjourMonde));
            g.Assert(new Triple(dotNetRDF, says, helloWorld));
            g.Assert(new Triple(dotNetRDF, says, bonjourMonde));
            g.Assert(new Triple(dotNetRDF, says, helloWorld));
            g.Assert(new Triple(dotNetRDF, says, bonjourMonde));
            g.Assert(new Triple(dotNetRDF, says, helloWorld));
            g.Assert(new Triple(dotNetRDF, says, bonjourMonde));
            g.Assert(new Triple(dotNetRDF, says, helloWorld));
            g.Assert(new Triple(dotNetRDF, says, bonjourMonde));

            sw.Stop();
            Console.WriteLine("Execution time: " + sw.ElapsedMilliseconds + " ms");
        }

        private static void PerformanceTestWith100Triples()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            IGraph g = new Graph();

            IUriNode dotNetRDF = g.CreateUriNode(UriFactory.Create("http://www.dotnetrdf.org"));
            IUriNode says = g.CreateUriNode(UriFactory.Create("http://example.org/says"));
            ILiteralNode helloWorld = g.CreateLiteralNode("Hello World");

            for (int i = 0; i < 100; i++)
            {
                g.Assert(new Triple(dotNetRDF, says, helloWorld));
            }

            sw.Stop();
            Console.WriteLine("Execution time: " + sw.ElapsedMilliseconds + " ms");
        }

        private static void PerformanceTestWith1000Triples()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            IGraph g = new Graph();
            
            IUriNode dotNetRDF = g.CreateUriNode(UriFactory.Create("http://www.dotnetrdf.org"));
            IUriNode says = g.CreateUriNode(UriFactory.Create("http://example.org/says"));

            for (int i = 0; i < 1000; i++)
            {
                g.Assert(new Triple(dotNetRDF, says, g.CreateLiteralNode("Hello World"+i)));
            }

            sw.Stop();
            Console.WriteLine("Execution time: " + sw.ElapsedMilliseconds + " ms");
        }
        


        static void Main(string[] args)
        {

            //Console.WriteLine("PerformanceTestCreate1Triple");
            //PerformanceTestWith1Triple();

            //Console.WriteLine("PerformanceTestCreate2Triples");
            //PerformanceTestWith2Triples();

            //Console.WriteLine("PerformanceTestCreate10Triples");
            //PerformanceTestWith10Triples();

            //Console.WriteLine("PerformanceTestCreate100Triples");
            //PerformanceTestWith100Triples();

            //Console.WriteLine("PerformanceTestCreate1000Triples");
            //PerformanceTestWith1000Triples();

            //Console.WriteLine("PerformanceTestLoadDatabase:");
            //PerformanceTestWithDatabase();
          
            //WriteAnRDF();
            AQuery();
            Console.ReadKey();
        }
    }
}

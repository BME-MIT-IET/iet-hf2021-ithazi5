using System;
using System.Collections.Generic;
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
        private static object results;

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

            IBlankNode b = g.GetBlankNode("nodeID");
            if (b != null)
            {
                Console.WriteLine("Blank Node with ID " + b.InternalID + " exists in the Graph");
            }
            else
            {
                Console.WriteLine("No Blank Node with the given ID existed in the Graph");
            }

            TripleStore store = new TripleStore();
            store.Add(g);

            //Assume that we fill our Store with data from somewhere

            InMemoryDataset ds = new InMemoryDataset(store, true);

            //Get the Query processor
            ISparqlQueryProcessor processor = new LeviathanQueryProcessor(ds);

            //Use the SparqlQueryParser to give us a SparqlQuery object
            //Should get a Graph back from a CONSTRUCT query
            SparqlQueryParser sparqlparser = new SparqlQueryParser();
            SparqlQuery query = sparqlparser.ParseFromString("SELECT ?actor {}");
            results = processor.ProcessQuery(query);
            if (results is IGraph)
            {
                //Print out the Results
                IGraph r = (IGraph)results;
                NTriplesFormatter formatter = new NTriplesFormatter();
                foreach (Triple t in r.Triples)
                {
                    Console.WriteLine(t.ToString(formatter));
                }
                Console.WriteLine("Query took " + query.QueryTime + " milliseconds");
            }
        }

        static void Main(string[] args)
        {
            //WriteAnRDF();
            AQuery();
            Console.ReadKey();
        }
    }
}

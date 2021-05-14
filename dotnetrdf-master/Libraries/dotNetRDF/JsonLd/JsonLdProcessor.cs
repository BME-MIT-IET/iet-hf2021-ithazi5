/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2021 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.JsonLd.Processors;
using VDS.RDF.JsonLd.Syntax;

namespace VDS.RDF.JsonLd
{
    /// <summary>
    /// Implements the core JSON-LD processing. 
    /// </summary>
    public class JsonLdProcessor
    {
        private Uri _base;
        private readonly JsonLdProcessorOptions _options;

        /// <summary>
        /// Get or set the base IRI for processing.
        /// </summary>
        /// <remarks>This value should be set to the IRI of the document being processed if available.</remarks>
        public Uri BaseIri
        {
            get => _options.Base ?? _base;
            set => _base = value;
        }

        /// <summary>
        /// Get or set the current processing mode.
        /// </summary>
        public JsonLdProcessingMode? ProcessingMode { get; set; }

        /// <summary>
        /// Get the warnings generated by the processor.
        /// </summary>
        /// <remarks>May be an empty list.</remarks>
        public List<JsonLdProcessorWarning> Warnings { get; } = new List<JsonLdProcessorWarning>();

        /// <summary>
        /// Create a new processor instance.
        /// </summary>
        /// <param name="options">JSON-LD processing options.</param>
        private JsonLdProcessor(JsonLdProcessorOptions options)
        {
            _options = options ?? new JsonLdProcessorOptions();
            ProcessingMode = _options.ProcessingMode;
        }

        /// <summary>
        /// Run the Compaction algorithm.
        /// </summary>
        /// <param name="input">The JSON-LD data to be compacted. Expected to be a JObject or JArray of JObject or a JString whose value is the IRI reference to a JSON-LD document to be retrieved.</param>
        /// <param name="context">The context to use for the compaction process. May be a JObject, JArray of JObject, JString or JArray of JString. String values are treated as IRI references to context documents to be retrieved.</param>
        /// <param name="options">Additional processor options.</param>
        /// <returns></returns>
        public static JObject Compact(JToken input, JToken context, JsonLdProcessorOptions options)
        {
            var processor = new JsonLdProcessor(options);
            return processor.Compact(input, context);
        }

        /// <summary>
        /// Run the Compaction algorithm.
        /// </summary>
        /// <param name="input">The JSON-LD data to be compacted. Expected to be a JObject or JArray of JObject or a JString whose value is the IRI reference to a JSON-LD document to be retrieved.</param>
        /// <param name="context">The context to use for the compaction process. May be a JObject, JArray of JObject, JString or JArray of JString. String values are treated as IRI references to context documents to be retrieved.</param>
        /// <returns></returns>
        public JObject Compact(JToken input, JToken context) 
        { 
            // Set expanded input to the result of using the expand method using input and options.
            JArray expandedInput;
            Uri remoteDocumentUrl = null;
            Uri contextBase = null;
            if (input.Type == JTokenType.String)
            {
                var remoteDocument = LoadJson(new Uri(input.Value<string>()),
                    new JsonLdLoaderOptions {ExtractAllScripts = _options.ExtractAllScripts}, _options);
                expandedInput = Expand(remoteDocument, remoteDocument.DocumentUrl,
                    new JsonLdLoaderOptions {ExtractAllScripts = false}, _options);
                remoteDocumentUrl = contextBase = remoteDocument.DocumentUrl;
            }
            else
            {
                expandedInput = Expand(input, _options);
            }

            if (contextBase == null)
            {
                contextBase = _options.Base;
            }

            if (context is JObject contextObject && contextObject.ContainsKey("@context"))
            {
                context = contextObject["@context"];
            }

            var contextProcessor = new ContextProcessor(_options, Warnings);

            var activeContext = contextProcessor.ProcessContext(new JsonLdContext(), context, contextBase);
            if (activeContext.Base == null)
            {
                activeContext.Base = _options?.Base ?? (_options.CompactToRelative ? remoteDocumentUrl : null);
            }

            var compactor = new CompactProcessor(_options, contextProcessor, Warnings);
            var compactedOutput = compactor.CompactElement(activeContext, null, expandedInput, _options.CompactArrays,
                _options.Ordered);
            if (JsonLdUtils.IsEmptyArray(compactedOutput))
            {
                compactedOutput = new JObject();
            }
            else if (compactedOutput is JArray)
            {
                compactedOutput = new JObject(new JProperty(compactor.CompactIri(activeContext, "@graph", vocab: true),
                    compactedOutput));
            }

            if (context != null && !JsonLdUtils.IsEmptyObject(context))
            {
                (compactedOutput as JObject)["@context"] = context;
            }

            return compactedOutput as JObject;
        }


        /// <summary>
        /// Apply the JSON-LD context expansion algorithm to the context found at the specified URL.
        /// </summary>
        /// <param name="contextUrl">The URL to load the source context from.</param>
        /// <param name="options">Options to apply during the expansion processing.</param>
        /// <returns>The expanded JSON-LD context.</returns>
        public static JArray Expand(Uri contextUrl, JsonLdProcessorOptions options = null)
        {
            var parsedJson = LoadJson(contextUrl, null, options);
            var processor = new JsonLdProcessor(options);
            return processor.Expand(parsedJson, contextUrl, null, options);
        }

        /// <summary>
        /// Apply the JSON-LD expansion algorithm to a context JSON object.
        /// </summary>
        /// <param name="input">The context JSON object to be expanded.</param>
        /// <param name="options">Options to apply during the expansion processing.</param>
        /// <returns>The expanded JSON-LD context.</returns>
        public static JArray Expand(JToken input, JsonLdProcessorOptions options = null)
        {
            var remoteDoc = new RemoteDocument
            {
                Document = input is JValue v && v.Type == JTokenType.String ? v.Value<string>() : input,
            };
            var processor = new JsonLdProcessor(options);
            return processor.Expand(remoteDoc, null, null, options);
        }

        private JArray Expand(RemoteDocument doc, Uri documentLocation,
            JsonLdLoaderOptions loaderOptions = null,
            JsonLdProcessorOptions options = null)
        {
            if (doc.Document is string docContent)
            {
                try
                {
                    doc.Document = JToken.Parse(docContent);
                }
                catch (Exception ex)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.LoadingDocumentFailed,
                        "Loading document failed. Error parsing document content as JSON: " + ex.Message);
                }
            }

            if (documentLocation == null) documentLocation = doc.DocumentUrl;
            var contextBase = options?.Base ?? documentLocation;
            var activeContext = contextBase != null ? new JsonLdContext(contextBase) : new JsonLdContext();
            var contextProcessor = new ContextProcessor(options, Warnings);
            if (options?.ExpandContext != null)
            {
                if (options.ExpandContext is JObject expandObject)
                {
                    var contextProperty = expandObject.Property("@context");
                    activeContext = contextProcessor.ProcessContext(activeContext,
                        contextProperty != null ? contextProperty.Value : expandObject,
                        activeContext.OriginalBase);
                }
                else
                {
                    activeContext = contextProcessor.ProcessContext(activeContext, options.ExpandContext,
                        activeContext.OriginalBase);
                }
            }

            if (doc.ContextUrl != null)
            {
                var contextDoc = LoadJson(doc.ContextUrl, loaderOptions, options);
                if (contextDoc.Document is string contextJson)
                {
                    contextDoc.Document = JToken.Parse(contextJson);
                }

                activeContext = contextProcessor.ProcessContext(activeContext, contextDoc.Document as JToken, doc.ContextUrl);
            }
            
            var expander = new ExpandProcessor(options, contextProcessor, Warnings);
            var expandedOutput = expander.ExpandElement(activeContext, null, doc.Document as JToken,
                doc.DocumentUrl ?? options?.Base,
                options?.FrameExpansion ?? false,
                options?.Ordered ?? false);
            if (expandedOutput is JObject expandedObject)
            {
                if (expandedObject.ContainsKey("@graph") && expandedObject.Count == 1)
                    expandedOutput = expandedObject["@graph"];
            }

            if (expandedOutput == null) expandedOutput = new JArray();
            expandedOutput = JsonLdUtils.EnsureArray(expandedOutput);
            return expandedOutput as JArray;
        }

        /// <summary>
        /// Applies the framing algorithm to the specified input.
        /// </summary>
        /// <param name="input">The input to be framed.</param>
        /// <param name="frame">The framing specification document.</param>
        /// <param name="options">Processor options.</param>
        /// <returns></returns>
        public static JObject Frame(JToken input, JToken frame, JsonLdProcessorOptions options)
        {
            var processor = new JsonLdProcessor(options);
            return processor.Frame(input, frame);
        }

        private JObject Frame(JToken input, JToken frame) 
        {
            // JSON-LD 1.0 compatible framing requires ordered processing
            if (_options.ProcessingMode == JsonLdProcessingMode.JsonLd10)
            {
                _options.Ordered = true;
            }

            var nodeMapGenerator = new NodeMapGenerator();
            Uri remoteDocumentUri = null, remoteFrameUri = null;
            RemoteDocument remoteDocument = null, remoteFrame = null;
            var loaderOptions = new JsonLdLoaderOptions {ExtractAllScripts = _options.ExtractAllScripts};
            if (input.Type == JTokenType.String)
            {
                remoteDocumentUri = new Uri(input.Value<string>());
                remoteDocument = LoadJson(remoteDocumentUri, loaderOptions, _options);
            }

            var expandOptions = _options.Clone();
            expandOptions.Ordered = false;

            var expandedInput = remoteDocument != null
                ? Expand(remoteDocument, remoteDocumentUri, loaderOptions, expandOptions)
                : Expand(input, expandOptions);

            if (frame.Type == JTokenType.String)
            {
                remoteFrameUri = new Uri(frame.Value<string>());
                remoteFrame = LoadJson(remoteFrameUri, loaderOptions, _options);
            }

            var frameExpansionOptions = _options.Clone();
            frameExpansionOptions.Ordered = false;
            frameExpansionOptions.FrameExpansion = true;
            var expandedFrame = remoteFrame != null
                ? Expand(remoteFrame, remoteFrameUri, loaderOptions, frameExpansionOptions)
                : Expand(frame, frameExpansionOptions);

            JToken context = new JObject();
            var haveContext = false;
            if (remoteFrame != null && remoteFrame.Document is JObject remoteFrameObject &&
                remoteFrameObject.ContainsKey("@context"))
            {
                context = remoteFrameObject["@context"] as JObject;
                haveContext = true;
            }
            else if (frame is JObject fo && fo.ContainsKey("@context"))
            {
                context = fo["@context"];
                haveContext = true;
            }

            var contextBase = _options.Base;
            if (remoteFrame?.DocumentUrl != null) contextBase = remoteFrame.DocumentUrl;

            var contextProcessor = new ContextProcessor(_options, Warnings);

            // 10 - Initialize active context to the result of the Context Processing algorithm passing a new empty context as active context context as local context, and context base as base URL.
            var activeContext = contextProcessor.ProcessContext(new JsonLdContext(), context, contextBase);

            // 11 - Initialize an active context using context; the base IRI is set to the base option from options, if set; otherwise, if the compactToRelative option is true, to the IRI of the currently being processed document, if available; otherwise to null.
            // KA - Spec is a bit unclear here. I assume that it means that this step creates a separate active context potentially with a different base IRI for the reverse context creation in step 12
            var reverseContextBase = _options.Base ??
                                     (_options.CompactToRelative && remoteDocument != null
                                         ? remoteDocument.DocumentUrl
                                         : null);
            var toReverse = contextProcessor.ProcessContext(new JsonLdContext(), context, reverseContextBase);

            // 12 - Initialize inverse context to the result of performing the Inverse Context Creation algorithm.
            var inverseContext = toReverse.InverseContext;

            // 13 - If frame has a top-level property which expands to @graph set the frameDefault option to options with the value true.
            if (frame is JObject frameObject && frameObject.Properties()
                .Any(p => contextProcessor.ExpandIri(activeContext, p.Name, true).Equals("@graph")))
            {
                _options.FrameDefault = true;
            }

            // 14 - Initialize a new framing state (state) to an empty map. 
            var graphMap = nodeMapGenerator.GenerateNodeMap(expandedInput);
            if (!_options.FrameDefault)
            {
                // Add an @merged entry to graphMap
                var mergedNodeMap = nodeMapGenerator.GenerateMergedNodeMap(graphMap);
                graphMap["@merged"] = mergedNodeMap;
            }

            var state = new FramingState(_options, graphMap, _options.FrameDefault ? "@default" : "@merged");

            // 15 - Initialize results as an empty array.
            var results = new JArray();
            // 16 - Invoke the Framing algorithm, passing state, the keys from subject map in state for subjects, expanded frame, results for parent, and null as active property.
            FramingProcessor.ProcessFrame(state, state.Subjects.Properties().Select(p => p.Name).ToList(), expandedFrame, results, null,
                processingMode: _options.ProcessingMode ?? JsonLdProcessingMode.JsonLd11);

            // 17 - If the processing mode is not json-ld-1.0, remove the @id entry of each node object in results where the entry value is a blank node identifier which appears only once in any property value within results.
            if (_options.ProcessingMode != JsonLdProcessingMode.JsonLd10)
            {
                PruneBlankNodeIdentifiers(results);
            }

            // 18 - Recursively, replace all entries in results where the key is @preserve with the first value of that entry.
            ReplacePreservedValues(results, activeContext, false);

            // 19 - Set compacted results to the result of using the compact method using active context, inverse context, null for active property, results as element,, and the compactArrays and ordered flags from options.
            var compactor = new CompactProcessor(_options, contextProcessor, Warnings);
            var compactedResults =
                compactor.CompactElement(activeContext, null, results, _options.CompactArrays, _options.Ordered);
            var graphProperty = compactor.CompactIri(activeContext, "@graph", vocab: true);
            // 19.1 - If compacted results is an empty array, replace it with a new map.
            if (JsonLdUtils.IsEmptyArray(compactedResults))
            {
                compactedResults = new JObject();
            }
            else if (JsonLdUtils.IsArray(compactedResults))
            {
                // 19.2 - Otherwise, if compacted results is an array, replace it with a new map with a single entry whose key is the result of IRI compacting @graph and value is compacted results.
                compactedResults = new JObject(new JProperty(graphProperty, compactedResults));
            }

            var compactedResultsObject = compactedResults as JObject;
            // 19.3 - Add an @context entry to compacted results and set its value to the provided context.
            if (haveContext
            ) // Only if it was explicitly provided, not if context was created as part of this algorithm.
            {
                compactedResultsObject.Add("@context", context);
            }

            // 20 - Recursively, replace all @null values in compacted results with null. If, after replacement, an array contains only the value null remove that value, leaving an empty array.
            ReplaceNulls(compactedResultsObject);

            // 21 - If omitGraph is false and compacted results does not have a top-level @graph entry, or its value is not an array,
            // modify compacted results to place the non @context entry of compacted results into a map contained within the array value of @graph.
            // If omitGraph is true, a top-level @graph entry is used only to contain multiple node objects.
            if (!_options.OmitGraph && (!compactedResultsObject.ContainsKey(graphProperty) ||
                                       compactedResultsObject[graphProperty].Type != JTokenType.Array))
            {
                var g = new JObject();
                foreach (var property in compactedResultsObject.Properties().ToList())
                {
                    if (!property.Name.Equals("@context") && !property.Name.Equals(graphProperty))
                    {
                        g.Add(property);
                        compactedResultsObject.Remove(property.Name);
                    }
                }

                if (compactedResultsObject.ContainsKey(graphProperty))
                {
                    compactedResultsObject[graphProperty] = new JArray(g, compactedResultsObject[graphProperty]);
                }
                else
                {
                    if (g.Count > 0)
                    {
                        compactedResultsObject[graphProperty] = new JArray(g);
                    }
                    else
                    {
                        compactedResultsObject[graphProperty] = new JArray();
                    }
                }
            }

            return compactedResultsObject;
        }

        /// <summary>
        /// Flattens the given input and compacts it using the passed context according to the steps in the JSON-LD Flattening algorithm.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static JToken Flatten(JToken input, JToken context, JsonLdProcessorOptions options)
        {
            // Set expanded input to the result of using the expand method using input and options.
            var expandedInput = Expand(input, options);
            var flattenProcessor = new FlattenProcessor();
            var flattenedOutput = flattenProcessor.FlattenElement(expandedInput, options.Ordered);
            if (context != null)
            {
                flattenedOutput = Compact(flattenedOutput, context, options);
            }

            return flattenedOutput;
        }

        /// <summary>
        /// Flattens the given input and compacts it using the passed context according to the steps in the JSON-LD Flattening algorithm.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public JToken Flatten(JToken input, JToken context)
        {
            return Flatten(input, context, _options);
        }

        private static void ReplaceNulls(JToken token)
        {
            switch (token)
            {
                case JObject o:
                    foreach (var property in o.Properties())
                    {
                        switch (property.Value.Type)
                        {
                            case JTokenType.String:
                                if ("@null".Equals(property.Value.Value<string>()))
                                {
                                    property.Value = null;
                                }

                                break;
                            case JTokenType.Array:
                            case JTokenType.Object:
                                ReplaceNulls(property.Value);
                                break;
                        }
                    }

                    break;
                case JArray a:
                    for (var ix = 0; ix < a.Count; ix++)
                    {
                        var item = a[ix];
                        switch (item.Type)
                        {
                            case JTokenType.String:
                                if ("@null".Equals(item.Value<string>()))
                                {
                                    a[ix] = null;
                                }

                                break;
                            case JTokenType.Object:
                            case JTokenType.Array:
                                ReplaceNulls(item);
                                break;
                        }
                    }

                    if (a.All(x => x.Type == JTokenType.Null))
                    {
                        a.RemoveAll();
                    }

                    break;
            }
        }

        private static void ReplacePreservedValues(JToken token, JsonLdContext context, bool compactArrays)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    var o = token as JObject;
                    if (o["@preserve"] != null)
                    {
                        var parent = o.Parent;
                        var preserveValue = o["@preserve"];
                        if (preserveValue.Type == JTokenType.String && preserveValue.Value<string>().Equals("@null"))
                        {
                            if (parent is JArray)
                            {
                                o.Remove();
                            }
                            else
                            {
                                o.Replace(JValue.CreateNull());
                            }
                        }
                        else
                        {
                            o.Replace(preserveValue);
                        }
                    }

                    foreach (var p in o)
                    {
                        ReplacePreservedValues(p.Value, context, compactArrays);
                    }

                    break;
                case JTokenType.Array:
                    var a = token as JArray;
                    foreach (var item in a.ToList())
                    {
                        ReplacePreservedValues(item, context, compactArrays);
                    }

                    if (compactArrays && a.Count == 1)
                    {
                        if (a.Parent is JProperty parentProperty)
                        {
                            var termDefinition = context.GetTerm(parentProperty.Name);
                            var expandedName = termDefinition?.TypeMapping ?? parentProperty.Name;
                            if (expandedName != "@graph" &&
                                expandedName != "@list" &&
                                (termDefinition == null || termDefinition.ContainerMapping == null))
                            {
                                a.Replace(a[0]);
                            }
                        }
                    }

                    break;
            }
        }

        private static void PruneBlankNodeIdentifiers(JToken token)
        {
            var objectMap = new Dictionary<string, BlankNodeMapEntry>();
            GenerateBlankNodeMap(objectMap, token, null);
            foreach (var mapEntry in objectMap)
            {
                if (!mapEntry.Value.IsReferenced)
                {
                    PruneBlankNodeIdentifier(mapEntry.Key, mapEntry.Value.IdProperty);
                }
            }
        }

        private static void PruneBlankNodeIdentifier(string id, JProperty toUpdate)
        {
            if (toUpdate.Value.Type == JTokenType.String)
            {
                toUpdate.Remove();
            }
            else if (toUpdate.Value is JArray valueArray)
            {
                foreach (var item in valueArray)
                {
                    if (item.Value<string>().Equals(id))
                    {
                        item.Remove();
                        break;
                    }
                }
            }
        }

        private class BlankNodeMapEntry
        {
            public bool IsReferenced;
            public JProperty IdProperty;
        }

        private static void GenerateBlankNodeMap(IDictionary<string, BlankNodeMapEntry> objectMap, JToken token,
            JProperty activeProperty)
        {
            switch (token.Type)
            {
                case JTokenType.String:
                    var str = token.Value<string>();
                    if (JsonLdUtils.IsBlankNodeIdentifier(str))
                    {
                        if (!objectMap.TryGetValue(str, out var mapEntry))
                        {
                            mapEntry = new BlankNodeMapEntry();
                            objectMap[str] = mapEntry;
                        }

                        if (activeProperty.Name == "@id" && mapEntry.IdProperty == null)
                        {
                            mapEntry.IdProperty = activeProperty;
                        }
                        else
                        {
                            mapEntry.IsReferenced = true;
                        }
                    }

                    break;
                case JTokenType.Array:
                    foreach (var item in (token as JArray))
                    {
                        GenerateBlankNodeMap(objectMap, item, activeProperty);
                    }

                    break;
                case JTokenType.Object:
                    foreach (var p in (token as JObject).Properties())
                    {
                        if (p.Name == "@value") continue;
                        GenerateBlankNodeMap(objectMap, p.Value, p);
                    }

                    break;
            }
        }

        private static RemoteDocument LoadJson(Uri remoteRef, JsonLdLoaderOptions loaderOptions,
            JsonLdProcessorOptions options)
        {
            return options.DocumentLoader != null
                ? options.DocumentLoader(remoteRef, loaderOptions)
                : DefaultDocumentLoader.LoadJson(remoteRef, loaderOptions);
        }
    }
}

﻿
//load sk
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.Orchestration;
using SKIngest;
using System.Reflection;
using Microsoft.SemanticKernel.CoreSkills;

var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.development.json", optional: true, reloadOnChange: true)
            .Build();

var embeddingConfig = configBuilder.GetRequiredSection("EmbeddingConfig").Get<Config>();
var completionConfig = configBuilder.GetRequiredSection("CompletionConfig").Get<Config>();

var sk = Kernel.Builder.Configure(embeddingConfig, completionConfig);

//build pipeline
var dataImporter = new DataImporter(sk);

//CSV data source supports 1-n data files in the given folder
// var folder = "";
// var csvDataSource = new CsvDataSource(folder);
// dataImporter.AddDataSource(csvDataSource);

//HTML data source supports 1-n URIs
var uris = new[] 
{ 
    "https://www.ecfr.gov/current/title-26/chapter-I/subchapter-A/part-1/subject-group-ECFR504ddca54174c57/section-1.1-1" 
};

foreach (var uri in uris)
{
    dataImporter.AddDataSource(new HtmlDataSource(uri));
}

//add transforms
var chunkingTransform = new ChunkingTransform();
var summaryTransform = new SummaryTransform(sk);

//first summarise
dataImporter.AddTransform(summaryTransform);

//then we'll chunk just incase (should not do anything as the summary should be smaller than the token limit)
dataImporter.AddTransform(chunkingTransform);

//run pipeline
var destinationMemoryCollection = "mycollection";
await dataImporter.ProcessAsync(destinationMemoryCollection);

//import skills so that the data ingested can be queried
sk.ImportSkill(new TextMemorySkill(), nameof(TextMemorySkill));

var query = "If I was unmarried in 1964 and made $15750, how much tax would I owe?";

sk.CreateSemanticFunction(Assembly.GetEntryAssembly().LoadEmbeddedResource("sk_ingest.Skills.Query.skprompt.txt"),
    "Query",
    "IngestionSkill",
    maxTokens: 2048);

var contextVariables = new ContextVariables(query);
contextVariables.Set("collection", destinationMemoryCollection);

var result = await sk.RunAsync(contextVariables, sk.Skills.GetFunction("IngestionSkill", "Query"));

if (result.ErrorOccurred)
{
    Console.WriteLine($"[ERROR] {result.LastErrorDescription}");
    return;
}

Console.WriteLine(result.Result);
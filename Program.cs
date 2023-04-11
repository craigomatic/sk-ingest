
//load sk
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.Orchestration;
using SKIngest;

var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.development.json", optional: true, reloadOnChange: true)
            .Build();

var embeddingConfig = configBuilder.GetRequiredSection("EmbeddingConfig").Get<Config>();
var textCompletionConfig = configBuilder.GetRequiredSection("TextCompletionConfig").Get<Config>();

var sk = Kernel.Builder.
    Configure(c =>
    {
        if (embeddingConfig != null)
        {
            c.ConfigureEmbeddings(embeddingConfig);
        }

        if (textCompletionConfig != null)
        {
            c.ConfigureTextCompletion(textCompletionConfig);
        }
    }).
    WithMemoryStorage(new VolatileMemoryStore()).
    Build();

var prompt = "Evaluate the given input against the material referenced. If you don't know the answer, explain why. If you do know the answer, please cite the relevant context that gave you the answer. The input is: {{$INPUT}}. The context is: {{$CONTEXT}}";

//build pipeline
var dataImporter = new DataImporter(sk);

//add data sources

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
dataImporter.AddTransform(chunkingTransform);

//run pipeline
var destinationMemoryCollection = "mycollection";
await dataImporter.ProcessAsync(destinationMemoryCollection);

var query = "Which tax year does this document apply to?";
var memoryQueryResults = sk.Memory.SearchAsync(destinationMemoryCollection, query, 5, 0.7);
var memoryQueryResult = await memoryQueryResults.FirstOrDefaultAsync();

var cv = new ContextVariables(prompt);

if (memoryQueryResult != null)
{
    cv.Set("Context", memoryQueryResult.Text);
}

//var result = sk.RunAsync(cv);

//load sk
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.Extensions.Configuration;

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

//build pipeline
var dataIngestionPipeline = new DataImporter(sk);

//add data source
var folder = "";
var csvDataSource = new CsvDataSource(folder);
dataIngestionPipeline.AddDataSource(csvDataSource);

//add transforms
var chunkingTransform = new ChunkingTransform();
dataIngestionPipeline.AddTransform(chunkingTransform);

//run pipeline
var destinationMemoryCollection = "mycollection";
await dataIngestionPipeline.ProcessAsync(destinationMemoryCollection);
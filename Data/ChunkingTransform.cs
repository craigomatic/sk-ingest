using BlingFire;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.SemanticKernel.Text;
using System.Text.Json;

/// <summary>
/// Takes an input resource and chunks it down to fit the given max length
/// </summary>
public class ChunkingTransform : ITransform
{
    public int MaxSize { get; private set; }

    public ISKFunction ChunkingFunction { get; private set; }

    public string EmbeddingModel { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="maxSize"></param>
    /// <param name="useModelAlgo">When true, uses the completion model to help determine the correct way to chunk the given document</param>
    public ChunkingTransform(int maxSize = 2048, ISKFunction chunkingFunction = null, string embeddingModel = "text-embedding-ada-002")
    {
        this.MaxSize = maxSize;
        this.ChunkingFunction = chunkingFunction;
        this.EmbeddingModel = embeddingModel;
    }

    private const int BUFFER = 300; 

    public async Task<IEnumerable<Resource>> Run(Resource input)
    {
        var textResource = input as TextResource;

        if (textResource == null)
        {
            throw new Exception("Only TextResources are currently supported.");
        }

        var toReturn = new List<Resource>();

        if (textResource.Value.Length > this.MaxSize)
        {
            if (this.ChunkingFunction != null)
            {
                var contextVariables = new ContextVariables(textResource.Value);
                contextVariables.Set("embeddingModel", this.EmbeddingModel);
                contextVariables.Set("tokenWindow", this.MaxSize.ToString());
                
                if (textResource.Value.Length > this.ChunkingFunction.RequestSettings.MaxTokens - BUFFER) //300 token buffer for the chunking function prompt itself
                {
                    //trim it down early as it's too big for the chunking function to operate on
                    var lines = BlingFireUtils.GetSentences(textResource.Value);
                    var paragraphs = TextChunker.SplitPlainTextParagraphs(lines.ToList(), this.ChunkingFunction.RequestSettings.MaxTokens - BUFFER);

                    foreach (var paragraph in paragraphs)
                    {
                        contextVariables.Update(paragraph);
                        
                        var context = new SKContext(contextVariables);
                        
                        var result = await this.ChunkingFunction.InvokeAsync(context);
                        var chunkingParams = JsonSerializer.Deserialize<IEnumerable<ChunkingParams>>(result.Result);

                        foreach (var chunkingParam in chunkingParams)
                        {
                            toReturn.Add(new TextResource 
                            { 
                                ContentType = "text/plain",
                                Id = Guid.NewGuid().ToString(),
                                Value = paragraph.Substring(chunkingParam.StartChunk, chunkingParam.EndChunk) 
                            });
                        }
                    }
                }                
            }
            else
            {
                List<string> paragraphs;

                switch (textResource.ContentType)
                {
                    case "text/markdown":
                        {
                            var lines = TextChunker.SplitMarkDownLines(textResource.Value, this.MaxSize);
                            paragraphs = TextChunker.SplitMarkdownParagraphs(lines, this.MaxSize);

                            break;
                        }
                    default:
                        {
                            var lines = BlingFireUtils.GetSentences(textResource.Value);
                            paragraphs = TextChunker.SplitPlainTextParagraphs(lines.ToList(), this.MaxSize);

                            break;
                        }
                }

                for (int i = 0; i < paragraphs.Count; i++)
                {
                    toReturn.Add(new TextResource
                    {
                        ContentType = textResource.ContentType,
                        Id = $"{textResource.Id}_{i}",
                        Value = paragraphs[i]
                    });
                }
            }                       
        }
        else
        {
            toReturn.Add(input);
        }

        return toReturn.AsEnumerable();
    }
}
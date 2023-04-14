using Microsoft.SemanticKernel.SemanticFunctions.Partitioning;

/// <summary>
/// Takes an input resource and chunks it down to fit the given max length
/// </summary>
public class ChunkingTransform : ITransform
{
    public int MaxSize { get; private set; }

    public ChunkingTransform(int maxSize = 2048)
    {
        this.MaxSize = maxSize;
    }

    public Task<IEnumerable<Resource>> Run(Resource input)
    {
        var textResource = input as TextResource;

        if (textResource == null)
        {
            throw new Exception("Only TextResources are currently supported.");
        }

        var toReturn = new List<Resource>();

        if (textResource.Value.Length > this.MaxSize)
        {
            List<string> lines;
            List<string> paragraphs;

            switch (textResource.ContentType)
            {
                case "text/markdown":
                {
                    lines = SemanticTextPartitioner.SplitMarkDownLines(textResource.Value, this.MaxSize);
                    paragraphs = SemanticTextPartitioner.SplitMarkdownParagraphs(lines, this.MaxSize);

                    break;
                }
                default:
                {
                    lines = SemanticTextPartitioner.SplitPlainTextLines(textResource.Value, this.MaxSize);
                    paragraphs = SemanticTextPartitioner.SplitPlainTextParagraphs(lines, this.MaxSize);

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
        else
        {
            toReturn.Add(input);
        }

        return Task.FromResult(toReturn.AsEnumerable());
    }
}
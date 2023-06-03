using System.Reflection;
using System.Text;
using BlingFire;
using Microsoft.SemanticKernel;

/// <summary>
/// Takes an input resource and chunks it down to fit the given max length
/// </summary>
public class SummaryTransform : ITransform
{
    private readonly IKernel _Kernel;
    
    public int MaxSize { get; private set; }

    public SummaryTransform(IKernel sk, int maxSize = 2048)
    {
        _Kernel = sk;

        this.MaxSize = maxSize;

        sk.CreateSemanticFunction(Assembly.GetEntryAssembly().LoadEmbeddedResource("sk_ingest.Skills.Summary.skprompt.txt"),
            "Summarise",
            "Summary",
            maxTokens: 2048);
    }
    
    public async Task<IEnumerable<Resource>> Run(Resource input)
    {
        var textResource = input as TextResource;

        if (textResource == null)
        {
            throw new Exception("Only TextResources are currently supported.");
        }

        var toSummarise = new List<string>();
        var allsentences = BlingFireUtils.GetSentences(textResource.Value);

        var sb = new StringBuilder();

        foreach (var sentence in allsentences)
        {
            if (sb.Length + sentence.Length < this.MaxSize)
            {
                sb.Append(sentence);
            }
            else
            {
                toSummarise.Add(sb.ToString());
                
                sb = new StringBuilder();
            }
        }

        var toReturn = new List<Resource>();

        foreach (var item in toSummarise)
        {
            var result = await _Kernel.RunAsync(item, _Kernel.Skills.GetFunction("Summary", "Summarise"));
            
            toReturn.Add(new TextResource
            {
                ContentType = textResource.ContentType,
                Id = $"{textResource.Id}_{Guid.NewGuid()}",
                Value = result.Result
            });        
        }

        return toReturn.AsEnumerable();
    }
}
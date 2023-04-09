using Microsoft.SemanticKernel;

public class DataImporter
{
    private List<ITransform> _Transforms = new();

    private List<IDataSource> _Datasources = new();

    private readonly IKernel _SemanticKernel;

    public DataImporter(IKernel sk)
    {
        _SemanticKernel = sk;
    }

    public void AddDataSource(IDataSource dataSource)
    {
        _Datasources.Add(dataSource);
    }

    public void AddTransform(ITransform transform)
    {
        _Transforms.Add(transform);
    }

    public async Task ProcessAsync(string destinationCollection)
    {
        if(!_Datasources.Any())
        {
            throw new Exception("Must have at least one data source defined before invoking run");
        }

        foreach (var ds in _Datasources)
        {            
            var resources = await ds.Load();
            var inputResources = resources.OfType<TextResource>();            
            var processedResources = new List<TextResource>();

            if(!_Transforms.Any())
            {
                processedResources = inputResources as List<TextResource>;
            }
            else
            {
                foreach (var resource in inputResources)
                {
                    var state = await _RunTransforms(resource);
                    var pending = state.Pending;
                    
                    processedResources.AddRange(state.Completed.Cast<TextResource>());

                    while (pending.Count > 0)
                    {
                        var result = await _RunTransforms(pending.Dequeue());
                        
                        foreach (var item in result.Pending)
                        {
                            pending.Enqueue(item);
                        }
                    }
                }
            }

            foreach (var resource in processedResources)
            {
                //once all transforms are complete, generate embeddings and store
                await _SemanticKernel.Memory.SaveInformationAsync(destinationCollection, resource.Value, resource.Id);
            }
        }
    }

    private async Task<TransformState> _RunTransforms(Resource resource)
    {
        var state = new TransformState();

        foreach (var tf in _Transforms)
        {
            //each transform operation could result in multiple outputs
            //each output needs to be independently processed against all transforms before they are considered processed
            var results = await tf.Run(resource);

            foreach (var result in results)
            {
                //don't add the input item into the reprocess queue (comparison by id)
                if (result.Id == resource.Id)
                {
                    state.Completed.Add(result as TextResource);
                }
                else
                {
                    state.Pending.Enqueue(result as TextResource);    
                }
            }
        }
        
        return state;
    }

    private class TransformState
    {
        public List<Resource> Completed { get; set; } = new();

        public Queue<Resource> Pending { get; set; } = new();
    }
}
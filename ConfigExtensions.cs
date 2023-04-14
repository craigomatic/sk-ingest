using Microsoft.SemanticKernel;

public static class ConfigExtensions
{
    public static void ConfigureTextCompletion(this KernelConfig kernelConfig, Config config)
    {
        switch(config.AIService)
        {
            case Config.AzureOpenAI:
                {
                    kernelConfig.AddAzureOpenAITextCompletionService(
                        config.AIService,
                        config.DeploymentOrModelId,
                        config.Endpoint,
                        config.Key);
                    break;
                }
            case Config.OpenAI:
                {
                    kernelConfig.AddOpenAITextCompletionService(
                        config.AIService,
                        config.DeploymentOrModelId,
                        config.Key);
                    break;
                }
        }        
    }

    public static void ConfigureEmbeddings(this KernelConfig kernelConfig, Config config)
    {
        switch(config.AIService)
        {
            case Config.AzureOpenAI:
                {
                    kernelConfig.AddAzureOpenAIEmbeddingGenerationService(
                        config.AIService,
                        config.DeploymentOrModelId,
                        config.Endpoint,
                        config.Key);
                    break;
                }
            case Config.OpenAI:
                {
                    kernelConfig.AddOpenAIEmbeddingGenerationService(
                        config.AIService,
                        config.DeploymentOrModelId,
                        config.Key);
                    break;
                }
        }
    }

}

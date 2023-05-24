using Microsoft.SemanticKernel;

public static class ConfigExtensions
{
    public static void ConfigureCompletion(this KernelConfig kernelConfig, Config config)
    {
        switch(config.AIService)
        {
            case Config.AzureOpenAI:
                {
                    kernelConfig.AddAzureChatCompletionService(
                        config.DeploymentOrModelId,
                        config.Endpoint,
                        config.Key);
                    break;
                }
            case Config.OpenAI:
                {
                    kernelConfig.AddOpenAITextCompletionService(
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
                    kernelConfig.AddAzureTextEmbeddingGenerationService(
                        config.DeploymentOrModelId,
                        config.Endpoint,
                        config.Key);
                    break;
                }
            case Config.OpenAI:
                {
                    kernelConfig.AddOpenAITextEmbeddingGenerationService(
                        config.DeploymentOrModelId,
                        config.Key);
                    break;
                }
        }
    }

}

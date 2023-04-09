using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Configuration;

public static class ConfigExtensions
{
    public static void ConfigureTextCompletion(this KernelConfig kernelConfig, Config config)
    {
        switch(config.AIService)
        {
            case Config.AzureOpenAI:
                {
                    kernelConfig.AddAzureOpenAITextCompletion(
                        config.AIService,
                        config.DeploymentOrModelId,
                        config.Endpoint,
                        config.Key);
                    break;
                }
            case Config.OpenAI:
                {
                    kernelConfig.AddOpenAITextCompletion(
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
                    kernelConfig.AddAzureOpenAIEmbeddingGeneration(
                        config.AIService,
                        config.DeploymentOrModelId,
                        config.Endpoint,
                        config.Key);
                    break;
                }
            case Config.OpenAI:
                {
                    kernelConfig.AddOpenAIEmbeddingGeneration(
                        config.AIService,
                        config.DeploymentOrModelId,
                        config.Key);
                    break;
                }
        }
    }

}

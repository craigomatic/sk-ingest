public interface ITransform
{
    /// <summary>
    /// Runs the transform operation over the given resource.
    /// </summary>
    /// <param name="input">The resource to transform</param>
    /// <returns>One or more transformed resources that are the result of this transformation.</returns>
    Task<IEnumerable<Resource>> Run(Resource input);
}
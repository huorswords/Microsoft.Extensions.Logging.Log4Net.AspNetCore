namespace Microsoft.Extensions.Logging.Scope
{
    public interface ILog4NetScopeFactory
    {
        Log4NetScope BeginScope<TScope>(TScope scope);
    }
}
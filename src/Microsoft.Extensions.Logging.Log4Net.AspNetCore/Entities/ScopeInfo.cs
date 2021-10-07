using System.Collections.Generic;

namespace Microsoft.Extensions.Logging.Log4Net.AspNetCore.Entities
{
    public class ScopeInfo
    {
        public ScopeInfo()
        {
        }
        public string Text { get; set; }
        
        public Dictionary<string, object> Properties { get; set; }
    }
}

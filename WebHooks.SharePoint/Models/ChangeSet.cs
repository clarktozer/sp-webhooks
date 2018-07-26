using Microsoft.SharePoint.Client;
using System.Collections.Generic;

namespace WebHooks.SharePoint.Models
{
    public class ChangeSet
    {
        public Web Web { get; set; }
        public List List { get; set; }
        public List<Change> Changes { get; set; }
    }
}

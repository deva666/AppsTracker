using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using AppsTracker.Common.Utils;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;

namespace AppsTracker.Tracking.Helpers
{
    [Export(typeof(IUsageProcessor))]
    internal sealed class UsageProcessor : IUsageProcessor
    {
        private readonly IDataService dataService;
        private readonly IDictionary<UsageTypes, Usage> usageTypesMap = new Dictionary<UsageTypes, Usage>();

        [ImportingConstructor]
        public UsageProcessor(IDataService dataService)
        {
            this.dataService = dataService;
        }

        public void NewUsage(UsageTypes usageType, int userId, int parentUsageId)
        {
            Ensure.Condition<InvalidOperationException>(usageTypesMap.ContainsKey(usageType) == false, "Usage type exists");
            var usage = new Usage(userId, usageType) { SelfUsageID = parentUsageId};
            usageTypesMap.Add(usageType, usage);
        }

        public void UsageEnded(UsageTypes usageType)
        {
            if (usageTypesMap.ContainsKey(usageType) == false)
                return;

            var usage = usageTypesMap[usageType];
            SaveUsage(usageType, usage);
        }

        private void SaveUsage(UsageTypes usageType, Usage usage)
        {
            usage.UsageEnd = DateTime.Now;
            if (usage.UsageType == UsageTypes.Login)
                usage.IsCurrent = false;
            usageTypesMap.Remove(usageType);
            dataService.SaveNewEntity(usage);
        }


        public void EndAllUsages()
        {
            var usagesCopy = usageTypesMap.Values.ToList();
            foreach (var usage in usagesCopy)
            {
                SaveUsage(usage.UsageType, usage);
            }
        }

        public void RegisterUsage(Usage usage)
        {
            Ensure.NotNull(usage, "usage");
            Ensure.Condition<InvalidOperationException>(usageTypesMap.ContainsKey(usage.UsageType) == false, "Usage type exists");
            usageTypesMap.Add(usage.UsageType, usage);
        }
    }
}

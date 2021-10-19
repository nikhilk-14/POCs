using System.Collections.Generic;

namespace Prebatching_Automation
{
    public class HttpTriggerInputModel
    {
        public int RetailerId { get; set; }
        public string Environment { get; set; }
    }

    public class AppInsightsResponseModel
    {
        public List<AppInsightsResult> tables { get; set; }
    }

    public class AppInsightsResult
    {
        public string name { get; set; }
        public List<Column> columns { get; set; }
        public List<string[]> rows { get; set; }
    }

    public class Column
    {
        public string name { get; set; }
        public string type { get; set; }
    }

    public class AppInsightsItem
    {
        public string name { get; set; }
        public string value { get; set; }
        public string type { get; set; }
    }

    public class AppInsightsData
    {
        public List<List<AppInsightsItem>> data { get; set; }
    }
}

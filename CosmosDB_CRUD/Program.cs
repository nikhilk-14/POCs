using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace CosmosDB_CRUD
{
    class Program
    {
        private CosmosClient cosmosClient;
        private Database database;
        private Container container;

        static async Task Main(string[] args)
        {
            var p = new Program();
            p.SetCosmosConnecttion();
            await p.ProcessJsonFiles();
            Console.WriteLine("Press Any Key To Exit...");
            Console.ReadKey();
        }

        private async Task<List<JObject>> QueryItemsAsync(string sqlQueryText)
        {
            var result = new List<JObject>();
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            var queryResultSetIterator = this.container.GetItemQueryIterator<JObject>(queryDefinition);

            while (queryResultSetIterator.HasMoreResults)
            {
                var record = await queryResultSetIterator.ReadNextAsync();
                foreach (var item in record)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        private async Task CreateItemAsync<T>(T item)
        {
            await this.container.CreateItemAsync<T>(item);
        }

        private async Task ReplaceItemAsync<T>(T item, string id)
        {
            await this.container.ReplaceItemAsync<T>(item, id);
        }

        private async Task InsertOrUpdateItemAsync<T>(T item, string id)
        {
            await this.container.UpsertItemAsync<T>(item);
        }

        private async Task DeleteItemAsync<T>(string id, double partitionKeyValue)
        {
            await this.container.DeleteItemAsync<T>(id, new PartitionKey(partitionKeyValue));
        }

        private void SetCosmosConnecttion()
        {
            this.cosmosClient = new CosmosClient(ConfigurationManager.AppSettings.Get("ConnectionString"));
            this.database = this.cosmosClient.GetDatabase(ConfigurationManager.AppSettings.Get("DatabaseName"));
            this.container = this.database.GetContainer(ConfigurationManager.AppSettings.Get("ContainerName"));
        }

        private async Task ProcessJsonFiles()
        {
            string[] jsonFiles = Directory.GetFiles(ConfigurationManager.AppSettings.Get("ConfigFolderPath"), "*.json", SearchOption.TopDirectoryOnly);
            Console.WriteLine($"Found {jsonFiles.Length} JSON Config files: \n{string.Join("\n", jsonFiles)}");
            Console.WriteLine("----------------");

            foreach (var jsonFile in jsonFiles)
            {
                Console.WriteLine($"Processing JSON Confg File {jsonFile}");
                try
                {
                    var jsonConfig = JObject.Parse(File.ReadAllText(jsonFile));

                    var docType = jsonConfig["DocType"];
                    var appType = jsonConfig["AppType"];

                    var sqlQueryText = $"SELECT * FROM c WHERE c.DocType = {docType} and c.AppType = '{appType}'";
                    Console.WriteLine($"Query: {sqlQueryText}");

                    var existingRecords = await QueryItemsAsync(sqlQueryText);
                    if (existingRecords != null && existingRecords.Any())
                    {
                        // Update existing item
                        Console.WriteLine($"Existing config found");
                        var item = existingRecords.FirstOrDefault();

                        // take backup
                        Console.WriteLine($"Taking backup of existing config");
                        var backupFolderPath = $"{ConfigurationManager.AppSettings.Get("BackupFolderPath")}";

                        if (!Directory.Exists(backupFolderPath))
                        {
                            Directory.CreateDirectory(backupFolderPath);
                        }

                        var filePath = $"{backupFolderPath}{appType}_{docType}.json";
                        File.WriteAllText(filePath, JsonConvert.SerializeObject(item));
                        Console.WriteLine($"Existing config backup created at {filePath}");

                        var id = item["id"].ToString();
                        jsonConfig.Add("id", id);
                        await ReplaceItemAsync(jsonConfig, id);
                        Console.WriteLine($"Config {appType}_{docType} updated successfully");
                    }
                    else
                    {
                        // Create new item
                        Console.WriteLine($"Existing config not present");
                        jsonConfig.Add("id", Guid.NewGuid().ToString());
                        await CreateItemAsync(jsonConfig);
                        Console.WriteLine($"Config {appType}_{docType} created successfully");
                    }
                }
                catch (JsonReaderException exception)
                {
                    Console.WriteLine($"Exception occurred while processing JSON Config File {jsonFile}\nJSON is not valid, kindly recheck config file and confirm\n{exception.Message}\n{exception.StackTrace}");
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Exception occurred while processing JSON Config File {jsonFile}\n{exception.Message}\n{exception.StackTrace}");
                }

                Console.WriteLine("----------------");
            }
        }
    }
}

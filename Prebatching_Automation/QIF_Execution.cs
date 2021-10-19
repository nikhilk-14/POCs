using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Prebatching_Automation
{
    public class QIF_Execution
    {
        private readonly ILogger logger;

        public QIF_Execution(ILogger log)
        {
            logger = log;
        }

        public async Task<bool> Execute()
        {
            logger.Information($"QIF_Execution started...");

            var status = await ExecuteProcess();
            if (status)
            {
                logger.Information("Execution completed successfully");
            }
            else
            {
                logger.Error("Execution completed with errors, need to rerun process");
                status = await ExecuteProcess();
            }

            logger.Information($"QIF_Execution finished... Success - {status}");

            return status;
        }

        private async Task<bool> ExecuteProcess()
        {
            var status = true;

            logger.Information("Start: HttpTrigger function app for Prebatching QIF");
            var uniqueKey = await HttpTriggerCall();
            logger.Information("Finish: HttpTrigger function app for Prebatching QIF");

            if (!string.IsNullOrWhiteSpace(uniqueKey))
            {
                var waitTimeBetweenContinuousCalls = Convert.ToInt32(ConfigurationManager.AppSettings.Get("WaitTime_Between_Continuous_Calls")); // in seconds
                var maxCompletionCount = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Max_Completion_Count"));

                var timespan = "PT4H";
                var query = "get_prebatching_qif_execution_status";

                logger.Information($"Waiting for {60 * 5} seconds before getting status from App Insights");
                Thread.Sleep(60 * 10 * 1000);

                var isExecutionCompleted = await HttpTrigger_Execution_Status_From_AppInsights(waitTimeBetweenContinuousCalls, maxCompletionCount, timespan, query, uniqueKey);

                if (isExecutionCompleted)
                {
                    logger.Information($"Waiting for {waitTimeBetweenContinuousCalls} seconds");
                    Thread.Sleep(waitTimeBetweenContinuousCalls * 1000);
                    // check if execution is completed with success or failed status

                    status = await HttpTrigger_Success_Status_From_AppInsights(timespan, query, uniqueKey);
                }
                else
                {
                    status = false;
                    logger.Error($"Error occurred during http trigger status from app insights");
                }
            }
            else
            {
                status = false;
                logger.Error($"Error occurred during http trigger call");
            }

            return status;
        }

        private async Task<string> HttpTriggerCall()
        {
            string uniqueKey = null;
            var url = ConfigurationManager.AppSettings.Get("QIF_Http_Trigger");

            var retailerId = Convert.ToInt32(ConfigurationManager.AppSettings.Get("RetailerId"));
            var environment = ConfigurationManager.AppSettings.Get("Environment");

            var input = new HttpTriggerInputModel()
            {
                RetailerId = retailerId,
                Environment = environment
            };
            var inputJson = JsonConvert.SerializeObject(input);

            var response = await Utilities.PostApiCall(url, inputJson);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                uniqueKey = JsonConvert.DeserializeObject<JObject>(content)["id"].ToString();
            }
            else
            {
                logger.Error($"Error executing API {url} {response.StatusCode} - {response.ReasonPhrase}");
            }

            return uniqueKey;
        }

        private async Task<AppInsightsData> ApiCall_AppInsights(string timespan, string query)
        {
            var appInsightsApi = ConfigurationManager.AppSettings.Get("AppInsights_Api");
            var appInsightsId = ConfigurationManager.AppSettings.Get("AppInsights_Id");
            var appInsightsKey = ConfigurationManager.AppSettings.Get("AppInsights_Key");

            var url = $"{appInsightsApi}{appInsightsId}/query?timespan={timespan}&query={query}";

            var headers = new Dictionary<string, string>();
            headers.Add("x-api-key", appInsightsKey);

            var response = await Utilities.GetApiCall(url, headers);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var data = Utilities.ReadAppInsightsData(result);
                return data;
            }
            else
            {
                logger.Error($"Error executing API {url} {response.StatusCode} - {response.ReasonPhrase}");
                return null;
            }
        }

        private async Task<bool> HttpTrigger_Execution_Status_From_AppInsights(int waitTimeBetweenContinuousCalls, int maxCompletedCounter, string timespan, string query, string uniqueKey)
        {
            var status = true;
            var completionCounter = 0;
            var apiCallCounter = 0;

            while (true)
            {
                apiCallCounter += 1;
                logger.Information("Getting Data for execution status from App Insights");
                var appInsightsData = await ApiCall_AppInsights(timespan, query);

                if (appInsightsData != null && appInsightsData.data != null && appInsightsData.data.Any())
                {
                    var uniqueKeyRow = appInsightsData.data.FirstOrDefault(item => item.Any(subItem => string.Equals(subItem.name, "uniqueKey", StringComparison.OrdinalIgnoreCase) && string.Equals(subItem.value, uniqueKey, StringComparison.OrdinalIgnoreCase)));

                    var result = uniqueKeyRow?.FirstOrDefault(item => string.Equals(item.name, "executionStatus", StringComparison.OrdinalIgnoreCase));
                    if (result != null)
                    {
                        if (string.Equals(result.value, "completed", StringComparison.OrdinalIgnoreCase))
                        {
                            completionCounter += 1;

                            if (completionCounter >= maxCompletedCounter)
                            {
                                logger.Information($"Execution status received as {result.value} {completionCounter} times, which is same as max counter {maxCompletedCounter}, hence finishing execution");
                                status = true;
                                break;
                            }
                            else
                            {
                                logger.Information($"Execution status received as {result.value} {completionCounter} times, which is less than max counter {maxCompletedCounter} required");
                            }
                        }
                        else
                        {
                            logger.Information($"Execution status received as {result.value}");
                        }
                    }
                    else
                    {
                        logger.Information("No data received from App Insights");
                    }
                }
                else
                {
                    logger.Information("No data received from App Insights");
                    // add logic to check maximum no. of times data not received from App Insights (if required)
                    status = false;
                }

                logger.Information($"Waiting for {waitTimeBetweenContinuousCalls} seconds");
                Thread.Sleep(waitTimeBetweenContinuousCalls * 1000);
            }

            return status;
        }

        private async Task<bool> HttpTrigger_Success_Status_From_AppInsights(string timespan, string query, string uniqueKey)
        {
            var status = true;

            logger.Information("Getting Data for success status from App Insights");
            var appInsightsData = await ApiCall_AppInsights(timespan, query);

            if (appInsightsData != null && appInsightsData.data != null && appInsightsData.data.Any())
            {
                var uniqueKeyRow = appInsightsData.data.FirstOrDefault(item => item.Any(subItem => string.Equals(subItem.name, "uniqueKey", StringComparison.OrdinalIgnoreCase) && string.Equals(subItem.value, uniqueKey, StringComparison.OrdinalIgnoreCase)));

                var result = uniqueKeyRow?.FirstOrDefault(item => string.Equals(item.name, "successStatus", StringComparison.OrdinalIgnoreCase));
                if (result != null && string.Equals(result.value, "success", StringComparison.OrdinalIgnoreCase))
                {
                    logger.Information($"Success status received as {result.value}");
                    status = true;
                }
                else
                {
                    logger.Information($"Success status received as {result.value}");
                    status = false;
                }
            }
            else
            {
                logger.Information("Not data received from App Insights");
                // add logic to check maximum no. of times data not received from App Insights (if required)
                status = false;
            }

            return status;
        }
    }
}

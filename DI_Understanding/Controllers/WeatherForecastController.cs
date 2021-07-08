using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DI_Understanding.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DI_Understanding.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        private readonly ISingleton _singleton1;
        private readonly ISingleton _singleton2;

        private readonly ITransient _transient1;
        private readonly ITransient _transient2;

        private readonly IScoped _scoped1;
        private readonly IScoped _scoped2;

        private readonly TestClass_Singleton _testClassSingleton1;
        private readonly TestClass_Singleton _testClassSingleton2;

        private readonly TestClass_Transient _testClassTransient1;
        private readonly TestClass_Transient _testClassTransient2;

        private readonly TestClass_Scoped _testClassScoped1;
        private readonly TestClass_Scoped _testClassScoped2;

        private readonly TestClass_Singleton2 _testClassSingleton2_2;

        private readonly List<TestClassList> _testClassLists;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, ISingleton singleton1, ISingleton singleton2, ITransient transient1, ITransient transient2, IScoped scoped1, IScoped scoped2, TestClass_Singleton testClass_Singleton1, TestClass_Singleton testClass_Singleton2, TestClass_Transient testClass_Transient1, TestClass_Transient testClass_Transient2, TestClass_Scoped testClass_Scoped1, TestClass_Scoped testClass_Scoped2, TestClass_Singleton2 testClass_Singleton2_2, List<TestClassList> testClassLists)
        {
            _logger = logger;

            _singleton1 = singleton1;
            _singleton2 = singleton2;

            _transient1 = transient1;
            _transient2 = transient2;

            _scoped1 = scoped1;
            _scoped2 = scoped2;

            _testClassSingleton1 = testClass_Singleton1;
            _testClassSingleton2 = testClass_Singleton2;

            _testClassTransient1 = testClass_Transient1;
            _testClassTransient2 = testClass_Transient2;

            _testClassScoped1 = testClass_Scoped1;
            _testClassScoped2 = testClass_Scoped2;

            _testClassSingleton2_2 = testClass_Singleton2_2;

            _testClassLists = testClassLists;
            //_testClassLists = new TestClassList().GetList();
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet]
        [Route("test_di")]
        public string Test_DI()
        {
            var result = new JObject();

            result.Add("_singleton1", _singleton1.GetOperationId());
            result.Add("_singleton2", _singleton2.GetOperationId());

            result.Add("_transient1", _transient1.GetOperationId());
            result.Add("_transient2", _transient2.GetOperationId());

            result.Add("_scoped1", _scoped1.GetOperationId());
            result.Add("_scoped2", _scoped2.GetOperationId());

            result.Add("_testClassSingleton1", _testClassSingleton1.GetOperationId());
            result.Add("_testClassSingleton2", _testClassSingleton2.GetOperationId());

            result.Add("_testClassTransient1", _testClassTransient1.GetOperationId());
            result.Add("_testClassTransient2", _testClassTransient2.GetOperationId());

            result.Add("_testClassScoped1", _testClassScoped1.GetOperationId());
            result.Add("_testClassScoped2", _testClassScoped2.GetOperationId());

            result.Add("_testClassSingleton2_2", _testClassSingleton2_2.GetOperationId());

            return JsonConvert.SerializeObject(result);
        }

        [HttpGet]
        [Route("test_di_list")]
        public string Test_DI_List()
        {
            //var result = new List<TestClassList>()
            //{
            //    _testClassLists.GetData(),
            //    _testClassLists.GetData()
            //};

            return JsonConvert.SerializeObject(_testClassLists);
        }

        [HttpGet]
        [Route("test_di_list_2")]
        public string Test_DI_List_2()
        {
            //var result = new List<TestClassList>()
            //{
            //    _testClassLists.GetData(),
            //    _testClassLists.GetData()
            //};

            return JsonConvert.SerializeObject(new TestClassList().GetList_2());
        }
    }
}

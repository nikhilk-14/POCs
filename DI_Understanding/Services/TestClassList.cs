using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DI_Understanding.Services
{
    public class TestClassList
    {
        public Guid id;
        public string name;

        public TestClassList()
        {
            id = Guid.NewGuid();
            name = $"abc_{new Random().Next()}";
        }

        public TestClassList GetData()
        {
            return new TestClassList()
            {
                id = this.id,
                name = this.name
            };
        }

        public List<TestClassList> GetList()
        {
            var result = new List<TestClassList>();
            result.Add(new TestClassList().GetData());
            result.Add(new TestClassList().GetData());
            return result;
        }

        public List<TestClassList> GetList_2()
        {
            var result = new List<TestClassList>();
            result.Add(new TestClassList().GetData());
            result.Add(new TestClassList().GetData());
            return result;
        }
    }
}

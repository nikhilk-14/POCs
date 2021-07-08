using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DI_Understanding.Services
{
    public interface ISingleton
    {
        Guid GetOperationId();
    }

    public interface ITransient
    {
        Guid GetOperationId();
    }

    public interface IScoped
    {
        Guid GetOperationId();
    }

    public class SingletonTransientScoped : ISingleton, ITransient, IScoped
    {
        Guid id;
        public SingletonTransientScoped()
        {
            id = Guid.NewGuid();
        }

        Guid ISingleton.GetOperationId()
        {
            return id;
        }

        Guid ITransient.GetOperationId()
        {
            return id;
        }

        Guid IScoped.GetOperationId()
        {
            return id;
        }
    }

    public class TestClass_Singleton
    {
        Guid id;
        public TestClass_Singleton()
        {
            id = Guid.NewGuid();
        }

        public TestClass_Singleton(Guid guid)
        {
            id = guid;
        }

        public Guid GetOperationId()
        {
            return id;
        }
    }

    public class TestClass_Transient
    {
        Guid id;
        public TestClass_Transient()
        {
            id = Guid.NewGuid();
        }

        public Guid GetOperationId()
        {
            return id;
        }
    }

    public class TestClass_Scoped
    {
        Guid id;
        public TestClass_Scoped()
        {
            id = Guid.NewGuid();
        }

        public Guid GetOperationId()
        {
            return id;
        }
    }

    public class TestClass_Singleton2
    {
        Guid id;

        public TestClass_Singleton2(Guid guid)
        {
            id = guid;
        }

        public Guid GetOperationId()
        {
            return id;
        }
    }
}

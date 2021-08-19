using System;
using System.Collections.Generic;

namespace Oldsu.Bancho
{
    public class DependencyManagerBuilder
    {
        private readonly Dictionary<Type, object> _dependencies;

        public DependencyManagerBuilder() =>
            _dependencies = new Dictionary<Type, object>();

        public DependencyManagerBuilder Add<T>(T dependency) where T : notnull
        {
            _dependencies.Add(typeof(T), dependency);
            return this;
        }

        public DependencyManagerBuilder Remove<T>(T dependency)
        {
            _dependencies.Remove(typeof(T));
            return this;
        }

        public DependencyManager Build() => new DependencyManager(_dependencies);
    }
    
    public class DependencyManager
    {
        private IReadOnlyDictionary<Type, object> _dependencies;

        internal DependencyManager(IReadOnlyDictionary<Type, object> dependencies)
        {
            _dependencies = dependencies;
        }

        public T Get<T>() => (T) _dependencies[typeof(T)];

    }
}
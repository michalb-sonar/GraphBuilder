using System.Collections.Generic;

namespace CodeGraph.VS.Progression
{
    /// <summary>
    /// Maintains a mapping between pairs of objects
    /// </summary>
    public class ObjectMapper<TSource, TTarget>
    {
        private readonly Dictionary<TSource, TTarget> sourceToTargetMap;

        public ObjectMapper()
        {
            this.sourceToTargetMap = new Dictionary<TSource, TTarget>();
        }

        public void Register(TSource source, TTarget target)
        {
            this.sourceToTargetMap.Add(source, target);
        }

        public TTarget TryGetTarget(TSource source)
        {
            TTarget target;
            this.sourceToTargetMap.TryGetValue(source, out target);
            return target;
        }

        public T TryGetTarget<T>(TSource source) where T: class, TTarget
        {
            TTarget target;
            this.sourceToTargetMap.TryGetValue(source, out target);
            T t = target as T;
            return t;
        }

    }
}

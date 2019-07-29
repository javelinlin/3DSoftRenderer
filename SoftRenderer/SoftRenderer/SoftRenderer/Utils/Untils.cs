// jave.lin 2019.07.22
using System;
using System.Collections.Generic;

namespace SoftRenderer.SoftRenderer.Utils
{
    public class Pool<T> : IDisposable where T : IDisposable, new()
    {
        private int max;
        private int maxMinusOne;
        private List<T> pool;

        public int Count { get; private set; }

        public Pool(int max)
        {
            this.max = max;
            this.maxMinusOne = max - 1;
            pool = new List<T>(max);
            for (int i = 0; i < max; i++)
            {
                pool.Add(default(T));
            }
        }

        public Object Get()
        {
            if (Count > 0)
            {
                var result = pool[Count - 1];
                --Count;
                return result;
            }
            else
            {
                return Activator.CreateInstance(typeof(T));
                //return new T();
            }
        }

        public void To(T instance)
        {
            if (Count > maxMinusOne)
            {
                instance.Dispose();
                return;
            }
            pool[Count++] = instance;
        }

        public void Dispose()
        {
            if (pool != null)
            {
                foreach (var item in pool)
                {
                    item.Dispose();
                }
                pool.Clear();
                pool = null;
            }
        }
    }
}

using System;
using System.Threading.Tasks;

namespace TeCLI
{
    public struct ReturnType<T0, T1>
    {
        public T0 ItemT0 { get; private set; } = default!;
        public T1 ItemT1 { get; private set; } = default!;

        private int Value { get; set; } = -1;

        private ReturnType(T0 t)
        {
            ItemT0 = t;
            Value = 0;
        }

        private ReturnType(T1 t)
        {
            ItemT1 = t;
            Value = 1;
        }

        public static implicit operator T0(ReturnType<T0, T1> d) => d.ItemT0;
        public static explicit operator ReturnType<T0, T1>(T0 t) => new(t);

        public static implicit operator T1(ReturnType<T0, T1> d) => d.ItemT1;
        public static explicit operator ReturnType<T0, T1>(T1 t) => new(t);

        public Task<TReturn> Map<TReturn>(Func<T0, Task<TReturn>> func0, Func<T1, Task<TReturn>> func1)
        {
            if (Value == 0)
            {
                return func0(ItemT0);
            }
            else if (Value == 1)
            {
                return func1(ItemT1);
            }

            return Task.FromResult<TReturn>(default!);
        }


        public TReturn Map<TReturn>(Func<T0, TReturn> func0, Func<T1, TReturn> func1)
        {
            if (Value == 0)
            {
                return func0(ItemT0);
            }
            else if (Value == 1)
            {
                return func1(ItemT1);
            }

            return default!;
        }

        public Task Switch(Func<T0, Task> func0, Func<T1, Task> func1)
        {
            if (Value == 0)
            {
                return func0(ItemT0);
            }
            else if (Value == 1)
            {
                return func1(ItemT1);
            }

            return Task.CompletedTask;
        }

        public void Switch(Action<T0> func0, Action<T1> func1)
        {
            if (Value == 0)
            {
                func0(ItemT0);
            }
            else if (Value == 1)
            {
                func1(ItemT1);
            }
        }
    }
}

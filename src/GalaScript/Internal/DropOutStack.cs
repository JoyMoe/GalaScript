using System;
using System.Collections.Generic;
using GalaScript.Interfaces;

namespace GalaScript.Internal
{
    internal class DropOutStack<T> : LinkedList<T>, IDropOutStack<T>
    {
        private readonly int _capacity;

        public DropOutStack(int capacity)
        {
            _capacity = capacity;
        }

        public void Push(T item)
        {
            if (Count >= _capacity)
            {
                RemoveLast();
            }

            AddFirst(item);
        }

        public T Peek()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Stack empty.");
            }

            var item = First;

            return item.Value;
        }

        public T Pop()
        {
            var item = Peek();

            RemoveFirst();

            return item;
        }
    }
}

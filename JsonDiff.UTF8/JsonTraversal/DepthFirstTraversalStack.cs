using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace JsonDiff.UTF8.JsonTraversal
{
    class DepthFirstTraversalStack<T>
    {
        readonly Stack<T> _stack = new();
        readonly Stack<T> _reverseOrder = new();
        
        public void Push(T element)
        {
            _stack.Push(element);
        }

        public void PushReversed(Func<IEnumerable<T>> getItemsToPush)
        {
            foreach (var item in getItemsToPush())
            {
                _reverseOrder.Push(item);
            }
            while (_reverseOrder.Count > 0)
            {
                _stack.Push(_reverseOrder.Pop());
            }
        }
        
        public int Count => _stack.Count;

        public T Pop()
        {
            if (!TryPop(out var result))
            {
                throw new InvalidOperationException("QueueStack is empty");
            }

            return result!;
        }
        
        public bool TryPop([MaybeNullWhen(false)]out T result)
        {
            return _stack.TryPop(out result);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace JsonDiff.UTF8.JsonTraversal
{
    class DepthFirstTraversalStack<T>
    {
        readonly Stack<T> _stack = new();
        readonly Stack<T> _reverseOrder = new();
        bool _reversed;
        
        public void Push(T element)
        {
            if (_reversed)
            {
                _reverseOrder.Push(element);
            }
            else
            {
                _stack.Push(element);                
            }
        }

        public IDisposable ReverseOrder()
        {
            if (_reversed)
            {
                throw new InvalidOperationException("Order already reversed");
            }
            return new ReverseOrderContext(this);
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
        
        class ReverseOrderContext : IDisposable
        {
            readonly DepthFirstTraversalStack<T> _depthFirstTraversalStack;

            public ReverseOrderContext(DepthFirstTraversalStack<T> depthFirstTraversalStack)
            {
                _depthFirstTraversalStack = depthFirstTraversalStack;
                _depthFirstTraversalStack._reversed = true;
            }

            public void Dispose()
            {
                _depthFirstTraversalStack._reversed = false;
                while (_depthFirstTraversalStack._reverseOrder.Count > 0)
                {
                    _depthFirstTraversalStack._stack.Push(_depthFirstTraversalStack._reverseOrder.Pop());
                }
            }
        }
    }
}
using System;

namespace Microsoft.Xna.Framework.Input
{
    public class Stack<T>
    {
        T[] stack;

        public int Capacity { get { return stack.Length; } }

        public int Count { get; private set; }

        public Stack()
            : this( 0x20 ) { }

        public Stack( int capacity )
        {
            if ( capacity < 0 )
                capacity = 0;
            //
            stack = new T[capacity];
        }

        public void Push( ref T item )
        {
            if ( Count == stack.Length )
            {
                T[] tmp = new T[stack.Length << 1];
                Array.Copy( stack, 0, tmp, 0, stack.Length );
                stack = tmp;
            }
            stack[Count] = item;
            ++Count;
        }

        public void Pop( out T item )
        {
            if ( !(Count > 0) )
                throw new InvalidOperationException();
            //
            item = stack[Count];
            stack[Count] = default( T );
            --Count;
        }

        public void PopSegment( out ArraySegment<T> segment )
        {
            segment = new ArraySegment<T>( stack, 0, Count );
            Count = 0;
        }
    }
}

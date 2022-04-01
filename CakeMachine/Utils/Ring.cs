namespace CakeMachine.Utils
{
    internal class Ring<T>
    {
        private readonly Queue<T> _elements;

        public Ring(IEnumerable<T> elements)
        {
            _elements = new Queue<T>(elements);
        }

        public T Next
        {
            get
            {
                lock(_elements)
                {
                    var element = _elements.Dequeue();
                    _elements.Enqueue(element);
                    return element;
                }
            }
        }
    }
}

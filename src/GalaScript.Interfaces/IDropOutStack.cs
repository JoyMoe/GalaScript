namespace GalaScript.Interfaces
{
    public interface IDropOutStack<T>
    {
        void Push(T item);

        T Peek();

        T Pop();
    }
}

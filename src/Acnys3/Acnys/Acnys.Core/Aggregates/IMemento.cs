namespace Acnys.Core.Aggregates
{
    public interface IMemento<T>
    {
        T GetState();
        void SetState(T state);
    }
}

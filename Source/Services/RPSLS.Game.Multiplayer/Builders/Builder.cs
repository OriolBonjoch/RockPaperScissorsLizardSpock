namespace RPSLS.Game.Multiplayer.Builders
{
    public class Builder<T> where T : new()
    {
        protected T _product;
        public Builder()
        {
            _product = new T();
        }

        public T Build() => _product;
    }
}

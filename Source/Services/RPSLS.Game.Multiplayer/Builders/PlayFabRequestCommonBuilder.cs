using PlayFab.Internal;

namespace RPSLS.Game.Multiplayer.Builders
{
    public class PlayFabRequestCommonBuilder<T, U> : Builder<U>
        where T : PlayFabRequestCommonBuilder<T, U>
        where U : PlayFabRequestCommon, new()
    {
        public T WithTitleContext(string title, string token)
        {
            _product.AuthenticationContext = new PlayFab.PlayFabAuthenticationContext()
            {
                EntityType = "title",
                EntityId = title,
                EntityToken = token
            };

            return this as T;
        }
    }
}

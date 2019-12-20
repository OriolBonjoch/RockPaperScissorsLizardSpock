using PlayFab.AuthenticationModels;

namespace RPSLS.Game.Multiplayer.Builders
{
    public class ValidateEntityTokenRequestBuilder : PlayFabRequestCommonBuilder<ValidateEntityTokenRequestBuilder, ValidateEntityTokenRequest>
    {
        public ValidateEntityTokenRequestBuilder WithToken(string token)
        {
            _product.EntityToken = token;
            return this;
        }
    }
}

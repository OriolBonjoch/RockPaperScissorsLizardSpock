using PlayFab.ClientModels;

namespace RPSLS.Game.Multiplayer.Builders
{
    public class LoginWithCustomIDRequestBuilder : PlayFabRequestCommonBuilder<LoginWithCustomIDRequestBuilder, LoginWithCustomIDRequest>
    {
        public LoginWithCustomIDRequestBuilder WithUser(string username)
        {
            _product.CustomId = username;
            return this;
        }

        public LoginWithCustomIDRequestBuilder CreateIfDoesntExist()
        {
            _product.CreateAccount = true;
            return this;
        }

        public LoginWithCustomIDRequestBuilder WithAccountInfo()
        {
            _product.InfoRequestParameters.GetUserAccountInfo = true;
            return this;
        }
    }
}

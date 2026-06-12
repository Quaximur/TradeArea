using R3;

namespace TradeArea.Data
{
    public class UserModel
    {
        public ReactiveProperty<string> OnNameChanged;
        public ReactiveProperty<string> OnTokenChanged;
        public ReactiveProperty<string> OnUserIdChanged;

        public UserModel(UserState state)
        {
            OnNameChanged = new(state.Name);
            OnTokenChanged = new(state.Token);
            OnUserIdChanged = new(state.UserId);
        }
    }
}
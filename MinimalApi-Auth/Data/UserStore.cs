using MinimalApi_Auth.Models;

namespace MinimalApi_Auth.Data

{
    public class UserStore
    {
        private readonly List<User> _users = new();

        public bool TryAdd(User user)
        {
            if (_users.Any(u => u.Username == user.Username))
                return false;

            _users.Add(user);
            return true;
        }

        public User? FindByUsername(string username)
        {
            return _users.FirstOrDefault(u => u.Username == username);
        }

        public void Update(User user)
        {
            var index = _users.FindIndex(u => u.Id == user.Id);
            if (index >= 0) _users[index] = user;
        }
    }
}

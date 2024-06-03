namespace Dating_App_Backend.Helper
{
    public static class CommonMethod
    {
        public static string GetGroupName(string user1, string user2)
        {
            var flag = string.CompareOrdinal(user1, user2) < 0;

            return flag ? $"{user1}-{user2}" : $"{user2}-{user1}";
        }
        public static bool IsSingleChat(string groupName)
        {
            var users = groupName.Split('-');
            return users.Length == 2;
        }
        public static bool IsPartFromSingleChat(string groupName, string username)
        {
            var users = groupName.Split("-");
            return users.Length == 2 && (users[0] == username || users[1] == username);
        }
        public static string GetRecipientUserName(string groupName, string username)
        {
            var users = groupName.Split("-");
            return users[0] == username ? users[1] : users[0];
        }
    }
}

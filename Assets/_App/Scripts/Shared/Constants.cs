namespace TradeArea.Shared
{
    public static class Constants
    {
        public const string AUCY_URL = "http://localhost:8765";
        public const string GAMEPLAY_START_SIGNAL = nameof(GAMEPLAY_START_SIGNAL);
        public const string GAMEPLAY_RESTART_SIGNAL = nameof(GAMEPLAY_RESTART_SIGNAL);
        public const string GAMEPLAY_EXIT_SIGNAL = nameof(GAMEPLAY_EXIT_SIGNAL);

        public const int UserIndex1 = 0;
        public const int UserIndex2 = 1;

        public static string[] UserTokens = { TokenUser1, TokenUser2 };
        public static string[] UserIds = { UserId1, UserId2 };

        private const string TokenUser1 = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMWMyZjJmNzEtYWEwZi00NmYxLTgzOWYtOTc2NGEzNGI0Zjc2IiwiZXhwIjoxNzgyMDgxNDA5fQ.knK_gXwXwSt88rVhoXh_VlKDLuTpgL8D9FJLlYgfyHI";
        private const string TokenUser2 = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMmMyZjJmNzEtYWEwZi00NmYxLTgzOWYtOTc2NGEzNGI0Zjc2IiwiZXhwIjoxNzgxMjQ1MjMxfQ.2uX07jRLRns9g3cvXrMO5_jtmU1zzaKakRbEyhTSOS8";
        private const string UserId1 = "1c2f2f71-aa0f-46f1-839f-9764a34b4f76";
        private const string UserId2 = "2c2f2f71-aa0f-46f1-839f-9764a34b4f76";
    }
}
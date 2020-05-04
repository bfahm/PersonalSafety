namespace PersonalSafety.Contracts
{
    public static class ApiRoutes
    {
        private const string Base = "api";
        private const string DefaultBasic = Base + "/[controller]/";
        private const string TrailingAction = "/[action]";
        
        public const string Default = Base + "/[controller]/[action]";

        public static class Account
        {
            public const string PersonnelBasicInfo = "~/" + Base + "/[controller]/Personnel/[action]";
        }

        public static class Admin
        {
            public const string Technical = DefaultBasic + nameof(Technical) + TrailingAction;
            public const string Registration = DefaultBasic + nameof(Registration) +TrailingAction;
            public const string Management = DefaultBasic + nameof(Management) +TrailingAction;
        }

        public static class Agent
        {
            public const string Department = DefaultBasic + nameof(Department) + TrailingAction;
            public const string Resucer = DefaultBasic + nameof(Resucer) + TrailingAction;
            public const string SOS = DefaultBasic + nameof(SOS) + TrailingAction;
        }

        public static class Client
        {
            public const string Registration = DefaultBasic + nameof(Registration) + TrailingAction;
            public const string SOS = DefaultBasic + nameof(SOS) + TrailingAction;
            public const string Events = DefaultBasic + nameof(Events) + TrailingAction;
        }

        public static class Manager
        {
            public const string Departments = DefaultBasic + nameof(Departments) + TrailingAction;
            public const string Categories = DefaultBasic + nameof(Categories) + TrailingAction;
        }
    }
}

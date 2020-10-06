namespace AMStock.DAL
{
    public static class GenericMessages
    {
        public static readonly string DatabaseErrorGeneric = "There was a problem in the database. Please contact your system administrator.";
        public static readonly string DatabaseErrorRecordAlreadyExists = "Record already exists.";
        public static readonly string DatabaseErrorRecordAlreadyInUse = "Record is already in use...";
        public static readonly string DatabaseErrorAuthorizationDenied = "Authorization denied.";
        public static readonly string DatabaseNoRows = "No rows returned.";
        public static readonly string DatabaseParameterNotFound = "Parameter not found.";
        public static readonly string DatabaseErrorNoRowsAffected = "No rows affected.";

        public static readonly string DataSuccessfullyAdded = "Data Successfully Added";
        public static readonly string DataSuccessfullyUpdated = "Data Successfully Updated";

        public static readonly string InvalidArgument = "Invalid Argument";
        public static readonly string UnauthorizedAccess = "Unauthorized Access.";
        public static readonly string InvalidId = "Invalid Id.";
        public static readonly string StringIsNullOrEmpty = " is null or empty.";
        public static readonly string ObjectIsNull = "Object is null.";

        public static readonly string AuthenticationLoginFailed = "Login failed.";
        public static readonly string AuthenticationLoginDisabled = "Your account is disabled.";
        public static readonly string AuthenticationGenericError = "There was a problem with Authentication.";

        public static readonly string Authentication_InvalidSingleSignOn = "Invalid Single Sign On.";
        public static readonly string Authentication_NullMasterCredentials = "MasterCredentials was null.";
        public static readonly string Authentication_InvalidMasterCredentials = "Invalid MasterCredentials.";

    }
}
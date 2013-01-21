namespace ermeX.Tests.AcceptanceTester.Helpers.Db
{
    public  interface ISqlServerInfo
    {
        string GetConnectionString(string dbName);
    }
}
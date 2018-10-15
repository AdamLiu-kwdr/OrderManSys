
//ConnectionString option POCO class
//will be registred in startup.

namespace OrderManSys
{
    public class ConnectionStringOption
    {
        public string Factory {get; set;} //Database connection string
        public string AutoManSys {get;set;} //Automan api address
    }
}
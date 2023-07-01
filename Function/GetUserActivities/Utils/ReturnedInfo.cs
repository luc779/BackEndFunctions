namespace GetUserActivitiesUtil
{
    public class ReturnedInfo
    {
        public List<dynamic> Transports { get; }
        public List<dynamic> Foods { get; }
        public List<dynamic> Utilities { get; }
        public ReturnedInfo(List<dynamic> transports, List<dynamic> foods, List<dynamic> utilities)
        {
            Transports = transports;
            Foods = foods;
            Utilities = utilities;
        }
    }
}
namespace GetEmissionHistoryUtils
{
    public static class ConvertToDateTime
    {
        public static DateTime Convert(DailyEmission e)
        {
        return e.DateTime;
        }
    }
}
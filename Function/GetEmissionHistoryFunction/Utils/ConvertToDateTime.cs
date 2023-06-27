using GetEmissionHistoryUtils;

namespace BackEndFucntions.Function.GetEmissionHistoryFunction.Utils
{
    public static class ConvertToDateTime
    {
        public static DateTime Convert(DailyEmission e)
        {
        return e.DateTime;
        }
    }
}
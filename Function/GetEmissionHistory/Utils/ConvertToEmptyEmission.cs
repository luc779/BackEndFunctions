namespace GetEmissionHistoryUtils
{
    public static class ConvertToEmptyEmission
    {
        private const double EMISSION_GOAL = 60.4;
        public static DailyEmission Convert(DateTime date) {
            return new DailyEmission
                {
                DateTime = date,
                Total = null,
                Goal = EMISSION_GOAL
                };
            }
    }
}
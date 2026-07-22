namespace Gymble.Services
{
    public static class MembershipDatePolicy
    {
        public static DateTime CalculatePeriodEndDate(DateTime startDate, int durationDays)
        {
            if (durationDays < 1)
                throw new ArgumentOutOfRangeException(nameof(durationDays), "이용 기간은 1일 이상이어야 합니다.");

            return startDate.Date.AddDays(durationDays - 1);
        }
    }
}

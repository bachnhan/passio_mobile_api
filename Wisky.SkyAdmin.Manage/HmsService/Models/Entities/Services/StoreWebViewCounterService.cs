using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{

    public partial interface IStoreWebViewCounterService
    {
        Task<StoreWebViewCounter> GetAndIncreaseAsync(int storeId);
        Task<StoreWebViewCounter> GetCounter(int storeId);
    }

    public partial class StoreWebViewCounterService
    {

        public async Task<StoreWebViewCounter> GetAndIncreaseAsync(int storeId)
        {
            var counter = await this.FirstOrDefaultAsync(q => q.StoreId == storeId);
            if (counter == null)
            {
                counter = new StoreWebViewCounter
                {
                    StoreId = storeId,
                    LastUpdate = Utils.GetCurrentDateTime(),
                };
                this.Create(counter);
            }
            var now = Utils.GetCurrentDateTime();
            var isDifferentYear = now.Year != counter.LastUpdate.Year;
            var isDifferentMonth = isDifferentYear || now.Month != counter.LastUpdate.Month;

            CultureInfo cul = CultureInfo.CurrentCulture;

            int weekNum1 = cul.Calendar.GetWeekOfYear(
                now,
                CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);

            int weekNum2 = cul.Calendar.GetWeekOfYear(
                counter.LastUpdate,
                CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);

            var isDifferentWeek = weekNum1 != weekNum2;
            var isDifferentDay = now.Date != counter.LastUpdate.Date;

            if (isDifferentYear)
            {
                counter.YearlyViewCount = 1;
            }
            else
            {
                counter.YearlyViewCount++;
            }
            if (isDifferentMonth)
            {
                counter.MonthlyViewCount = 1;
            }
            else
            {
                counter.MonthlyViewCount++;
            }



            if (isDifferentWeek)
            {
                counter.WeeklyViewCount = 1;
            }
            else
            {
                counter.WeeklyViewCount++;
            }

            if (isDifferentDay)
            {
                counter.TodayViewCount = 1;
            }
            else
            {
                counter.TodayViewCount++;
            }
            counter.TotalViewCount++;

            counter.LastUpdate = now;

            await this.SaveAsync();
            return counter;
        }

        public async Task<StoreWebViewCounter> GetCounter(int storeId)
        {
            var counter = await this.FirstOrDefaultAsync(q => q.StoreId == storeId);
            if (counter == null)
            {
                counter = new StoreWebViewCounter
                {
                    StoreId = storeId,
                    LastUpdate = DateTime.Now
                };
                this.Create(counter);
            }
            await this.SaveAsync();
            return counter;
        }

    }
}

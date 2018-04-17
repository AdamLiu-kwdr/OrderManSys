using System;
using System.Collections.Generic;
using System.Linq;
using OrderManSys.Model;

//This Engine will accept Orders passed in and create Schedules based on conditions.

namespace Scheduling_Engine.Sequence
{
    public class SequenceEngine
    {
        public IList<Schedule> CreateSchedule(IList<Orders> CurrentOrders) //Main function, accepting orders.
        {
            CurrentOrders = CurrentOrders.OrderBy(O => O.FinishTime).ToList();
            var ScheduleList = new List<Schedule>();
            foreach (var ThisOrder in CurrentOrders)
            {
                ScheduleList.Add(new Schedule{
                    Id = CurrentOrders.IndexOf(ThisOrder)+1,
                    Quantity = ThisOrder.Quantity,
                    OrderId = ThisOrder.Id,
                    Working = false,
                    Orders = ThisOrder
                });
            }

            ScheduleList.First().Working = true;

            return ScheduleList;
        }
    }
}
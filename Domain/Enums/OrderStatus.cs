using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 0,
        Preparing = 1,
        Ready = 2,
        Delivered = 3,
        Cancelled = 4
    }
}

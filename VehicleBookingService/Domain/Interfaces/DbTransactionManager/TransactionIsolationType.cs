using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.DbTransactionManager
{
    public enum TransactionIsolationType
    {
        ReadUncommitted,
        ReadCommitted,
        RepeatableRead,
        Serializable
    }
}

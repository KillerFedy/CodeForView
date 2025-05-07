using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectDatabase.Common
{
    public class BaseModelWithDeletedProperty : BaseModel
    {
        public bool IsDeleted { get; set; }
    }
}

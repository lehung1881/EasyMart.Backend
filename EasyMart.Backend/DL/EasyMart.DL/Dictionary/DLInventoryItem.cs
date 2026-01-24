using BASE.Service.Core.Services;
using EasyMart.DLBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMart.DL.Dictionary
{
    public class DLInventoryItem : DLBaseEasyMart
    {
        public DLInventoryItem(IMySQLService databaseService) : base(databaseService)
        {
        }
    }
}

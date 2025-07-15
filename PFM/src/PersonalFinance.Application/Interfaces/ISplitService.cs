using PersonalFinance.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinance.Application.Interfaces
{
    public interface ISplitService
    {
        Task SplitAsync(int transactionId, IEnumerable<SingleCategorySplitDto> splits);
    }
}

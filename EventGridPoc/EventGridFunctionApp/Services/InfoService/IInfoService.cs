using ListenEventGrid.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListenEventGrid.Services.InfoService
{
    public interface IInfoService
    {
        public Task CreateInfo(CreateInfoDto info, ILogger logger);
    }
}

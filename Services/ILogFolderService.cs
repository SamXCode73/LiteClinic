using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Services
{
    public interface ILogFolderService
    {
        Task<bool> OpenLogFolderAsync();
    }
}

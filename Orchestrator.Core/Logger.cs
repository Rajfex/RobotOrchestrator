using System;
using System.Collections.Generic;
using System.Text;
using Orchestrator.Core.Data;
using Orchestrator.Core.Models;

namespace Orchestrator.Core
{
    public class Logger
    {
        private readonly AppDbContext _context;

        public Logger(AppDbContext context)
        {
            _context = context;
        }


        public void Log(Guid taskID, string message, int type)
        {
            Log logToSave = new Log
            {
                TaskId = taskID,
                Message = message,
                LogTypeId = type
            };

            _context.Logs.Add(logToSave);
            _context.SaveChanges();
        }

    }
}

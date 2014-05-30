using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Google.Apis.Calendar.v3.Data;

namespace SliceCalendarAgent.Models.Data
{
    public class Repository
    {
        private readonly SliceCalendarAgentEntities _entities;

        public Repository()
        {
            _entities = new SliceCalendarAgentEntities();
        }

        public Transaction SaveTransaction(Transaction txn)
        {
            var saved =  _entities.Transactions.Add(txn);
            _entities.SaveChanges();
            return saved;
        }

        public IList<Transaction> GetTransaction(string txn_id)
        {
            return _entities.Transactions.Where(txn => txn.txn_id == txn_id).ToList();
        }

        public ErrorLog AddErrorLog(ErrorLog errorLog)
        {
            var saved = _entities.ErrorLogs.Add(errorLog);
            _entities.SaveChanges();
            return saved;
        }

        public RawTransaction AddRawTransaction(RawTransaction rtxn)
        {
            var saved = _entities.RawTransactions.Add(rtxn);
            _entities.SaveChanges();
            return saved;
        }
    }
}
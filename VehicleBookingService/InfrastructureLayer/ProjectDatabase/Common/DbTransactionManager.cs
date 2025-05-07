using Domain.Interfaces.DbTransactionManager;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectDatabase.Common
{
    public class DbTransactionManager(Context context) : IDbTransactionManager
    {
        private IDbContextTransaction? _currentTransaction;

        /// <summary>
        /// Начинает новую транзакцию с указанным уровнем изоляции.
        /// </summary>
        public async Task BeginTransactionAsync(TransactionIsolationType isolationType 
            = TransactionIsolationType.ReadCommitted)
        {
            if (_currentTransaction == null)
            {
                var isolationLevel = MapToIsolationLevel(isolationType);
                _currentTransaction = await context.Database.BeginTransactionAsync(isolationLevel);
            }
        }

        /// <summary>
        /// Фиксирует текущую транзакцию.
        /// </summary>
        public async Task CommitTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync();
                Dispose();
            }
        }

        public void Dispose()
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }

        /// <summary>
        /// Откатывает текущую транзакцию.
        /// </summary>
        public async Task RollbackTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync();
                Dispose();
            }
        }


        /// <summary>
        /// Преобразует уровень изоляции в формат, понятный для Entity Framework Core.
        /// </summary>
        private IsolationLevel MapToIsolationLevel(TransactionIsolationType isolationType)
        {
            return isolationType switch
            {
                TransactionIsolationType.ReadUncommitted => IsolationLevel.ReadUncommitted,
                TransactionIsolationType.ReadCommitted => IsolationLevel.ReadCommitted,
                TransactionIsolationType.RepeatableRead => IsolationLevel.RepeatableRead,
                TransactionIsolationType.Serializable => IsolationLevel.Serializable,
                _ => throw new ArgumentOutOfRangeException(nameof(isolationType), 
                $"Неизвестный уровень изоляции: {isolationType}")
            };
        }
    }
}

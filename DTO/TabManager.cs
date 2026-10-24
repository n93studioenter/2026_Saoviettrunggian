using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaovietTax.DTO
{
    public class TabManager
    {
        private readonly ConcurrentQueue<string> _availableTabs = new ConcurrentQueue<string>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public void AddTab(string tabHandle)
        {
            _availableTabs.Enqueue(tabHandle);
        }

        public async Task<string> AcquireTabAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                string tab; // Khai báo biến ở ngoài vòng lặp

                // Vòng lặp sẽ chạy cho đến khi lấy được tab thành công
                while (!_availableTabs.TryDequeue(out tab))
                {
                    _semaphore.Release();
                    await Task.Delay(100);
                    await _semaphore.WaitAsync();
                }

                return tab; // Lúc này biến 'tab' đã hợp lệ
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void ReleaseTab(string tabHandle)
        {
            _availableTabs.Enqueue(tabHandle);
        }
    }
}

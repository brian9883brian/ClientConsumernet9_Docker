using System.Collections.Concurrent;
using ClientConsumerOrder.Models;
using System.Text.Json;

namespace ClientConsumerOrder.Services
{
    public class OrderStorageService
    {
        private readonly ConcurrentDictionary<Guid, OrderMessage> _orders = new();
        private readonly string _storageFile = "orders.json";

        public OrderStorageService()
        {
            LoadFromFile();
        }

        public void AddOrUpdate(OrderMessage order)
        {
            _orders.AddOrUpdate(order.Id, order, (key, old) => order);
            SaveToFile();
        }

        public void Remove(Guid id)
        {
            _orders.TryRemove(id, out _);
            SaveToFile();
        }

        public IEnumerable<OrderMessage> GetAll()
        {
            return _orders.Values;
        }

        public OrderMessage GetById(Guid id)
        {
            _orders.TryGetValue(id, out var order);
            return order;
        }

        private void LoadFromFile()
        {
            try
            {
                if (File.Exists(_storageFile))
                {
                    var json = File.ReadAllText(_storageFile);
                    var orders = JsonSerializer.Deserialize<List<OrderMessage>>(json);
                    if (orders != null)
                    {
                        foreach (var order in orders)
                        {
                            _orders[order.Id] = order;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading orders from file: {ex.Message}");
            }
        }

        private void SaveToFile()
        {
            try
            {
                var json = JsonSerializer.Serialize(_orders.Values.ToList(), new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(_storageFile, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving orders to file: {ex.Message}");
            }
        }
    }
}
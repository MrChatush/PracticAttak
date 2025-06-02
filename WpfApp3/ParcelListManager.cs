using System.Linq;
using System.Windows.Controls;
using BestDelivery;

namespace DeliveryOptimizer
{
    public class ParcelListManager
    {
        private readonly ListBox _listBox;

        public ParcelListManager(ListBox listBox)
        {
            _listBox = listBox;
        }

        public void Refresh(Order[] parcels)
        {
            _listBox.Items.Clear();
            foreach (var order in parcels)
            {
                _listBox.Items.Add(order.ID == -1
                    ? $"СКЛАД: ({order.Destination.X:F2}, {order.Destination.Y:F2})"
                    : $"Заказ #{order.ID}: ({order.Destination.X:F2}, {order.Destination.Y:F2}), Приоритет: {order.Priority:F2}");
            }
        }
    }
}
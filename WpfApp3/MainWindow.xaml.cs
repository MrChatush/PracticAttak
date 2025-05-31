using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using BestDelivery;
using DisplayPoint = System.Windows.Point;
using GeoPoint = BestDelivery.Point;

namespace DeliveryOptimizer
{
    public partial class MainWindow : Window
    {
        private Order[] activeParcels = Array.Empty<Order>();
        private GeoPoint hubLocation;
        private int[] deliveryOrder = Array.Empty<int>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OptionOneTrigger(object sender, RoutedEventArgs e) => HandleDeliveryScenario(OrderArrays.GetOrderArray1, "Центр города");
        private void OptionTwoTrigger(object sender, RoutedEventArgs e) => HandleDeliveryScenario(OrderArrays.GetOrderArray2, "Окраины");
        private void OptionThreeTrigger(object sender, RoutedEventArgs e) => HandleDeliveryScenario(OrderArrays.GetOrderArray3, "Один район");
        private void OptionFourTrigger(object sender, RoutedEventArgs e) => HandleDeliveryScenario(OrderArrays.GetOrderArray4, "Разные районы");
        private void OptionFiveTrigger(object sender, RoutedEventArgs e) => HandleDeliveryScenario(OrderArrays.GetOrderArray5, "Разные приоритеты");
        private void OptionSixTrigger(object sender, RoutedEventArgs e) => HandleDeliveryScenario(OrderArrays.GetOrderArray6, "Много заказов");
        private void HandleDeliveryScenario(Func<Order[]> fetchOrders, string description)
        {
            activeParcels = fetchOrders();
            hubLocation = activeParcels.First(o => o.ID == -1).Destination;
            deliveryOrder = CreateOptimizedRoute(activeParcels, hubLocation);
            RefreshParcelList();
            UpdateRouteInfo();
        }

        private void RefreshParcelList()
        {
            ParcelList.Items.Clear();
            foreach (var order in activeParcels)
            {
                ParcelList.Items.Add(order.ID == -1
                    ? $"СКЛАД: ({order.Destination.X:F2}, {order.Destination.Y:F2})"
                    : $"Заказ #{order.ID}: ({order.Destination.X:F2}, {order.Destination.Y:F2}), Приоритет: {order.Priority:F2}");
            }
        }

        private void UpdateRouteInfo()
        {
            if (ConfirmRouteValidity(hubLocation, activeParcels, deliveryOrder, out double routeLength))
            {
                DistanceInfo.Text = $"Расстояние: {routeLength:F2} усл. ед.";
                RouteSequence.Text = "Маршрут: " + string.Join(" → ", deliveryOrder.Select(id => id == -1 ? "СКЛАД" : "#" + id));
                DrawRoute();
            }
            else
            {
                DistanceInfo.Text = "Маршрут неккоректен";
                RouteSequence.Text = "";
                RouteCanvas.Children.Clear();
            }
        }

        private void DrawRoute()
        {
            RouteCanvas.Children.Clear();
            var positions = new Dictionary<int, DisplayPoint>();

            double margin = 50;
            double width = RouteCanvas.ActualWidth > 0 ? RouteCanvas.ActualWidth : 800;
            double height = RouteCanvas.ActualHeight > 0 ? RouteCanvas.ActualHeight : 600;

            var points = activeParcels.Select(p => p.Destination).ToList();
            double minX = points.Min(p => p.X);
            double maxX = points.Max(p => p.X);
            double minY = points.Min(p => p.Y);
            double maxY = points.Max(p => p.Y);

            double scaleX = (width - 2 * margin) / (maxX - minX);
            double scaleY = (height - 2 * margin) / (maxY - minY);
            double scale = Math.Min(scaleX, scaleY);

            double shiftX = margin - minX * scale;
            double shiftY = height - margin + minY * scale;

            DisplayPoint Map(GeoPoint p) => new DisplayPoint(p.X * scale + shiftX, shiftY - p.Y * scale);

            foreach (var order in activeParcels)
            {
                var point = Map(order.Destination);
                positions[order.ID] = point;

                var marker = new Ellipse
                {
                    Width = 12,
                    Height = 12,
                    Fill = order.ID == -1 ? Brushes.Red : Brushes.Blue,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.5
                };

                Canvas.SetLeft(marker, point.X - 6);
                Canvas.SetTop(marker, point.Y - 6);
                RouteCanvas.Children.Add(marker);
            }

            for (int i = 0; i < deliveryOrder.Length - 1; i++)
            {
                var from = positions[deliveryOrder[i]];
                var to = positions[deliveryOrder[i + 1]];
                var line = new Line
                {
                    X1 = from.X,
                    Y1 = from.Y,
                    X2 = to.X,
                    Y2 = to.Y,
                    Stroke = Brushes.DarkGreen,
                    StrokeThickness = 2
                };
                RouteCanvas.Children.Add(line);
            }
        }
        private void RouteCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(RouteCanvas);

            double margin = 50;
            double width = RouteCanvas.ActualWidth > 0 ? RouteCanvas.ActualWidth : 800;
            double height = RouteCanvas.ActualHeight > 0 ? RouteCanvas.ActualHeight : 600;

            var points = activeParcels.Select(p => p.Destination).ToList();
            double minX = points.Min(p => p.X);
            double maxX = points.Max(p => p.X);
            double minY = points.Min(p => p.Y);
            double maxY = points.Max(p => p.Y);

            double scaleX = (width - 2 * margin) / (maxX - minX);
            double scaleY = (height - 2 * margin) / (maxY - minY);
            double scale = Math.Min(scaleX, scaleY);

            double shiftX = margin - minX * scale;
            double shiftY = height - margin + minY * scale;

            double x = (pos.X - shiftX) / scale;
            double y = (shiftY - pos.Y) / scale;

            //диалоговое окно для ввода приоритета
            var dialog = new PriorityInputDialog();
            if (dialog.ShowDialog() == true)
            {
                var newId = activeParcels.Max(o => o.ID) + 1;
                var newOrder = new Order
                {
                    ID = newId,
                    Destination = new GeoPoint { X = x, Y = y },
                    Priority = dialog.Priority
                };

                var list = activeParcels.ToList();
                list.Add(newOrder);
                activeParcels = list.ToArray();

                deliveryOrder = CreateOptimizedRoute(activeParcels, hubLocation);
                RefreshParcelList();
                UpdateRouteInfo();
            }
        }
        public class PriorityInputDialog : Window
        {
            public double Priority { get; private set; } = 0.5;

            private TextBox priorityTextBox;

            public PriorityInputDialog()
            {
                Title = "Введите приоритет заказа";
                Width = 300;
                Height = 150;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
                ResizeMode = ResizeMode.NoResize;

                var stackPanel = new StackPanel { Margin = new Thickness(10) };

                var label = new Label { Content = "Приоритет заказа (0,0 - 1.0):" };
                stackPanel.Children.Add(label);

                priorityTextBox = new TextBox { Text = "0,5" };
                stackPanel.Children.Add(priorityTextBox);

                var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 10, 0, 0) };

                var okButton = new Button { Content = "Да", Width = 80, Margin = new Thickness(5) };
                okButton.Click += OkButton_Click;
                buttonPanel.Children.Add(okButton);

                var cancelButton = new Button { Content = "Отмена", Width = 80, Margin = new Thickness(5) };
                cancelButton.Click += (s, e) => { DialogResult = false; Close(); };
                buttonPanel.Children.Add(cancelButton);

                stackPanel.Children.Add(buttonPanel);

                Content = stackPanel;
            }

            private void OkButton_Click(object sender, RoutedEventArgs e)
            {
                if (double.TryParse(priorityTextBox.Text, out double priority) && priority >= 0 && priority <= 1)
                {
                    Priority = priority;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Введите число от 0,0 до 1.0", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                    priorityTextBox.Focus();
                }
            }
        }

        public static int[] CreateOptimizedRoute(Order[] parcels, GeoPoint hub)
        {
            var orders = parcels.Where(p => p.ID != -1).ToList();
            if (orders.Count == 0) return new[] { -1, -1 };

            var points = new List<GeoPoint> { hub };
            points.AddRange(orders.Select(o => o.Destination));

            int n = points.Count;
            double[,] dist = new double[n, n];

            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    dist[i, j] = i == j ? 0 : Math.Sqrt(Math.Pow(points[i].X - points[j].X, 2) + Math.Pow(points[i].Y - points[j].Y, 2));

            List<int> route = new List<int> { 0 };
            var unvisited = Enumerable.Range(1, n - 1).ToList();

            while (unvisited.Count > 0)
            {
                int last = route.Last();
                int next = unvisited.OrderBy(i => dist[last, i]).First();
                route.Add(next);
                unvisited.Remove(next);
            }

            route.Add(0);

            var result = new List<int> { -1 };
            for (int i = 1; i < route.Count - 1; i++)
                result.Add(orders[route[i] - 1].ID);
            result.Add(-1);
            return result.ToArray();
        }

        public static bool ConfirmRouteValidity(GeoPoint hub, Order[] parcels, int[] route, out double routeLength)
        {
            routeLength = 0;
            if (parcels == null || route == null || parcels.Length == 0 || route.Length == 0) return false; // Исправлено: заменены " " на "||"

            var routeList = new List<int>(route);
            if (routeList.First() != -1 || routeList.Last() != -1) return false;

            var allIds = parcels.Where(p => p.ID != -1).Select(p => p.ID).ToHashSet();
            var visited = routeList.Where(id => id != -1).ToHashSet();
            if (!allIds.SetEquals(visited)) return false;

            GeoPoint current = hub;
            foreach (var id in routeList.Skip(1))
            {
                var o = parcels.First(p => p.ID == id);
                routeLength += Math.Sqrt(Math.Pow(current.X - o.Destination.X, 2) + Math.Pow(current.Y - o.Destination.Y, 2));
                current = o.Destination;
            }
            return true;
        }
    }
}
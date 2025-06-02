using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using BestDelivery;
using DisplayPoint = System.Windows.Point; // Alias for WPF Point
using GeoPoint = BestDelivery.Point;      // Alias for BestDelivery.Point

namespace DeliveryOptimizer
{
    public class RouteRenderer
    {
        private readonly Canvas _canvas;

        public RouteRenderer(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void DrawRoute(Order[] parcels, int[] deliveryOrder)
        {
            _canvas.Children.Clear();
            var positions = new Dictionary<int, DisplayPoint>();

            double margin = 50;
            double width = _canvas.ActualWidth > 0 ? _canvas.ActualWidth : 800;
            double height = _canvas.ActualHeight > 0 ? _canvas.ActualHeight : 600;

            var points = parcels.Select(p => p.Destination).ToList();
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

            foreach (var order in parcels)
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
                _canvas.Children.Add(marker);
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
                _canvas.Children.Add(line);
            }
        }

        public Order ConvertCanvasPointToOrder(DisplayPoint canvasPoint, Order[] activeParcels, double priority)
        {
            double margin = 50;
            double width = _canvas.ActualWidth > 0 ? _canvas.ActualWidth : 800;
            double height = _canvas.ActualHeight > 0 ? _canvas.ActualHeight : 600;

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

            double x = (canvasPoint.X - shiftX) / scale;
            double y = (shiftY - canvasPoint.Y) / scale;

            return new Order
            {
                ID = activeParcels.Max(o => o.ID) + 1,
                Destination = new GeoPoint { X = x, Y = y },
                Priority = priority
            };
        }

        public void Clear()
        {
            _canvas.Children.Clear();
        }
    }
}
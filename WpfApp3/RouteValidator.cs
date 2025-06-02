using System;
using System.Collections.Generic;
using System.Linq;
using BestDelivery;

namespace DeliveryOptimizer
{
    public static class RouteValidator
    {
        public static bool ConfirmRouteValidity(BestDelivery.Point hub, Order[] parcels, int[] route, out double routeLength)
        {
            routeLength = 0;
            if (parcels == null || route == null || parcels.Length == 0 || route.Length == 0)
                return false;

            var routeList = new List<int>(route);
            if (routeList.First() != -1 || routeList.Last() != -1)
                return false;

            var allIds = parcels.Where(p => p.ID != -1).Select(p => p.ID).ToHashSet();
            var visited = routeList.Where(id => id != -1).ToHashSet();
            if (!allIds.SetEquals(visited))
                return false;

            BestDelivery.Point current = hub;
            foreach (var id in routeList.Skip(1))
            {
                var o = parcels.First(p => p.ID == id);
                routeLength += Math.Sqrt(
                    Math.Pow(current.X - o.Destination.X, 2) +
                    Math.Pow(current.Y - o.Destination.Y, 2));
                current = o.Destination;
            }
            return true;
        }
    }
}
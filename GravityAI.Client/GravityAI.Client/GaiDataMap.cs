using GravityAI.Client.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GravityAI.Client
{
    public class GaiDataMap
    {
        private readonly Dictionary<string, GaiPathMappingModel> _map = new Dictionary<string, GaiPathMappingModel>();


        public void MapDataValue(string source, string destination, object defaultValue = null)
        {
            if (string.IsNullOrEmpty(source))
                throw new ArgumentException("Source cannot be null or empty", nameof(source));
            if (string.IsNullOrEmpty(destination))
                throw new ArgumentException("Destination cannot be null or empty", nameof(destination));
            var mapping = new GaiPathMappingModel
            {
                Source = source,
                Destination = destination,
                DefaultValue = defaultValue?.ToString()
            };
            _map[source] = mapping;
        }

        public bool RemoveMap(string source)
        {
            if (string.IsNullOrEmpty(source))
                throw new ArgumentException("Source cannot be null or empty", nameof(source));
            return _map.Remove(source);
        }

        internal List<GaiPathMappingModel> GetMap() {
            return new List<GaiPathMappingModel>(_map.Values);
        }

    }
}

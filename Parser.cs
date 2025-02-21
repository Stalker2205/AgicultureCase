using AgroCase.Model;
using System.Globalization;
using System.Xml.Linq;

namespace AgroCase
{
    public class Parser
    {
        /// <summary>
        /// Распарсирование файлов
        /// </summary>
        /// <param name="fieldsPath">Путь до файла полей</param>
        /// <param name="centroidsPath">Путь до файла центров полей</param>
        /// <returns></returns>
        public static List<Field> ParseFields(string fieldsPath, string centroidsPath)
        {
            var fieldsList = new List<Field>();

            var xdocFields = XDocument.Load(fieldsPath);
            var xdocCentroids = XDocument.Load(centroidsPath);
            var namespaceUri = "http://www.opengis.net/kml/2.2";
            var fields = xdocFields.Descendants(XName.Get("Placemark", namespaceUri)).ToList();
            var centroids = xdocCentroids.Descendants(XName.Get("Placemark", namespaceUri))
                .Select(c => new
                {
                    Id = int.TryParse(c.Descendants(XName.Get("SimpleData", namespaceUri))
                                      .FirstOrDefault(s => (string)s.Attribute("name") == "fid")?.Value, out var id) ? id : 0,
                    Center = c.Descendants(XName.Get("coordinates", namespaceUri)).FirstOrDefault()?.Value.Trim()
                })
                .ToDictionary(c => c.Id, c => c.Center);

            foreach (var field in fields)
            {
                var idString = field.Descendants(XName.Get("SimpleData", namespaceUri))
                                    .FirstOrDefault(s => (string)s.Attribute("name") == "fid")?.Value;
                int id = 0;
                int.TryParse(idString, out id);

                var name = (string)field.Element(XName.Get("name", namespaceUri));

                var sizeString = field.Descendants(XName.Get("SimpleData", namespaceUri))
                                      .FirstOrDefault(s => (string)s.Attribute("name") == "size")?.Value;
                double size = 0;
                double.TryParse(sizeString, out size);

                var coordinates = field.Descendants(XName.Get("coordinates", namespaceUri)).FirstOrDefault()?.Value.Trim();

                var polygonCoordinates = coordinates?.Split(' ')
                    .Select(coord =>
                    {
                        var parts = coord.Split(',');
                        double latitude = 0, longitude = 0;

                        double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out latitude);  // lat
                        double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out longitude); // lng

                        return new List<double> { latitude, longitude };
                    }).ToList();

                string centerCoordinates = centroids.ContainsKey(id) ? centroids[id] : null;

                List<double>? center = null;
                if (!string.IsNullOrEmpty(centerCoordinates))
                {
                    var parts = centerCoordinates.Split(',');
                    center = new List<double>
            {
                double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double lat) ? lat : 0,  // lat
                double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double lng) ? lng : 0   // lng
            };
                }

                fieldsList.Add(new Field
                {
                    Id = id,
                    Name = name,
                    Size = size,
                    Locations = new AgroCase.Model.Location
                    {
                        Polygon = polygonCoordinates,
                        Center = center
                    }
                });
            }

            return fieldsList;
        }

    }
}

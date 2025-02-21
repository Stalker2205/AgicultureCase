using NetTopologySuite.Geometries;
using Microsoft.AspNetCore.Mvc;
using AgroCase.Model;

namespace AgroCase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FieldController : ControllerBase
    {
        private List<Field> fields;

        public FieldController()
        {
            fields = Parser.ParseFields("Data/fields.kml", "Data/centroids.kml");
        }

        /// <summary>
        /// Получение всех элементов fields с полями
        /// </summary>
        /// <returns>Возвращает все данные о полях</returns>
        [HttpGet("all")]
        public IActionResult GetAllFields()
        {
            return Ok(fields);
        }

        /// <summary>
        /// Получение площади поля
        /// </summary>
        /// <param name="id"> Уникальный идентификатор поля</param>
        /// <returns>Возвращает площадь поля, если она указана</returns>
        /// <returns>Возвращает NotFound, если не указана площадь поля</returns>
        [HttpGet("{id}/size")]
        public IActionResult GetFieldSize(int id)
        {
            var field = fields.FirstOrDefault(f => f.Id == id);
            if (field == null)
                return NotFound();

            return Ok(field.Size);
        }

        /// <summary>
        /// Поиск удаленности от центра поля
        /// </summary>
        /// <param name="id">Уникальный идентификатор поля</param>
        /// <param name="lat">Широта, от которой нужно найти удаление</param>
        /// <param name="lng">Долгота, от которой нужно найти удаление</param>
        /// <returns>Расстояние в метрах между центром поля и точной</returns>
        /// 
        [HttpGet("{id}/distance")]
        public IActionResult GetDistance(int id, [FromQuery] double lat, [FromQuery] double lng)
        {
            var field = fields.FirstOrDefault(f => f.Id == id);
            if (field == null)
                return NotFound();

            var fieldCenter = field.Locations.Center;
            if (fieldCenter == null)
                return BadRequest("Центр поля не указан");

            double distance = Haversine.HaversineDistance(lat, lng, fieldCenter[0], fieldCenter[1]);
            return Ok(distance); // расстояние в метрах
        }

       

        /// <summary>
        /// Получение принадлежности точки к полям 
        /// </summary>
        /// <param name="lat">Широта точки, принадлежность которой необходимо вычислить</param>
        /// <param name="lng">Долгота точки, принадлежность которой необходимо вычислить</param>
        /// <returns>Возвращает id и name поля, если точка внутри контура одного из полей</returns>
        /// <returns>Возвращает false, если точка не принадлежит к полям</returns>
        [HttpGet("point")]
        public IActionResult CheckPoint([FromQuery] double lat, [FromQuery] double lng)
        {
            var point = new Point(new Coordinate(lng, lat));

            foreach (var field in fields)
            {
                var coordinates = field.Locations.Polygon.Select(coord => new Coordinate(coord[0], coord[1])).ToArray();
                var polygon = new Polygon(new LinearRing(coordinates));

                if (polygon.Contains(point))
                {
                    return Ok(new { id = field.Id, name = field.Name });
                }
            }

            return Ok(false);
        }
        

    }
}

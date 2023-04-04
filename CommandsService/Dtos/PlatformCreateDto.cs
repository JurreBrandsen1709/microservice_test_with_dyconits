using System.ComponentModel.DataAnnotations;

namespace CommandsService.Dtos
{
    public class PlatformCreateDto
    {
        public int id { get; set; }
        [Required]
        public string name { get; set; }

        [Required]
        public string publisher { get; set; }

        [Required]
        public string cost { get; set; }
    }
}

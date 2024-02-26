using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CkasTool_MVVM.Models
{
    public class Carla
    {
        public string? stamp { get; set; }
        public string[]? linear_acceleration { get; set; }
        public string[]? angular_velocity { get; set; }
        public string[]? position { get; set; }
        public string[]? orientation { get; set; }
        public string? velocity { get; set; }
    }
}

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
        public string? linear_acceleration_x { get; set; }
        public string? linear_acceleration_y { get; set; }
        public string? linear_acceleration_z { get; set; }
        public string? angular_velocity_x { get; set; }
        public string? angular_velocity_y { get; set; }
        public string? angular_velocity_z { get; set; }
        public string? orientation_roll { get; set; }
        public string? orientation_pitch { get; set; }
        public string? orientation_yaw { get; set; }
    }
}

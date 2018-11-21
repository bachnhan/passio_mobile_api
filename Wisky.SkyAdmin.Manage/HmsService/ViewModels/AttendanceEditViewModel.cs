using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
   public class AttendanceEditViewModel
    {
        public int? stt { get; set; }
        public string updatePerson { get; set; }
        public string checkMin { get; set; }
        public string checkMax { get; set; }
        public string shiftMin { get; set; }
        public string shiftMax { get; set; }
        public string totalWorkTime { get; set; }
        public string note { get; set; }
        public int? processStt { get; set; }
        public int AttendanceId { get; set; }
    }
}

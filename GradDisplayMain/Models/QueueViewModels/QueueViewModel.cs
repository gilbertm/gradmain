using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GradDisplayMain.Models.QueueViewModels
{
    public class QueueViewModel
    {

        [Key]
        [Required]
        [Display(Name = "ID")]
        public string GraduateId { get; set; }

        [Display(Name = "Scanner ID")]
        public string GraduateScannerId { get; set; }

        public int Status { get; set; }

        public int Arabic { get; set; }

        public string School { get; set; }

        public string Program { get; set; }

        public string Major { get; set; }

        public string Merit { get; set; }

        [Display(Name = "English Full Name")]
        public string Fullname { get; set; }

        [Display(Name = "Arabic Full Name")]
        public string ArabicFullname { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        public DateTime Created { get; set; }
    }
}